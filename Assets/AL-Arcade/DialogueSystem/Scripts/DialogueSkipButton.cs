using UnityEngine;
using UnityEngine.UI;

namespace AL_Arcade.DialogueSystem.Scripts
{
    /// <summary>
    /// Simple component to add to any button to make it skip dialogue audio
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class DialogueSkipButton : MonoBehaviour
    {
        private Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnSkipClicked);
        }

        void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnSkipClicked);
        }

        private void OnSkipClicked()
        {
            if (DialogueManager.Instance != null)
            {
                if (DialogueManager.Instance.IsDialogueAudioPlaying())
                {
                    DialogueManager.Instance.StopCurrentDialogueAudio();
                }
                else
                {
                    DialogueManager.Instance.SkipCurrentDialogue(true);
                }
            }
        }
    }
}