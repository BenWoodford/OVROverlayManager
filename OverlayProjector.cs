using UnityEngine;
using System.Collections;
using Valve.VR;
using System.Collections.Generic;

public class OverlayProjector : MonoBehaviour {

    public enum OverlayType {
        Dashboard,
        InGame
    }

    public static OverlayProjector Instance;

    ulong overlayHandle = 0;
    ulong thumbnailHandle = 0;
    Texture_t overlayTexture;
    Texture_t thumbTexture;
    public RenderTexture overlayRenderTexture;

    bool appliedIcon = false;

    EVROverlayError overlayError = EVROverlayError.None;

    VREvent_t vrEvent;
    uint eventSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREvent_t));

    OverlayInputState _inputState;

    public OverlayInputState InputState
    {
        get
        {
            return _inputState;
        }
    }

    public Canvas overlayCanvas;
    public OverlayType overlayType;
    public string overlayKey = "testOverlay";
    public string overlayFriendlyName = "TEST";
    float _overlayWidth = 2.0f;
    
    public float OverlayWidth
    {
        get { return _overlayWidth; }
        set
        {
            _overlayWidth = value;

            if(overlayHandle > 0)
                OverlayManager.Instance.OverlayFactory.SetOverlayWidthInMeters(overlayHandle, value);
        }
    }

    public string iconPath = "";

    void SetupOverlay()
    {
        if (overlayType == OverlayType.Dashboard)
            overlayError = OverlayManager.Instance.OverlayFactory.CreateDashboardOverlay(overlayKey, overlayFriendlyName, ref overlayHandle, ref thumbnailHandle);
        else
            overlayError = OverlayManager.Instance.OverlayFactory.CreateOverlay(overlayKey, overlayFriendlyName, ref overlayHandle);

        if (overlayError != EVROverlayError.None)
        {
            Debug.LogError(overlayError);
            enabled = false;
            return;
        }

        OverlayManager.Instance.OverlayFactory.SetOverlayWidthInMeters(overlayHandle, _overlayWidth);

        if(overlayType == OverlayType.Dashboard)
            OverlayManager.Instance.OverlayFactory.SetOverlayInputMethod(overlayHandle, VROverlayInputMethod.Mouse);

        if (SystemInfo.graphicsDeviceVersion.Contains("OpenGL"))
        {
            thumbTexture.eType = EGraphicsAPIConvention.API_OpenGL;
            overlayTexture.eType = EGraphicsAPIConvention.API_OpenGL;
        }
        else
        {
            thumbTexture.eType = EGraphicsAPIConvention.API_DirectX;
            overlayTexture.eType = EGraphicsAPIConvention.API_DirectX;

            foreach (Transform child in overlayCanvas.transform)
            {
                child.localScale = new Vector3(child.localScale.x, -child.localScale.y, child.localScale.z);
            }
        }
    }

    // Use this for initialization
    void Start () {
        SetupOverlay();
    }
	
	// Update is called once per frame
	void Update () {
        _inputState.PressCount = _inputState.ReleaseCount = 0;
        _inputState.IsPressedLastFrame = _inputState.IsPressed;

        if (OverlayManager.Instance.OverlayFactory.IsOverlayVisible(overlayHandle))
        {
            OverlayManager.Instance.ActiveProjector = this;

            if (overlayType == OverlayType.Dashboard)
            {
                overlayCanvas.enabled = true;
            }

            if (!appliedIcon && overlayType == OverlayType.Dashboard)
            {
                ApplyIcon();
            }

            overlayTexture.handle = overlayRenderTexture.GetNativeTexturePtr();
            overlayError = OverlayManager.Instance.OverlayFactory.SetOverlayTexture(overlayHandle, ref overlayTexture);

            OverlayManager.Instance.OverlayFactory.SetOverlayAutoCurveDistanceRangeInMeters(overlayHandle, 1f, 3f);

            if (overlayError != EVROverlayError.None)
            {
                Debug.LogError(overlayError);
                //enabled = false;
                return;
            }

            while (OverlayManager.Instance.OverlayFactory.PollNextOverlayEvent(overlayHandle, ref vrEvent, eventSize))
            {
                switch ((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREvent_MouseMove:
                        _inputState.PointerPosition = new Vector2(vrEvent.data.mouse.x * overlayRenderTexture.width, overlayRenderTexture.height - (vrEvent.data.mouse.y * overlayRenderTexture.height));
                        break;
                    case EVREventType.VREvent_MouseButtonDown:
                        _inputState.PressCount++;
                        _inputState.IsPressed = true;
                        break;
                    case EVREventType.VREvent_MouseButtonUp:
                        _inputState.ReleaseCount++;
                        _inputState.IsPressed = false;
                        break;
                    default:
                        break;
                }
            }
        } else
        {
            if(overlayType == OverlayType.Dashboard)
            {
                overlayCanvas.enabled = false;
            }

            if(OverlayManager.Instance.ActiveProjector == this)
            {
                OverlayManager.Instance.ActiveProjector = null;
            }
        }
    }

    void ApplyIcon()
    {
        if (overlayType != OverlayType.Dashboard || iconPath == "")
        {
            appliedIcon = true;
            return;
        }

        overlayError = OverlayManager.Instance.OverlayFactory.SetOverlayTexture(thumbnailHandle, ref thumbTexture);

        if (overlayError != EVROverlayError.None)
        {
            Debug.Log("Failed to apply icon... give up.");
        }

        appliedIcon = true;
    }

    void OnApplicationQuit()
    {
        OverlayManager.Instance.OverlayFactory.DestroyOverlay(overlayHandle);
    }
}