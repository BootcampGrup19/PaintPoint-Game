using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPaintColor : MonoBehaviour
{
    public static Color CurrentPaintColor = Color.red;

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) CurrentPaintColor = Color.red;
        if (Keyboard.current.digit2Key.wasPressedThisFrame) CurrentPaintColor = Color.blue;
        if (Keyboard.current.digit3Key.wasPressedThisFrame) CurrentPaintColor = Color.green;
        if (Keyboard.current.digit4Key.wasPressedThisFrame) CurrentPaintColor = Color.yellow;
    }
}