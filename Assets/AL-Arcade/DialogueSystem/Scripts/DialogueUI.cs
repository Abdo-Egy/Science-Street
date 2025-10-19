// ============================================
// File: DialogueUI.cs - Simplified Version
// ============================================

using ALArcade.ArabicTMP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AL_Arcade.DialogueSystem.Scripts
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI Elements")] public Image characterSprite;
        public ArabicTextMeshProUGUI characterName;
        public ArabicTextMeshProUGUI messageText;

        // Simplified setup - no layout modifications needed
        public void Setup(DialogueMessageBase message, bool isRTL)
        {
            if (message == null) return;

            // Set character sprite
            if (characterSprite != null && message.characterSprite != null)
            {
                characterSprite.sprite = message.characterSprite;
            }

            // Set character name
            if (characterName != null)
            {
                if (characterName.GetComponent<ArabicTextMeshProUGUI>() != null)
                    characterName.GetComponent<ArabicTextMeshProUGUI>().arabicText = message.characterName;
                else
                {
                    characterName.arabicText = message.characterName;
                }
                
            }

            // Set message text (text will be empty initially for typing effect)
            if (messageText != null)
            {
                if (messageText.GetComponent<ArabicTextMeshProUGUI>() != null)
                    messageText.GetComponent<ArabicTextMeshProUGUI>().arabicText = " ";
                // The actual text will be filled by the typing animation in DialogueManager
                messageText.arabicText = "";
            }
        }

        // Optional: Method to directly set the text without typing effect
        public void SetMessageTextDirect(string text)
        {
            if (messageText != null)
            {
                if (characterName.GetComponent<ArabicTextMeshProUGUI>() != null)
                    characterName.GetComponent<ArabicTextMeshProUGUI>().arabicText = text;
                else
                {
                    characterName.arabicText = text;
                }
            }
        }

        // Optional: Show/hide character elements if needed
        public void ShowCharacterInfo(bool show)
        {
            if (characterSprite != null)
                characterSprite.gameObject.SetActive(show);

            if (characterName != null)
                characterName.gameObject.SetActive(show);
        }
    }
}