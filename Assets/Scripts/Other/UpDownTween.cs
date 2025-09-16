using UnityEngine;
using DG.Tweening;

public class UpDownTween : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 2f;   // kitna upar niche jaye
    public float duration = 1f;       // ek side ka time (upar ya niche)
    public Ease easeType = Ease.InOutSine; // smoothness

    private void Start()
    {
        // object ko upar niche loop me move karwate hain
        transform.DOMoveY(transform.position.y + moveDistance, duration)
            .SetEase(easeType)
            .SetLoops(-1, LoopType.Yoyo); // -1 = infinite, Yoyo = upar niche
    }
}
