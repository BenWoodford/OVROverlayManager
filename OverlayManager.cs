using UnityEngine;
using System.Collections;
using Valve.VR;

public class OverlayManager : MonoBehaviour {

    SteamVR _vr;
    EVRInitError error = EVRInitError.None;
    CVRSystem _hmd;
    CVRCompositor _compositor;
    CVROverlay _overlay;

    static OverlayManager _instance;

    public OverlayProjector ActiveProjector;

    public static OverlayManager Instance
    {
        get { return _instance; }
    }

    public CVRSystem HMD
    {
        get { return _hmd; }
    }

    public CVRCompositor Compositor
    {
        get { return _compositor; }
    }

    public CVROverlay OverlayFactory
    {
        get { return _overlay; }
    }

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 90;
        _instance = this;
        _vr = SteamVR.instance;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

        if (error != EVRInitError.None)
        {
            Debug.LogError("Error initialising OpenVR");
            enabled = false;
            return;
        }

        OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);

        if (error != EVRInitError.None)
        {
            Debug.LogError("Error initialising Compositor");
            enabled = false;
            return;
        }

        OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);

        if (error != EVRInitError.None)
        {
            Debug.LogError("Error initialising Overlay");
            enabled = false;
            return;
        }

        _hmd = OpenVR.System;
        _compositor = OpenVR.Compositor;
        _overlay = OpenVR.Overlay;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
