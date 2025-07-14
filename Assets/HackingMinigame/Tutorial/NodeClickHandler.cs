using UnityEngine;
using UnityEngine.EventSystems;

public class NodeClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Notify the tutorial system when this node is clicked
        if (HackingMinigame.Instance != null)
        {
            HackingMinigame.Instance.OnNodeClickedForTutorial(gameObject);
        }
    }
} 