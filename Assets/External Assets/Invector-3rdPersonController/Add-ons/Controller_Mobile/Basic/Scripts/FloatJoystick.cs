using UnityEngine;
using UnityEngine.EventSystems;

public class FloatJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform background;
    public RectTransform handle;
    public float handleRange = 100f;

    private Vector2 input = Vector2.zero;
    private Vector2 startPos;
    private bool isTouching = false;

    void Start()
    {
        background.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        background.position = eventData.position;
        background.gameObject.SetActive(true);
        isTouching = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isTouching) return;

        Vector2 direction = eventData.position - (Vector2)background.position;
        float distance = Mathf.Min(direction.magnitude, handleRange);
        input = direction.normalized * (distance / handleRange);
        handle.anchoredPosition = input * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        background.gameObject.SetActive(false);
        isTouching = false;
    }

    public Vector2 Direction => input;
}
