using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AL_Arcade.DialogueSystem.Scripts
{
    public class AIVoiceHandler : MonoBehaviour
    {
        #region Singleton
        private static AIVoiceHandler instance;
        public static AIVoiceHandler Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<AIVoiceHandler>();
                return instance;
            }
        }
        #endregion

        #region API config
        private const string API_URL = "https://voice-agent.caprover.al-arcade.com/chat";
        private const string STUDENT_ID = "student_001";
        private const int REQUEST_TIMEOUT = 30;
        #endregion

        #region Events
        public event Action<string> OnResponseTextReceived;
        public event Action<AudioClip> OnResponseAudioReady;
        public event Action<string> OnError;
        public event Action OnProcessingStarted;
        public event Action OnProcessingComplete;
        #endregion

        #region Fields and Paths
        private bool isProcessing;
        private string saveDirectory;
        private string responseWavPath;
        private AudioSource targetAudioSource;
        
        private const string RESPONSE_WAV = "response.wav";
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this)  { Destroy(gameObject); return; }

            // Setup paths
            saveDirectory = Path.Combine(Application.persistentDataPath, "DialogueSystem");
            
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            responseWavPath = Path.Combine(saveDirectory, RESPONSE_WAV);

            // Cache the first AudioSource component
            AudioSource[] audioSources = GetComponents<AudioSource>();
            if (audioSources != null && audioSources.Length > 0)
            {
                targetAudioSource = audioSources[0];
                Debug.Log($"[AIVoiceHandler] Cached first AudioSource component");
            }
            else
            {
                Debug.LogError("[AIVoiceHandler] No AudioSource components found on this GameObject!");
            }

            Debug.Log($"[AIVoiceHandler] Initialized - Directory: {saveDirectory}");
        }

        private void OnDestroy()
        {
            CleanupTempFiles();
        }
        #endregion

        #region Public API
        public bool IsProcessing => isProcessing;

        public void SendVoiceMessage(byte[] wavData)
        {
            if (isProcessing)
            {
                Fail("Already processing a request");
                return;
            }

            if (wavData == null || wavData.Length < 44)
            {
                Fail("Invalid WAV data");
                return;
            }

            if (targetAudioSource == null)
            {
                Fail("No AudioSource component found to play audio");
                return;
            }

            StartCoroutine(ProcessVoiceMessageCoroutine(wavData));
        }
        #endregion

        #region Core Processing
        private IEnumerator ProcessVoiceMessageCoroutine(byte[] wavData)
        {
            isProcessing = true;
            OnProcessingStarted?.Invoke();
            Debug.Log("[AIVoiceHandler] Processing started");

            // Get game context
            string gameContext = GameContextBuilder.Instance != null
                ? GameContextBuilder.Instance.GetFullContext()
                : "No game context available";

            // Send API request
            ChatResponse response = null;
            yield return SendChatRequest(wavData, gameContext, (success, result, error) =>
            {
                if (success)
                {
                    response = result;
                    Debug.Log("[AIVoiceHandler] API Success");
                }
                else
                {
                    Fail($"API Error: {error}");
                }
            });

            if (response == null)
            {
                EndProcessing();
                yield break;
            }

            Debug.Log($"[AIVoiceHandler] Response received - Text: {response.agent_response}");

            // Invoke text response
            OnResponseTextReceived?.Invoke(response.agent_response ?? string.Empty);

            // Download WAV and save
            yield return DownloadDecodeAndSaveWav(response.audio_filepath);

            EndProcessing();
        }

        private void EndProcessing()
        {
            isProcessing = false;
            OnProcessingComplete?.Invoke();
            Debug.Log("[AIVoiceHandler] Processing complete");
        }
        #endregion

        #region API Communication
        private IEnumerator SendChatRequest(byte[] wavData, string gameContext, Action<bool, ChatResponse, string> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("student_id", STUDENT_ID);
            form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
            form.AddField("game_context", gameContext);

            using (UnityWebRequest request = UnityWebRequest.Post(API_URL, form))
            {
                request.timeout = REQUEST_TIMEOUT;
                
                Debug.Log("[AIVoiceHandler] Sending request to API");
                
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(false, null, request.error);
                    yield break;
                }

                ChatResponse parsedResponse = null;
                Exception parseException = null;
                
                try
                {
                    parsedResponse = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                }
                catch (Exception e)
                {
                    parseException = e;
                }
                
                if (parsedResponse != null && parsedResponse.status == "success")
                {
                    callback?.Invoke(true, parsedResponse, null);
                }
                else
                {
                    string error = parseException != null 
                        ? $"JSON parse error: {parseException.Message}"
                        : $"API returned status: {parsedResponse?.status}";
                    callback?.Invoke(false, null, error);
                }
            }
        }
        #endregion

        #region Download, Decode and Save
        private IEnumerator DownloadDecodeAndSaveWav(string audioUrl)
        {
            if (string.IsNullOrEmpty(audioUrl))
            {
                Fail("No audio URL provided");
                yield break;
            }

            Debug.Log("[AIVoiceHandler] Downloading WAV file");

            // Clean up old file
            DeleteFileIfExists(responseWavPath);

            // Download WAV data from server
            byte[] wavData = null;
            
            using (UnityWebRequest request = UnityWebRequest.Get(audioUrl))
            {
                request.timeout = REQUEST_TIMEOUT;
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Fail($"Download failed: {request.error}");
                    yield break;
                }

                wavData = request.downloadHandler.data;
                
                if (wavData == null || wavData.Length < 44)
                {
                    Fail($"Downloaded WAV data is invalid (size: {wavData?.Length ?? 0})");
                    yield break;
                }
            }

            // Repair WAV header if needed
            wavData = RepairWavHeader(wavData);

            // Save WAV file to disk
            try
            {
                File.WriteAllBytes(responseWavPath, wavData);
                Debug.Log($"[AIVoiceHandler] WAV file saved: {responseWavPath} ({wavData.Length} bytes)");
            }
            catch (Exception e)
            {
                Fail($"Failed to save WAV file: {e.Message}");
                yield break;
            }

            // Load WAV using WavUtility
            AudioClip audioClip = null;
            
            try
            {
                audioClip = WavUtility.ToAudioClip(wavData, 0, "response");
                
                if (audioClip != null)
                {
                    Debug.Log($"[AIVoiceHandler] AudioClip loaded - Duration: {audioClip.length:F2}s, Frequency: {audioClip.frequency}Hz");
                }
            }
            catch (Exception e)
            {
                // Try loading from file path as fallback
                try
                {
                    audioClip = WavUtility.ToAudioClip(responseWavPath);
                }
                catch
                {
                    Fail($"Failed to load WAV: {e.Message}");
                    yield break;
                }
            }
            
            if (audioClip == null || audioClip.samples <= 0 || audioClip.channels <= 0)
            {
                if (audioClip != null) Destroy(audioClip);
                Fail("Failed to create valid AudioClip");
                yield break;
            }

            // Play the audio clip on the cached AudioSource
            PlayAudioClip(audioClip);

            // Still invoke the event for any external listeners
            OnResponseAudioReady?.Invoke(audioClip);
        }

        private void PlayAudioClip(AudioClip clip)
        {
            if (targetAudioSource == null)
            {
                Debug.LogError("[AIVoiceHandler] Cannot play audio - AudioSource is null");
                return;
            }

            if (clip == null)
            {
                Debug.LogError("[AIVoiceHandler] Cannot play audio - AudioClip is null");
                return;
            }

            // Stop any currently playing audio
            if (targetAudioSource.isPlaying)
            {
                targetAudioSource.Stop();
                Debug.Log("[AIVoiceHandler] Stopped previous audio playback");
            }

            // Assign and play the new clip
            targetAudioSource.clip = clip;
            targetAudioSource.Play();
            
            Debug.Log($"[AIVoiceHandler] Playing audio response on AudioSource");
            Debug.Log($"  Clip: {clip.name}");
            Debug.Log($"  Duration: {clip.length:F2} seconds");
            Debug.Log($"  Is Playing: {targetAudioSource.isPlaying}");
            Debug.Log($"  Volume: {targetAudioSource.volume}");
            Debug.Log($"  Mute: {targetAudioSource.mute}");
        }

        private byte[] RepairWavHeader(byte[] wavData)
        {
            if (wavData == null || wavData.Length < 44)
                return wavData;

            // Check and repair file size field (position 4)
            int fileSizeField = BitConverter.ToInt32(wavData, 4);
            if (fileSizeField == -1 || fileSizeField == 0xFFFFFFFF)
            {
                int correctFileSize = wavData.Length - 8;
                byte[] sizeBytes = BitConverter.GetBytes(correctFileSize);
                Array.Copy(sizeBytes, 0, wavData, 4, 4);
                Debug.Log($"[AIVoiceHandler] Repaired file size header: {correctFileSize} bytes");
            }

            // Find and repair data chunk size
            for (int i = 36; i < Math.Min(wavData.Length - 8, 200); i++)
            {
                string marker = System.Text.Encoding.ASCII.GetString(wavData, i, 4);
                if (marker == "data")
                {
                    int dataSizeField = BitConverter.ToInt32(wavData, i + 4);
                    if (dataSizeField == -1 || dataSizeField == 0xFFFFFFFF)
                    {
                        int correctDataSize = wavData.Length - (i + 8);
                        byte[] dataSizeBytes = BitConverter.GetBytes(correctDataSize);
                        Array.Copy(dataSizeBytes, 0, wavData, i + 4, 4);
                        Debug.Log($"[AIVoiceHandler] Repaired data size header: {correctDataSize} bytes");
                    }
                    break;
                }
            }

            return wavData;
        }
        #endregion

        #region Helper Methods
        private void DeleteFileIfExists(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AIVoiceHandler] Failed to delete file: {e.Message}");
            }
        }

        private void CleanupTempFiles()
        {
            // Stop audio if playing when destroyed
            if (targetAudioSource != null && targetAudioSource.isPlaying)
            {
                targetAudioSource.Stop();
            }
        }

        private void Fail(string message)
        {
            Debug.LogError($"[AIVoiceHandler] ERROR: {message}");
            OnError?.Invoke(message);
        }
        #endregion

        #region Data Classes
        [Serializable]
        public class ChatResponse
        {
            public string status;
            public string message;
            public string student_id;
            public string agent_response;
            public string audio_filepath;
        }
        #endregion
    }
}