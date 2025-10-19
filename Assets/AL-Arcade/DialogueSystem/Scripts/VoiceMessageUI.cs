using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ALArcade.ArabicTMP;

namespace AL_Arcade.DialogueSystem.Scripts
{
    /// <summary>
    /// Handles voice recording UI for dialogue messages.
    /// Attach to dialogue message prefabs (EN/AR).
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class VoiceMessageUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("UI References")]
        [SerializeField] private Button voiceButton;
        [SerializeField] private Image buttonImage;
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Button Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite recordingSprite;
        [SerializeField] private Sprite sendingSprite;

        [Header("Recording Settings")]
        [SerializeField] private int maxRecordingDuration = 30;
        [SerializeField] private int recordingFrequency = 44100;
        #endregion

        #region State Enum
        private enum VoiceState
        {
            IDLE,
            RECORDING,
            SENDING,
            COMPLETE
        }
        #endregion

        #region Private Fields
        private VoiceState currentState = VoiceState.IDLE;
        private AudioClip recordedClip;
        private string microphoneDevice;
        private float recordingStartTime;
        private bool isInitialized = false;
        #endregion

        #region Unity Lifecycle
        void Awake()
        {
            // Auto-assign button if not set
            if (voiceButton == null)
                voiceButton = GetComponent<Button>();

            // Auto-assign button image if not set
            if (buttonImage == null && voiceButton != null)
                buttonImage = voiceButton.GetComponent<Image>();

            // Validate setup
            if (voiceButton == null)
            {
                Debug.LogError("[VoiceMessageUI] Button component is missing!");
                enabled = false;
                return;
            }

            if (messageText == null)
            {
                Debug.LogError("[VoiceMessageUI] MessageText reference is missing!");
            }
        }

        void Start()
        {
            // Setup button listener
            voiceButton.onClick.AddListener(OnVoiceButtonClicked);
            
            // Initialize microphone
            InitializeMicrophone();
            
            // Set initial state
            SetState(VoiceState.IDLE);
            
            Debug.Log("[VoiceMessageUI] Initialized");
        }

        void OnEnable()
        {
            // Subscribe to AI handler events
            if (AIVoiceHandler.Instance != null)
            {
                AIVoiceHandler.Instance.OnResponseTextReceived += OnResponseTextReceived;
                AIVoiceHandler.Instance.OnError += OnError;
                AIVoiceHandler.Instance.OnProcessingComplete += OnProcessingComplete;
            }
        }

        void OnDisable()
        {
            // Unsubscribe from events
            if (AIVoiceHandler.Instance != null)
            {
                AIVoiceHandler.Instance.OnResponseTextReceived -= OnResponseTextReceived;
                AIVoiceHandler.Instance.OnError -= OnError;
                AIVoiceHandler.Instance.OnProcessingComplete -= OnProcessingComplete;
            }

            // Stop recording if active
            if (currentState == VoiceState.RECORDING)
            {
                StopRecording();
            }
        }

        void OnDestroy()
        {
            // Cleanup
            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }

            if (voiceButton != null)
            {
                voiceButton.onClick.RemoveListener(OnVoiceButtonClicked);
            }
        }

        void Update()
        {
            // Auto-stop recording when max duration is reached
            if (currentState == VoiceState.RECORDING)
            {
                float recordingTime = Time.realtimeSinceStartup - recordingStartTime;
                if (recordingTime >= maxRecordingDuration)
                {
                    Debug.Log("[VoiceMessageUI] Max recording duration reached. Auto-stopping.");
                    OnVoiceButtonClicked();
                }
            }
        }
        #endregion

        #region Microphone Initialization
        /// <summary>
        /// Initializes microphone device.
        /// </summary>
        private void InitializeMicrophone()
        {
            if (Microphone.devices.Length == 0)
            {
                Debug.LogError("[VoiceMessageUI] No microphone devices found!");
                voiceButton.interactable = false;
                return;
            }

            microphoneDevice = null; // Use default device
            isInitialized = true;
            Debug.Log($"[VoiceMessageUI] Microphone initialized. Available devices: {Microphone.devices.Length}");
        }
        #endregion

        #region Button Click Handler
        /// <summary>
        /// Handles voice button clicks based on current state.
        /// </summary>
        private void OnVoiceButtonClicked()
        {
            switch (currentState)
            {
                case VoiceState.IDLE:
                    StartRecording();
                    break;

                case VoiceState.RECORDING:
                    StopRecordingAndSend();
                    break;

                case VoiceState.SENDING:
                    Debug.Log("[VoiceMessageUI] Currently sending. Please wait.");
                    break;

                case VoiceState.COMPLETE:
                    // Reset to idle
                    SetState(VoiceState.IDLE);
                    break;
            }
        }
        #endregion

        #region Recording Logic
        /// <summary>
        /// Starts microphone recording.
        /// </summary>
        private void StartRecording()
        {
            if (!isInitialized)
            {
                Debug.LogError("[VoiceMessageUI] Microphone not initialized!");
                return;
            }

            Debug.Log("[VoiceMessageUI] Starting recording...");

            // Clean up previous recording
            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }

            // Start recording
            recordedClip = Microphone.Start(microphoneDevice, false, maxRecordingDuration, recordingFrequency);
            recordingStartTime = Time.realtimeSinceStartup;
            
            SetState(VoiceState.RECORDING);
            Debug.Log("[VoiceMessageUI] Recording started");
        }

        /// <summary>
        /// Stops recording and sends audio to AI handler.
        /// </summary>
        private void StopRecordingAndSend()
        {
            if (recordedClip == null)
            {
                Debug.LogError("[VoiceMessageUI] No recording to send!");
                SetState(VoiceState.IDLE);
                return;
            }

            Debug.Log("[VoiceMessageUI] Stopping recording...");

            // Stop microphone
            int lastSample = Microphone.GetPosition(microphoneDevice);
            Microphone.End(microphoneDevice);

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
            Debug.Log($"[VoiceMessageUI] Recording stopped. Duration: {duration:F2} seconds");

            // Convert to WAV and send
            SetState(VoiceState.SENDING);
            byte[] wavData = ConvertToWAV(recordedClip);
            
            if (wavData != null && wavData.Length > 0)
            {
                Debug.Log($"[VoiceMessageUI] Sending {wavData.Length} bytes to AI handler");
                AIVoiceHandler.Instance.SendVoiceMessage(wavData);
            }
            else
            {
                Debug.LogError("[VoiceMessageUI] Failed to convert audio to WAV");
                SetState(VoiceState.IDLE);
            }
        }

        /// <summary>
        /// Stops recording without sending (cleanup).
        /// </summary>
        private void StopRecording()
        {
            if (Microphone.IsRecording(microphoneDevice))
            {
                Microphone.End(microphoneDevice);
                Debug.Log("[VoiceMessageUI] Recording stopped (cleanup)");
            }

            if (recordedClip != null)
            {
                Destroy(recordedClip);
                recordedClip = null;
            }
        }
        #endregion

        #region WAV Conversion
        /// <summary>
        /// Converts an AudioClip to WAV file bytes.
        /// </summary>
        private byte[] ConvertToWAV(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError("[VoiceMessageUI] Cannot convert null AudioClip");
                return null;
            }

            try
            {
                float[] samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);

                // Convert float samples to 16-bit PCM
                short[] intData = new short[samples.Length];
                byte[] bytesData = new byte[samples.Length * 2];

                int rescaleFactor = 32767; // Max value for 16-bit signed

                for (int i = 0; i < samples.Length; i++)
                {
                    intData[i] = (short)(samples[i] * rescaleFactor);
                    byte[] byteArr = BitConverter.GetBytes(intData[i]);
                    byteArr.CopyTo(bytesData, i * 2);
                }

                // Create WAV file structure
                int fileSize = 44 + bytesData.Length;
                byte[] wav = new byte[fileSize];

                // RIFF header
                byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
                riff.CopyTo(wav, 0);

                byte[] chunkSize = BitConverter.GetBytes(fileSize - 8);
                chunkSize.CopyTo(wav, 4);

                byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
                wave.CopyTo(wav, 8);

                // fmt subchunk
                byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
                fmt.CopyTo(wav, 12);

                byte[] subChunk1 = BitConverter.GetBytes(16);
                subChunk1.CopyTo(wav, 16);

                ushort audioFormat = 1; // PCM
                byte[] audioFormatBytes = BitConverter.GetBytes(audioFormat);
                audioFormatBytes.CopyTo(wav, 20);

                byte[] numChannels = BitConverter.GetBytes(clip.channels);
                numChannels.CopyTo(wav, 22);

                byte[] sampleRate = BitConverter.GetBytes(clip.frequency);
                sampleRate.CopyTo(wav, 24);

                int byteRate = clip.frequency * clip.channels * 2;
                byte[] byteRateBytes = BitConverter.GetBytes(byteRate);
                byteRateBytes.CopyTo(wav, 28);

                ushort blockAlign = (ushort)(clip.channels * 2);
                byte[] blockAlignBytes = BitConverter.GetBytes(blockAlign);
                blockAlignBytes.CopyTo(wav, 32);

                ushort bitsPerSample = 16;
                byte[] bitsPerSampleBytes = BitConverter.GetBytes(bitsPerSample);
                bitsPerSampleBytes.CopyTo(wav, 34);

                // data subchunk
                byte[] data = System.Text.Encoding.UTF8.GetBytes("data");
                data.CopyTo(wav, 36);

                byte[] subChunk2 = BitConverter.GetBytes(bytesData.Length);
                subChunk2.CopyTo(wav, 40);

                bytesData.CopyTo(wav, 44);

                Debug.Log($"[VoiceMessageUI] WAV conversion successful. Size: {wav.Length} bytes");
                return wav;
            }
            catch (Exception e)
            {
                Debug.LogError($"[VoiceMessageUI] WAV conversion failed: {e.Message}");
                return null;
            }
        }
        #endregion

        #region State Management
        /// <summary>
        /// Sets the current UI state and updates visuals.
        /// </summary>
        private void SetState(VoiceState newState)
        {
            currentState = newState;
            UpdateButtonVisuals();
            Debug.Log($"[VoiceMessageUI] State changed to: {newState}");
        }

        /// <summary>
        /// Updates button visuals based on current state.
        /// </summary>
        private void UpdateButtonVisuals()
        {
            if (buttonImage == null) return;

            switch (currentState)
            {
                case VoiceState.IDLE:
                    if (idleSprite != null)
                        buttonImage.sprite = idleSprite;
                    voiceButton.interactable = true;
                    break;

                case VoiceState.RECORDING:
                    if (recordingSprite != null)
                        buttonImage.sprite = recordingSprite;
                    voiceButton.interactable = true;
                    break;

                case VoiceState.SENDING:
                    if (sendingSprite != null)
                        buttonImage.sprite = sendingSprite;
                    voiceButton.interactable = false;
                    break;

                case VoiceState.COMPLETE:
                    if (idleSprite != null)
                        buttonImage.sprite = idleSprite;
                    voiceButton.interactable = true;
                    break;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Called when AI response text is received.
        /// </summary>
        private void OnResponseTextReceived(string responseText)
        {
            if (messageText == null)
            {
                Debug.LogWarning("[VoiceMessageUI] MessageText reference is null");
                return;
            }

            Debug.Log($"[VoiceMessageUI] Updating message text with response: {responseText}");

            // Handle Arabic text if component exists
            ArabicTextMeshProUGUI arabicText = messageText.GetComponent<ArabicTextMeshProUGUI>();
            if (arabicText != null)
            {
                arabicText.arabicText = responseText;
            }
            else
            {
                messageText.text = responseText;
            }
        }

        /// <summary>
        /// Called when processing is complete.
        /// </summary>
        private void OnProcessingComplete()
        {
            Debug.Log("[VoiceMessageUI] Processing complete");
            SetState(VoiceState.COMPLETE);
            
            // Auto-return to idle after a short delay
            StartCoroutine(AutoReturnToIdle());
        }

        /// <summary>
        /// Called when an error occurs.
        /// </summary>
        private void OnError(string error)
        {
            Debug.LogError($"[VoiceMessageUI] Error received: {error}");
            SetState(VoiceState.IDLE);
        }

        /// <summary>
        /// Auto-returns to idle state after response is complete.
        /// </summary>
        private IEnumerator AutoReturnToIdle()
        {
            yield return new WaitForSeconds(1f);
            SetState(VoiceState.IDLE);
        }
        #endregion
    }
}