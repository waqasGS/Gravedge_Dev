using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class InvectorFloatingJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum AxisOption { Both, OnlyHorizontal, OnlyVertical }

    public int MovementRange = 100;
    public AxisOption axesToUse = AxisOption.Both;
    public string horizontalAxisName = "Horizontal";
    public string verticalAxisName = "Vertical";

    public RectTransform joystickBackground; // assign in Inspector
    public RectTransform joystickHandle;     // assign in Inspector

    private Vector2 startPosition;
    private bool useX, useY;
    private CrossPlatformInputManager.VirtualAxis horizontalVirtualAxis;
    private CrossPlatformInputManager.VirtualAxis verticalVirtualAxis;

    private void Start()
    {
        CreateVirtualAxes();
        joystickBackground.gameObject.SetActive(false); // start hidden
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
        if (useX) horizontalVirtualAxis.Update(normalized.x);
        if (useY) verticalVirtualAxis.Update(normalized.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickBackground.gameObject.SetActive(false);
        joystickHandle.anchoredPosition = Vector2.zero;

        if (useX) horizontalVirtualAxis.Update(0);
        if (useY) verticalVirtualAxis.Update(0);
    }
}
