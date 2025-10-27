using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using AL_Arcade.DialogueSystem.Scripts;
using ALArcade.ArabicTMP;
using TMPro;

namespace AL_Arcade.DialogueSystem.Scripts
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("UI References")] [SerializeField]
        private Transform dialogueContainer;

        [SerializeField] private GameObject dialoguePrefabEN;
        [SerializeField] private GameObject dialoguePrefabAR;
        [SerializeField] private GameObject replyPrefab;
        [SerializeField] private GameObject replyPanel;
        [SerializeField] private Transform replyContainer;
        [SerializeField] private CanvasGroup dialogueCanvasGroup;
        [SerializeField] private RectTransform FreeUseDialogue; 

        [Header("Settings")] [SerializeField] private Language currentLanguage = Language.English;
        [SerializeField] private float textSpeed = 0.03f;
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private KeyCode advanceKey = KeyCode.Space;

        [Header("Audio")]  public AudioSource voiceAudioSource;
        [SerializeField] private AudioSource sfxAudioSource;

        // Current dialogue state
        private DialogueMessageBase currentMessage;
        public GameObject currentDialogueUI;
        private List<GameObject> activeReplyButtons = new List<GameObject>();
        private bool isTyping = false;
        private bool canAdvance = false;
        private Coroutine typingCoroutine;

        // Events
        public System.Action<DialogueSequence> OnDialogueStart;
        public System.Action OnDialogueEnd;
        public System.Action<DialogueMessageBase> OnMessageDisplay;
        public System.Action<DialogueReply> OnReplySelected;

        private static DialogueManager instance;

        public static DialogueManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<DialogueManager>();
                return instance;
            }
        }

        public enum Language
        {
            English,
            Arabic
        }

        void Awake()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy(gameObject);
            
            if (replyPanel != null)
                Debug.Log("I hId it start ");
            replyPanel.SetActive(false);

            if (dialogueCanvasGroup != null)
                dialogueCanvasGroup.alpha = 0;
        }

        void Start()
        {
    
        }
        #region Audio Skip Functionality
        /// <summary>
        /// Stops any currently playing dialogue audio
        /// </summary>
        public void StopCurrentDialogueAudio()
        {
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
                Debug.Log("[DialogueManager] Stopped current dialogue audio");
            }
        }

        /// <summary>
        /// Skips current dialogue audio and optionally advances to next message
        /// </summary>
        public void SkipCurrentDialogue(bool advanceToNext = false)
        {
            StopCurrentDialogueAudio();
    
            if (advanceToNext && canAdvance && !isTyping)
            {
                if (currentMessage != null && (currentMessage.replies == null || currentMessage.replies.Count == 0))
                {
                    AdvanceDialogue();
                }
            }
        }

        /// <summary>
        /// Checks if dialogue audio is currently playing
        /// </summary>
        public bool IsDialogueAudioPlaying()
        {
            return voiceAudioSource != null && voiceAudioSource.isPlaying;
        }
        #endregion


        void Update()
        {
            // Handle advance input
            if (canAdvance && !isTyping && Input.GetKeyDown(advanceKey))
            {
                if (currentMessage != null && (currentMessage.replies == null || currentMessage.replies.Count == 0))
                {
                    AdvanceDialogue();
                }
            }

            // Skip typing animation
            if (isTyping && Input.GetKeyDown(advanceKey))
            {
                CompleteTyping();
            }
            if (Input.GetKeyDown(advanceKey))
            {
                // If audio is playing, stop it first
                if (IsDialogueAudioPlaying())
                {
                    StopCurrentDialogueAudio();
                    return; // Don't advance yet, just stop the audio
                }
        
                // If typing, complete the typing
                if (isTyping)
                {
                    CompleteTyping();
                    return;
                }
        
                // Otherwise advance dialogue if possible
                if (canAdvance && currentMessage != null && (currentMessage.replies == null || currentMessage.replies.Count == 0))
                {
                    AdvanceDialogue();
                }
            }
        }
        

        public void StartDialogue(DialogueSequence sequence)
        {
            if (sequence == null || sequence.firstMessage == null)
            {
                Debug.LogError("Invalid dialogue sequence!");
                return;
            }

            OnDialogueStart?.Invoke(sequence);

            if (sequence.pauseGameDuringDialogue)
                Time.timeScale = 0;

            ShowDialogueUI(true);
            DisplayMessage(sequence.firstMessage);
            FreeUseDialogue.gameObject.SetActive(false);
        }

        public void StartDialogue(DialogueMessageBase firstMessage)
        {
            if (firstMessage == null)
            {
                Debug.LogError("Invalid dialogue message!");
                return;
            }

            ShowDialogueUI(true);
            DisplayMessage(firstMessage);
            FreeUseDialogue.gameObject.SetActive(false);
        }

        private void DisplayMessage(DialogueMessageBase message)
        {
            if (message == null)
            {
                EndDialogue();
                return;
            }

            // Stop any currently playing audio before starting new message
            StopCurrentDialogueAudio();

            currentMessage = message;
            OnMessageDisplay?.Invoke(message);

            // Report to GameContextBuilder
            if (GameContextBuilder.Instance != null && message != null)
            {
                string contextEntry = $"NPC [{message.characterName}]: {message.messageText}";
                GameContextBuilder.Instance.AddPlayerAction(contextEntry);
            }

            // Stop any currently playing audio before starting new message
            StopCurrentDialogueAudio();

            currentMessage = message;
            OnMessageDisplay?.Invoke(message);

            // Clear previous UI
            if (currentDialogueUI != null)
                Destroy(currentDialogueUI);

            // Create appropriate prefab
            GameObject prefabToUse = currentLanguage == Language.English ? dialoguePrefabEN : dialoguePrefabAR;
            currentDialogueUI = Instantiate(prefabToUse, dialogueContainer);

            // Setup the dialogue UI
            DialogueUI dialogueUI = currentDialogueUI.GetComponent<DialogueUI>();
            if (dialogueUI != null)
            {
                dialogueUI.Setup(message, currentLanguage == Language.Arabic);

                // Start typing animation
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);
                typingCoroutine = StartCoroutine(TypeText(dialogueUI.messageText, message.messageText));
            }

            // Play voice clip
            if (message.voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = message.voiceClip;
                voiceAudioSource.Play();
            }

            // Handle replies
            UpdateReplyPanel(message);
        }

        private IEnumerator TypeText(ArabicTextMeshProUGUI textComponent, string fullText)
        {
            isTyping = true;
            canAdvance = false;
            textComponent.arabicText = "";
            ArabicTextMeshProUGUI arText;
           arText =  textComponent.gameObject.GetComponent<ArabicTextMeshProUGUI>() != null ? textComponent.gameObject.GetComponent<ArabicTextMeshProUGUI>() :  null;
            foreach (char c in fullText)
            {
                if (arText != null) arText.arabicText += c;
                else textComponent.arabicText += c;
                yield return new WaitForSecondsRealtime(textSpeed);
            }

            isTyping = false;
            canAdvance = true;
        }

        private void CompleteTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (currentDialogueUI != null)
            {
                DialogueUI dialogueUI = currentDialogueUI.GetComponent<DialogueUI>();
                if (dialogueUI != null && currentMessage != null)
                {
                    dialogueUI.messageText.arabicText = currentMessage.messageText;
                }
            }

            isTyping = false;
            canAdvance = true;
        }

        private void UpdateReplyPanel(DialogueMessageBase message)
        {
            // Clear previous replies
            foreach (var btn in activeReplyButtons)
            {
                Destroy(btn);
            }

            activeReplyButtons.Clear();

            Debug.Log(message.replies.Count);
            // Check if we have replies
            if (message.replies != null && message.replies.Count > 0)
            {
                replyPanel.SetActive(true);
                replyContainer.gameObject.SetActive(true);
                
                Debug.Log("message.replies.Count");

                // Create reply buttons
                foreach (var reply in message.replies)
                {
                    if (reply == null) continue;

                    // Check conditions if needed
                    if (reply.requiresCondition && !CheckCondition(reply.conditionKey))
                        continue;

                    GameObject replyButton = Instantiate(replyPrefab, replyContainer);
                    ReplyButton replyBtn = replyButton.GetComponent<ReplyButton>();

                    if (replyBtn != null)
                    {
                        replyBtn.Setup(reply, () => SelectReply(reply));
                    }

                    activeReplyButtons.Add(replyButton);
                }
            }
            else
            {
                Debug.Log("I hId it else ");
                replyPanel.SetActive(false);
            }
        }

        private void SelectReply(DialogueReply reply)
        {
            if (reply == null) return;

            OnReplySelected?.Invoke(reply);

            // Report player's reply to GameContextBuilder
            if (GameContextBuilder.Instance != null && reply != null)
            {
                string contextEntry = $"Player Reply: {reply.replyText}";
                GameContextBuilder.Instance.AddPlayerAction(contextEntry);
            }

            // Hide reply panel
            Debug.Log("I hId it inSelectReply ");
                
            replyPanel.SetActive(false);

            // Display next message
            DisplayMessage(reply.nextMessage);
        }

        private void AdvanceDialogue()
        {
            if (currentMessage != null && currentMessage.nextMessage != null)
            {
                DisplayMessage(currentMessage.nextMessage);
            }
            else
            {
                EndDialogue();
               // GameContextBuilder.Instance.InitializeGame("","","");
            }
        }

        private void EndDialogue()
        {
            ShowDialogueUI(false);
            

            if (currentDialogueUI != null)
                Destroy(currentDialogueUI);

            currentMessage = null;
            Time.timeScale = 1;

            OnDialogueEnd?.Invoke();
            FreeUseDialogue.gameObject.SetActive(true);
        }

        private void ShowDialogueUI(bool show)
        {
            if (dialogueCanvasGroup != null)
            {
                StartCoroutine(FadeCanvas(show));
                
            }
        }

        private IEnumerator FadeCanvas(bool fadeIn)
        {
            float targetAlpha = fadeIn ? 1 : 0;
            float startAlpha = dialogueCanvasGroup.alpha;
            float elapsed = 0;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                dialogueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeInDuration);
                yield return null;
            }

            dialogueCanvasGroup.alpha = targetAlpha;
            dialogueCanvasGroup.interactable = fadeIn;
            dialogueCanvasGroup.blocksRaycasts = fadeIn;
        }

        private bool CheckCondition(string conditionKey)
        {
            // Implement your condition checking logic here
            // For example, checking player stats, quest progress, etc.
            // For now, return true as placeholder
            return true;
        }

        public void SetLanguage(Language language)
        {
            currentLanguage = language;
        }
    }
}