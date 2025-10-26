using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using AL_Arcade.DialogueSystem.Scripts;
using ALArcade.ArabicTMP;

namespace AL_Arcade.DialogueSystem.Scripts
{
    /// <summary>
    /// Lightweight AI chat overlay that appears during gameplay without pausing.
    /// Slides in/out based on mouse hover and AI responses.
    /// </summary>
    public class InGameAIChatPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Inspector Fields

        [Header("UI References")]
        [SerializeField] private RectTransform hoverArea;
        [SerializeField] private RectTransform charArea;
        [SerializeField] private RectTransform aiTextPanel;
        [SerializeField] private Image characterPoseImage;
        [SerializeField] private Button aiRecordButton;
        [SerializeField] private ArabicTextMeshProUGUI aiText;

        [Header("Animation Settings")]
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float offScreenXPosition = 500f;
        [SerializeField] private Ease slideEaseType = Ease.OutCubic;

        [Header("Recording Settings")]
        [SerializeField] private int maxRecordingDuration = 30;
        [SerializeField] private int recordingFrequency = 44100;

        [Header("Visual Feedback")]
        [SerializeField] private Sprite recordingSprite;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Image recordButtonImage;

        #endregion

        #region Private Fields

        private bool isPlayerHovering = false;
        private bool isCharAreaVisible = false;
        private bool isAITextPanelVisible = false;
        private bool isRecording = false;
        private bool isWaitingForAIResponse = false;

        private Vector2 charAreaOnScreenPos;
        private Vector2 charAreaOffScreenPos;
        private Vector2 aiTextPanelOnScreenPos;
        private Vector2 aiTextPanelOffScreenPos;

        private AudioClip recordedClip;
        private string microphoneDevice;
        private float recordingStartTime;

