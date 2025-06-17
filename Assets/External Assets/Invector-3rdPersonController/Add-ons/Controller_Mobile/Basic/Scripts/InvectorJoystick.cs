using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class InvectorFloatingJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum AxisOption { Both, OnlyHorizontal, OnlyVertical }

    public int MovementRange = 100;
    [Range(0f, 1f)]
    public float deadzone = 0.1f; // Deadzone radius (0-1)
    [Range(0f, 1f)]
    public float minInputValue = 0.2f; // Minimum input value after deadzone (0-1)
    [Range(0f, 1f)]
    public float maxInputValue = 1f; // Maximum input value (0-1)
    public AxisOption axesToUse = AxisOption.Both;
    public string horizontalAxisName = "Horizontal";
    public string verticalAxisName = "Vertical";
    public bool alwaysVisible = false; // New field to control joystick visibility

    public RectTransform joystickBackground; // assign in Inspector
    public RectTransform joystickHandle;     // assign in Inspector

    private Vector2 startPosition;
    private bool useX, useY;
    private CrossPlatformInputManager.VirtualAxis horizontalVirtualAxis;
    private CrossPlatformInputManager.VirtualAxis verticalVirtualAxis;

    private void Start()
    {
        CreateVirtualAxes();
        joystickBackground.gameObject.SetActive(alwaysVisible); // Show if alwaysVisible is true
    }

    private void CreateVirtualAxes()
    {
        useX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
        useY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

        if (useX && !CrossPlatformInputManager.AxisExists(horizontalAxisName))
        {
            horizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(horizontalVirtualAxis);
        }
        else if (useX)
        {
            horizontalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName);
        }

        if (useY && !CrossPlatformInputManager.AxisExists(verticalAxisName))
        {
            verticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(verticalVirtualAxis);
        }
        else if (useY)
        {
            verticalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(verticalAxisName);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startPosition = eventData.position;
        joystickBackground.position = eventData.position;
        joystickBackground.gameObject.SetActive(true);
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - startPosition;
        delta = Vector2.ClampMagnitude(delta, MovementRange);
        joystickHandle.anchoredPosition = delta;

        Vector2 normalized = delta / MovementRange;
        
        // Apply deadzone
        float magnitude = normalized.magnitude;
        if (magnitude < deadzone)
        {
            normalized = Vector2.zero;
        }
        else
        {
            // Remap the values from deadzone to 1 to minInputValue to maxInputValue
            float remappedMagnitude = Mathf.Lerp(minInputValue, maxInputValue, 
                (magnitude - deadzone) / (1f - deadzone));
            normalized = normalized.normalized * remappedMagnitude;
        }

        if (useX) horizontalVirtualAxis.Update(normalized.x);
        if (useY) verticalVirtualAxis.Update(normalized.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!alwaysVisible)
        {
            joystickBackground.gameObject.SetActive(false);
        }
        joystickHandle.anchoredPosition = Vector2.zero;

        if (useX) horizontalVirtualAxis.Update(0);
        if (useY) verticalVirtualAxis.Update(0);
    }
}
