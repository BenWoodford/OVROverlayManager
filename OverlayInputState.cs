using UnityEngine;

public struct OverlayInputState {
    public uint PressCount;
    public uint ReleaseCount;
    public bool IsPressed;
    public bool IsPressedLastFrame;
    public Vector2 PointerPosition;
}