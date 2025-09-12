using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageLine : MonoBehaviour
{
    #region Singleton

    public static MessageLine Instance;

    private void Awake()
    {
        Instance = this;
    }

    #endregion

    public TextMeshProUGUI messageText;

    public void ShowMessage(string message, Color? color = null)
    {
        if (!this.GetComponent<Image>().enabled)
        {
            this.GetComponent<Image>().enabled = true;
        }
        this.transform.DOScale(1, 0.2f);
        if (color == null) color = Color.white;

        messageText.color = color.Value;
        messageText.text = message;
        Invoke(nameof(HideMessage), 1f);
    }
    public void HideMessage()
    {
        this.transform.DOScale(0, 0.2f);
    }
}
