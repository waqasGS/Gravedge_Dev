using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        if (color == null) color = Color.white;
        
        messageText.color = color.Value;
        messageText.text = message;
    }
}
