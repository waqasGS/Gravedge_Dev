﻿using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class InvectorJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public enum AxisOption
    {
        // Options for which axes to use
        Both, // Use both
        OnlyHorizontal, // Only horizontal
        OnlyVertical // Only vertical
    }

    public int MovementRange = 100;
    public AxisOption axesToUse = AxisOption.Both; // The options for the axes that the still will use
    public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
    public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

    //public string pressButtonName;
    Vector3 m_StartPos;
    bool m_UseX; // Toggle for using the x axis
    bool m_UseY; // Toggle for using the Y axis
    CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
    CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

    IEnumerator Start()
    {
        m_StartPos = transform.localPosition;
        yield return new WaitForEndOfFrame();

        CreateVirtualAxes();
    }

    void UpdateVirtualAxes(Vector3 value)
    {
        var delta = m_StartPos - value;
        delta.y = -delta.y;
        delta /= MovementRange;
        if (m_UseX)
        {
            m_HorizontalVirtualAxis.Update(-delta.x);
        }

        if (m_UseY)
        {
            m_VerticalVirtualAxis.Update(delta.y);
        }
    }

    void CreateVirtualAxes()
    {
        // set axes to use
        m_UseX = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal);
        m_UseY = (axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical);

        // create new axes based on axes to use
        if (m_UseX)
        {
            if (CrossPlatformInputManager.AxisExists(horizontalAxisName))
            {
                m_HorizontalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(horizontalAxisName);
            }
            else
            {
                m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            }


        }
        if (m_UseY)
        {
            if (CrossPlatformInputManager.AxisExists(verticalAxisName))
            {
                m_VerticalVirtualAxis = CrossPlatformInputManager.VirtualAxisReference(verticalAxisName);
            }
            else
            {
                m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
                CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
            }
        }
    }

    public void OnDrag(PointerEventData data)
    {
        Vector3 newPos = Vector3.zero;

        if (m_UseX)
        {
            int delta = (int)(data.position.x - m_StartPos.x);
            //delta = Mathf.Clamp(delta, - MovementRange, MovementRange);
            newPos.x = delta;
        }

        if (m_UseY)
        {
            int delta = (int)(data.position.y - m_StartPos.y);
            //delta = Mathf.Clamp(delta, -MovementRange, MovementRange);
            newPos.y = delta;
        }

        // change to clamp on a circular area instead of a square area
        transform.localPosition = Vector3.ClampMagnitude(transform.parent.InverseTransformPoint(new Vector3(newPos.x, newPos.y, newPos.z)), MovementRange) + m_StartPos;
        //print("transform.localPosition:"+ transform.localPosition);
        UpdateVirtualAxes(transform.localPosition);
    }

    public void OnPointerUp(PointerEventData data)
    {        
        transform.localPosition = m_StartPos;
        UpdateVirtualAxes(m_StartPos);

        //if (!string.IsNullOrEmpty(pressButtonName))
        //{
        //    CrossPlatformInputManager.SetButtonUp(pressButtonName);
        //}


        // print("OnPointerUp");

        
    }

    public void OnPointerDown(PointerEventData data)
    {
        //if (!string.IsNullOrEmpty(pressButtonName))
        //{
        //    CrossPlatformInputManager.SetButtonDown(pressButtonName);
        //}

       // print("OnPointerDown");
    }
}