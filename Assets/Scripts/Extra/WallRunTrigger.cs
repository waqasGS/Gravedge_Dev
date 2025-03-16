using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WallRunTrigger : MonoBehaviour/*, IPointerDownHandler, IPointerUpHandler*/
{
    //public static event Action IsWallRun; // Proper event declaration
    //public static bool IsWallRunEnabled { get; private set; } // Encapsulated setter

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    IsWallRunEnabled = true;
    //    IsWallRun?.Invoke(); // Invoke event when wall run starts
    //}

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    IsWallRunEnabled = false;
    //    IsWallRun?.Invoke(); // Invoke event when wall run stops (in case other scripts need it)
    //}
}
