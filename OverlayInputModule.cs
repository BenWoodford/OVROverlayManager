using UnityEngine;
using System.Collections;
using System;
using UnityEngine.EventSystems;

public class OverlayInputModule : StandaloneInputModule
{
    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();

        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent)
                usedEvent |= SendMoveEventToSelectedObject();

            if (!usedEvent)
                SendSubmitEventToSelectedObject();
        }

        ProcessViveMouseEvent();
    }

    private readonly MouseState m_MouseState = new MouseState();

    protected override MouseState GetMousePointerEventData()
    {
        // Populate the left button...
        PointerEventData leftData;
        var created = GetPointerData(kMouseLeftId, out leftData, true);

        leftData.Reset();

        if(OverlayManager.Instance == null || OverlayManager.Instance.ActiveProjector == null)
        {
            m_MouseState.SetButtonState(PointerEventData.InputButton.Left, PointerEventData.FramePressState.NotChanged, leftData);
            return m_MouseState;
        }

        if (created)
            leftData.position = OverlayManager.Instance.ActiveProjector.InputState.PointerPosition;

        Vector2 pos = OverlayManager.Instance.ActiveProjector.InputState.PointerPosition;
        leftData.delta = pos - leftData.position;
        leftData.position = pos;
        leftData.scrollDelta = Vector2.zero; // TODO: Pick up touch pad scroll
        leftData.button = PointerEventData.InputButton.Left;
        eventSystem.RaycastAll(leftData, m_RaycastResultCache);
        var raycast = FindFirstRaycast(m_RaycastResultCache);
        leftData.pointerCurrentRaycast = raycast;
        m_RaycastResultCache.Clear();

        // copy the apropriate data into right and middle slots
        PointerEventData rightData;
        GetPointerData(kMouseRightId, out rightData, true);
        CopyFromTo(leftData, rightData);
        rightData.button = PointerEventData.InputButton.Right;

        PointerEventData middleData;
        GetPointerData(kMouseMiddleId, out middleData, true);
        CopyFromTo(leftData, middleData);
        middleData.button = PointerEventData.InputButton.Middle;

        m_MouseState.SetButtonState(PointerEventData.InputButton.Left, CalculateTriggerState(), leftData);
        //m_MouseState.SetButtonState(PointerEventData.InputButton.Right, StateForMouseButton(1), rightData);
        //m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, StateForMouseButton(2), middleData);

        return m_MouseState;
    }

    PointerEventData.FramePressState CalculateTriggerState()
    {
        if(OverlayManager.Instance.ActiveProjector.InputState.IsPressed == OverlayManager.Instance.ActiveProjector.InputState.IsPressedLastFrame)
        {
            return PointerEventData.FramePressState.NotChanged;
        }

        if(OverlayManager.Instance.ActiveProjector.InputState.IsPressed)
        {
            return PointerEventData.FramePressState.Pressed;
        }

        // Guess it's not pressed then.
        if(OverlayManager.Instance.ActiveProjector.InputState.PressCount > 0)
        {
            return PointerEventData.FramePressState.PressedAndReleased;
        } else
        {
            return PointerEventData.FramePressState.Released;
        }
    }


    /// <summary>
    /// Process all mouse events.
    /// </summary>
    private void ProcessViveMouseEvent()
    {
        var mouseData = GetMousePointerEventData();

        var pressed = mouseData.AnyPressesThisFrame();
        var released = mouseData.AnyReleasesThisFrame();

        var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

        if (!UseMouse(pressed, released, leftButtonData.buttonData))
            return;

        // Process the first mouse button fully
        ProcessMousePress(leftButtonData);
        ProcessMove(leftButtonData.buttonData);
        ProcessDrag(leftButtonData.buttonData);

        // Now process right / middle clicks
        /*ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
        ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
        ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);*/

        if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f))
        {
            var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
            ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
        }
    }

    private static bool UseMouse(bool pressed, bool released, PointerEventData pointerData)
    {
        if (pressed || released || pointerData.IsPointerMoving() || pointerData.IsScrolling())
            return true;

        return false;
    }
}
