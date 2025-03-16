using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;


public class LongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject targetPanel; // Jo panel activate karna hai
    private Coroutine pressCoroutine;
    private bool isPressing = false;


    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        pressCoroutine = StartCoroutine(LongPressRoutine());
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
        }
    }
    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(3f); // 3 seconds wait
        if (isPressing)
        {
            targetPanel.SetActive(true); // Panel activate ho jaye
        }
    }
}