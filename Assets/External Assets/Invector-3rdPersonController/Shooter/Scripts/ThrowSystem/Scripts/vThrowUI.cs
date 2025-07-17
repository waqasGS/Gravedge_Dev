using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.Throw
{
    public class vThrowUI : MonoBehaviour
    {
        public Text maxThrowCount;
        public Text currentThrowCount;
        public Image display;
        public Button gernadeButton;
        internal virtual void UpdateCount(vThrowManagerBase throwManager, bool showMaxAmount = true)
        {
            UnityEngine.Color32 color = display.color;
            //selectedGunImage.color = equipColor;
            if (throwManager.CurrentThrowAmount == 0)
            {
                color.a = 20;
                gernadeButton.interactable = false;
            }
            else
            {
                color.a = 255;
                gernadeButton.interactable = true;
            }
            display.color = color;
            if (currentThrowCount) currentThrowCount.text = throwManager.CurrentThrowAmount.ToString();
            if (maxThrowCount) maxThrowCount.text = showMaxAmount ? throwManager.MaxThrowObjects.ToString() : "";
            if (display) display.sprite = throwManager.CurrentThrowableSprite;
        }
    }
}