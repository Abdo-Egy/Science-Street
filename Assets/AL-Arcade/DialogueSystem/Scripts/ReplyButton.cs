using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ALArcade.ArabicTMP;
namespace AL_Arcade.DialogueSystem.Scripts
{
    public class ReplyButton : MonoBehaviour
    {
        [Header("UI Elements")]
        public ArabicTextMeshProUGUI replyText;
        public Button button;
        public Image backgroundImage;
    
        [Header("Visual Feedback")]
        public Color normalColor = Color.white;
        public Color hoverColor = Color.gray;
        public Color pressedColor = Color.gray;
    
        private System.Action onClickAction;
    
        public void Setup(DialogueReply reply, System.Action onClick)
        {
            if (reply == null) return;
        
            if (replyText != null)
                replyText.arabicText = reply.replyText;
        
            onClickAction = onClick;
        
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClickAction?.Invoke());
            }
        }
    
        public void OnPointerEnter()
        {
            if (backgroundImage != null)
                backgroundImage.color = hoverColor;
        }
    
        public void OnPointerExit()
        {
            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }
    
        public void OnPointerDown()
        {
            if (backgroundImage != null)
                backgroundImage.color = pressedColor;
        }
    }

}