        private Tweener charAreaTweener;
        private Tweener aiTextPanelTweener;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            ValidateReferences();
            InitializeMicrophone();
        }

        void Start()
        {
            SetupInitialPositions();
            SetupButtonListener();
            SubscribeToAIEvents();
            
            Debug.Log("[InGameAIChatPanel] Initialized - Ready for gameplay overlay");
        }

        void OnEnable()
        {
            SubscribeToAIEvents();
        }

        void OnDisable()
        {
            UnsubscribeFromAIEvents();
            StopRecording();
        }

        void OnDestroy()
        {
            UnsubscribeFromAIEvents();
            
            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }

            if (aiRecordButton != null)
            {
                aiRecordButton.onClick.RemoveAllListeners();
            }

            // Kill any active tweens
            charAreaTweener?.Kill();
            aiTextPanelTweener?.Kill();
        }

        void Update()
        {
            // Auto-stop recording when max duration is reached
            if (isRecording)
            {
                float recordingTime = Time.time - recordingStartTime;
                if (recordingTime >= maxRecordingDuration)
                {
                    Debug.Log("[InGameAIChatPanel] Max recording duration reached");
                    StopRecordingAndSend();
                }
            }
        }

        #endregion

        #region Initialization

        private void ValidateReferences()
        {
            if (hoverArea == null) Debug.LogError("[InGameAIChatPanel] HoverArea is not assigned!");
            if (charArea == null) Debug.LogError("[InGameAIChatPanel] CharArea is not assigned!");
            if (aiTextPanel == null) Debug.LogError("[InGameAIChatPanel] AITextPanel is not assigned!");
            if (aiRecordButton == null) Debug.LogError("[InGameAIChatPanel] AIRecordButton is not assigned!");
            if (aiText == null) Debug.LogError("[InGameAIChatPanel] AIText is not assigned!");
        }

        private void SetupInitialPositions()
        {
            // Store on-screen positions
            charAreaOnScreenPos = charArea.anchoredPosition;
            aiTextPanelOnScreenPos = aiTextPanel.anchoredPosition;

            // Calculate off-screen positions (to the right)
            charAreaOffScreenPos = new Vector2(charAreaOnScreenPos.x + offScreenXPosition, charAreaOnScreenPos.y);
            aiTextPanelOffScreenPos = new Vector2(aiTextPanelOnScreenPos.x + offScreenXPosition, aiTextPanelOnScreenPos.y);

            // Set initial off-screen positions
            charArea.anchoredPosition = charAreaOffScreenPos;
            aiTextPanel.anchoredPosition = aiTextPanelOffScreenPos;

            isCharAreaVisible = false;
            isAITextPanelVisible = false;

            Debug.Log($"[InGameAIChatPanel] Initial positions set - CharArea off-screen, AITextPanel off-screen");
        }

        private void SetupButtonListener()
        {
            if (aiRecordButton != null)
            {
                aiRecordButton.onClick.AddListener(OnRecordButtonClicked);
            }
        }

        private void InitializeMicrophone()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("[InGameAIChatPanel] No microphone devices found!");
                if (aiRecordButton != null)
                    aiRecordButton.interactable = false;
                return;
            }

            microphoneDevice = null; // Use default device
            Debug.Log($"[InGameAIChatPanel] Microphone initialized. Available devices: {Microphone.devices.Length}");
        }

        #endregion

        #region Hover Detection (IPointerEnterHandler, IPointerExitHandler)

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPlayerHovering = true;
            Debug.Log("[InGameAIChatPanel] Mouse entered hover area");
            SlideInCharArea();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPlayerHovering = false;
            Debug.Log("[InGameAIChatPanel] Mouse exited hover area");

            // Only slide out if not waiting for AI response
            if (!isWaitingForAIResponse && !isAITextPanelVisible)
            {
                SlideOutCharArea();
            }
        }

        #endregion

        #region Animation Methods

        private void SlideInCharArea()
        {
            if (isCharAreaVisible) return;

            Debug.Log("[InGameAIChatPanel] Sliding in CharArea");
            
            charAreaTweener?.Kill();
            charAreaTweener = charArea.DOAnchorPos(charAreaOnScreenPos, slideDuration)
                .SetEase(slideEaseType)
                .SetUpdate(true) // Use unscaled time to work during any time scale
                .OnComplete(() =>
                {
                    isCharAreaVisible = true;
                    Debug.Log("[InGameAIChatPanel] CharArea slide in complete");
                });
        }

        private void SlideOutCharArea()
        {
            if (!isCharAreaVisible) return;

            Debug.Log("[InGameAIChatPanel] Sliding out CharArea");
            
            charAreaTweener?.Kill();
            charAreaTweener = charArea.DOAnchorPos(charAreaOffScreenPos, slideDuration)
                .SetEase(slideEaseType)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    isCharAreaVisible = false;
                    Debug.Log("[InGameAIChatPanel] CharArea slide out complete");
                });
        }

        private void SlideInAITextPanel()
        {
            if (isAITextPanelVisible) return;

            Debug.Log("[InGameAIChatPanel] Sliding in AITextPanel");
            
            aiTextPanelTweener?.Kill();
            aiTextPanelTweener = aiTextPanel.DOAnchorPos(aiTextPanelOnScreenPos, slideDuration)
                .SetEase(slideEaseType)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    isAITextPanelVisible = true;
                    Debug.Log("[InGameAIChatPanel] AITextPanel slide in complete");
                });
        }

        private void SlideOutAITextPanel()
        {
            if (!isAITextPanelVisible) return;

            Debug.Log("[InGameAIChatPanel] Sliding out AITextPanel");
            
            aiTextPanelTweener?.Kill();
            aiTextPanelTweener = aiTextPanel.DOAnchorPos(aiTextPanelOffScreenPos, slideDuration)
                .SetEase(slideEaseType)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    isAITextPanelVisible = false;
                    Debug.Log("[InGameAIChatPanel] AITextPanel slide out complete");

                    // After AI panel slides out, check if char area should also slide out
                    if (!isPlayerHovering)
                    {
                        SlideOutCharArea();
                    }
                });
        }

        #endregion

        #region Recording Methods

        private void OnRecordButtonClicked()
        {
            if (isWaitingForAIResponse)
            {
                Debug.Log("[InGameAIChatPanel] Already waiting for AI response");
                return;
            }

            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecordingAndSend();
            }
        }

        private void StartRecording()
        {
            Debug.Log("[InGameAIChatPanel] Starting recording...");

            // Stop any currently playing dialogue audio
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StopCurrentDialogueAudio();
            }

            // Clean up previous recording
            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }

            // Start recording
            recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingDuration, recordingFrequency);
            recordingStartTime = Time.time;
            isRecording = true;

            // Update button visual
            if (recordButtonImage != null && recordingSprite != null)
            {
                recordButtonImage.sprite = recordingSprite;
            }

            Debug.Log("[InGameAIChatPanel] Recording started");
        }

        private void StopRecordingAndSend()
        {
            if (recordedClip == null)
            {
                Debug.LogError("[InGameAIChatPanel] No recording to send!");
                isRecording = false;
                return;
            }

            Debug.Log("[InGameAIChatPanel] Stopping recording...");

            // Stop microphone
            int lastSample = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);
            isRecording = false;

            // Update button visual
            if (recordButtonImage != null && idleSprite != null)
            {
                recordButtonImage.sprite = idleSprite;
            }

            // Trim the audio clip to actual recorded length
            if (lastSample > 0 && lastSample < recordedClip.samples)
            {
                AudioClip trimmedClip = AudioClip.Create(
                    "TrimmedRecording",
                    lastSample,
                    recordedClip.channels,
                    recordedClip.frequency,
                    false
                );

                float[] data = new float[lastSample * recordedClip.channels];
                recordedClip.GetData(data, 0);
                trimmedClip.SetData(data, 0);

                Destroy(recordedClip);
                recordedClip = trimmedClip;
            }

            float duration = (float)recordedClip.samples / recordedClip.frequency;
            Debug.Log($"[InGameAIChatPanel] Recording stopped. Duration: {duration:F2} seconds");

            // Convert to WAV and send
            byte[] wavData = ConvertToWAV(recordedClip);

            if (wavData != null && wavData.Length > 0)
            {
                Debug.Log($"[InGameAIChatPanel] Sending {wavData.Length} bytes to AIVoiceHandler");
                isWaitingForAIResponse = true;
                
                if (AIVoiceHandler.Instance != null)
                {
                    AIVoiceHandler.Instance.SendVoiceMessage(wavData);
                }
                else
                {
                    Debug.LogError("[InGameAIChatPanel] AIVoiceHandler.Instance is null!");
                    isWaitingForAIResponse = false;
                }
            }
            else
            {
                Debug.LogError("[InGameAIChatPanel] Failed to convert audio to WAV");
            }
        }

        private void StopRecording()
        {
            if (Microphone.IsRecording(microphoneDevice))
            {
                Microphone.End(microphoneDevice);
                Debug.Log("[InGameAIChatPanel] Recording stopped (cleanup)");
            }

            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }

            isRecording = false;

            if (recordButtonImage != null && idleSprite != null)
            {
                recordButtonImage.sprite = idleSprite;
            }
        }

        #endregion

        #region WAV Conversion

        private byte[] ConvertToWAV(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("[InGameAIChatPanel] Cannot convert null AudioClip");
                return null;
            }

            try
            {
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                // Convert float samples to 16-bit PCM
                short[] intData = new short[samples.Length];
                byte[] bytesData = new byte[samples.Length * 2];

                int rescaleFactor = 32767;

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * rescaleFactor);
                    byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
                    byteArr.CopyTo(bytesData, i * 2);
                }

                // Create WAV file structure
                int fileSize = 44 + bytesData.Length;
                byte[] wav = new byte[fileSize];

                // RIFF header
                byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
                riff.CopyTo(wav, 0);

                byte[] chunkSize = System.BitConverter.GetBytes(fileSize - 8);
                chunkSize.CopyTo(wav, 4);

                byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
                wave.CopyTo(wav, 8);

                // fmt subchunk
                byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
                fmt.CopyTo(wav, 12);

                byte[] subChunk1 = System.BitConverter.GetBytes(16);
                subChunk1.CopyTo(wav, 16);

                ushort audioFormat = 1;
                byte[] audioFormatBytes = System.BitConverter.GetBytes(audioFormat);
                audioFormatBytes.CopyTo(wav, 20);

                byte[] numChannels = System.BitConverter.GetBytes(clip.channels);
                numChannels.CopyTo(wav, 22);

                byte[] sampleRate = System.BitConverter.GetBytes(clip.frequency);
                sampleRate.CopyTo(wav, 24);

                int byteRate = clip.frequency * clip.channels * 2;
                byte[] byteRateBytes = System.BitConverter.GetBytes(byteRate);
                byteRateBytes.CopyTo(wav, 28);

                ushort blockAlign = (ushort)(clip.channels * 2);
                byte[] blockAlignBytes = System.BitConverter.GetBytes(blockAlign);
                blockAlignBytes.CopyTo(wav, 32);

                ushort bitsPerSample = 16;
                byte[] bitsPerSampleBytes = System.BitConverter.GetBytes(bitsPerSample);
                bitsPerSampleBytes.CopyTo(wav, 34);

                // data subchunk
                byte[] data = System.Text.Encoding.UTF8.GetBytes("data");
                data.CopyTo(wav, 36);

                byte[] subChunk2 = System.BitConverter.GetBytes(bytesData.Length);
                subChunk2.CopyTo(wav, 40);

                bytesData.CopyTo(wav, 44);

                Debug.Log($"[InGameAIChatPanel] WAV conversion successful. Size: {wav.Length} bytes");
                return wav;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InGameAIChatPanel] WAV conversion failed: {e.Message}");
                return null;
            }
        }

        #endregion

        #region AI Event Handlers

        private void SubscribeToAIEvents()
        {
            if (AIVoiceHandler.Instance != null)
            {
                AIVoiceHandler.Instance.OnResponseTextReceived += OnAIResponseTextReceived;
                AIVoiceHandler.Instance.OnResponseAudioReady += OnAIResponseAudioReady;
                AIVoiceHandler.Instance.OnError += OnAIError;
                AIVoiceHandler.Instance.OnProcessingComplete += OnAIProcessingComplete;
            }
        }

        private void UnsubscribeFromAIEvents()
        {
            if (AIVoiceHandler.Instance != null)
            {
                AIVoiceHandler.Instance.OnResponseTextReceived -= OnAIResponseTextReceived;
                AIVoiceHandler.Instance.OnResponseAudioReady -= OnAIResponseAudioReady;
                AIVoiceHandler.Instance.OnError -= OnAIError;
                AIVoiceHandler.Instance.OnProcessingComplete -= OnAIProcessingComplete;
            }
        }

        private void OnAIResponseTextReceived(string responseText)
        {
            Debug.Log($"[InGameAIChatPanel] AI response text received: {responseText}");

            // Update AI text
            if (aiText != null)
            {
                aiText.arabicText = responseText;
            }

            // Report to GameContextBuilder
            if (GameContextBuilder.Instance != null && !string.IsNullOrEmpty(responseText))
            {
                string contextEntry = $"AI Assistant (Free Chat): {responseText}";
                GameContextBuilder.Instance.AddPlayerAction(contextEntry);
            }

            // Slide in AI text panel
            SlideInAITextPanel();
        }

        private void OnAIResponseAudioReady(AudioClip audioClip)
        {
            Debug.Log($"[InGameAIChatPanel] AI response audio ready. Duration: {audioClip.length:F2}s");

            // Keep AI text panel visible for the duration of the audio
            StartCoroutine(HideAITextPanelAfterDelay(audioClip.length));
        }

        private void OnAIError(string error)
        {
            Debug.LogError($"[InGameAIChatPanel] AI error: {error}");
            
            isWaitingForAIResponse = false;

            // Show error in AI text
            if (aiText != null)
            {
                aiText.text = $"Error: {error}";
            }

            // Slide in panel to show error, then hide after delay
            SlideInAITextPanel();
            StartCoroutine(HideAITextPanelAfterDelay(3f));
        }

        private void OnAIProcessingComplete()
        {
            Debug.Log("[InGameAIChatPanel] AI processing complete");
            isWaitingForAIResponse = false;
        }

        private IEnumerator HideAITextPanelAfterDelay(float delay)
        {
            Debug.Log($"[InGameAIChatPanel] Waiting {delay:F2} seconds before hiding AI text panel");
            
            yield return new WaitForSeconds(delay);

            SlideOutAITextPanel();
        }

        #endregion

        #region Public API (Optional)

        /// <summary>
        /// Manually show the character area.
        /// </summary>
        public void ShowCharacterArea()
        {
            SlideInCharArea();
        }

        /// <summary>
        /// Manually hide the character area.
        /// </summary>
        public void HideCharacterArea()
        {
            SlideOutCharArea();
        }

        /// <summary>
        /// Set the character pose sprite.
        /// </summary>
        public void SetCharacterPose(Sprite sprite)
        {
            if (characterPoseImage != null)
            {
                characterPoseImage.sprite = sprite;
            }
        }

        /// <summary>
        /// Check if currently recording.
        /// </summary>
        public bool IsRecording => isRecording;

        /// <summary>
        /// Check if waiting for AI response.
        /// </summary>
        public bool IsWaitingForAI => isWaitingForAIResponse;

        #endregion
    }
}