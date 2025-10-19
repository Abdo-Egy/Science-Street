using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Assets.SimpleAudioLibrary.Demo
{
    public class Demo : MonoBehaviour
    {
        public Text Log;
        public AudioSource AudioSource;
        public Image TestResult;
        public Image Histogram;
        public RectTransform PlaybackPosition;

        public void Start()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        public void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            Log.text += condition + '\n';
        }
        
        public void LoadExample()
        {
            var asset = (TextAsset)Resources.Load("MP3");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.MP3);

            AudioSource.PlayOneShot(clip);
            Debug.Log($"MP3 sample loaded: {clip.length}s.");
        }

        public void LoadAsyncExample()
        {
            StartCoroutine(LoadCoroutine());

            IEnumerator LoadCoroutine()
            {
                var asset = (TextAsset)Resources.Load("MP3");

                yield return AudioLoader.LoadAsync(asset.bytes, AudioFormat.MP3, OnLoad);
            }

            void OnLoad(bool success, string error, AudioClip clip)
            {
                if (success) { AudioSource.PlayOneShot(clip); Debug.Log($"MP3 sample loaded: {clip.length}s."); } else Debug.LogError(error);
            }
        }

        public void EncodeExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip1 = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var clip2 = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var mp3 = AudioEncoder.Encode(clip1, AudioFormat.MP3);
            var ogg = AudioEncoder.Encode(clip2, AudioFormat.OGG);

            Debug.Log($"WAV sample encoded to MP3: {mp3.Length} bytes (-{100 - 100 * mp3.Length / asset.bytes.Length:0.##}%).");
            Debug.Log($"WAV sample encoded to OGG: {ogg.Length} bytes (-{100 - 100 * ogg.Length / asset.bytes.Length:0.##}%).");
        }

        public void CopyExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var copy = AudioEditor.Copy(clip);

            AudioSource.PlayOneShot(copy);
            Debug.Log($"Copied: {copy.length}s.");
        }

        public void CutExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var cut = AudioEditor.Cut(clip, 1, 2);

            AudioSource.PlayOneShot(cut);
            Debug.Log($"Cut: {cut.length}s.");
        }

        public void ChangeSpeedExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var fast = AudioEditor.SetSpeed(clip, 2);

            AudioSource.PlayOneShot(fast);
            Debug.Log($"Speed changed: {fast.length}s.");
        }

        public void ForceToMonoExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var mono = AudioEditor.ToMono(clip);

            AudioSource.PlayOneShot(mono);
            Debug.Log($"Forced to mono, channels: {mono.channels}.");
        }

        public void ChangeSampleRateExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);
            var low = AudioEditor.SetSampleRate(clip, 8000);

            AudioSource.PlayOneShot(low);
            Debug.Log($"Frequency changed: {low.frequency}.");
        }

        public void ReadTagsExample()
        {
            #if UNITY_EDITOR

            var source = Path.Combine(Environment.CurrentDirectory, "Assets/SimpleAudioLibrary/Demo/Resources/MP3.bytes");
            var path = Path.Combine(Application.temporaryCachePath, "Sample.mp3");

            File.Copy(source, path);

            var tags = AudioEditor.ReadMeta(path);
            var performer = tags.FirstPerformer;
            var title = tags.Title;
            var genre = tags.FirstGenre;

            File.Delete(path);

            Debug.Log($"ID3 tags: Performer={performer}, Title={title}, Genre={genre}");

            #else

            Debug.Log("This example works in Editor only.");

            #endif
        }

        public void BuildHistogramExample()
        {
            var asset = (TextAsset)Resources.Load("WAV");
            var clip = AudioLoader.Load(asset.bytes, AudioFormat.WAV);

            Histogram.sprite = AudioEditor.CreateHistogramSprite(clip, new Color32(100, 100, 200, 255), new Color32(0, 0, 0, 50), 512, 64);
            Histogram.color = Color.white;
            AudioSource.clip = clip;
            AudioSource.Play();
        }

        public void Update()
        {
            if (AudioSource.isPlaying && AudioSource.clip != null)
            {
                PlaybackPosition.anchoredPosition = new Vector2(Histogram.GetComponent<RectTransform>().rect.width * AudioSource.time / AudioSource.clip.length, 0);
            }
        }

        public void OpenWiki()
        {
            Application.OpenURL("https://github.com/hippogamesunity/Assets/wiki/Simple-Audio-Library");
        }

        public void TestRuntime()
        {
            try
            {
                var mp3Asset = (TextAsset)Resources.Load("MP3");
                var oggAsset = (TextAsset)Resources.Load("OGG");
                var wavAsset = (TextAsset)Resources.Load("WAV");

                Assert.AreEqual(mp3Asset.bytes.Length, 86361);
                Assert.AreEqual(oggAsset.bytes.Length, 79316);
                Assert.AreEqual(wavAsset.bytes.Length, 1048558);

                var mp3 = AudioLoader.Load(mp3Asset.bytes, AudioFormat.MP3);
                var ogg = AudioLoader.Load(oggAsset.bytes, AudioFormat.OGG);
                var wav = AudioLoader.Load(wavAsset.bytes, AudioFormat.WAV);

                Assert.AreEqual(mp3.samples, 263808);
                Assert.AreEqual(ogg.samples, 262094);
                Assert.AreEqual(wav.samples, 262094);

                var mp3Bin = AudioEncoder.Encode(wav, AudioFormat.MP3);
                var oggBin = AudioEncoder.Encode(wav, AudioFormat.OGG);
                var wavBin = AudioEncoder.Encode(wav, AudioFormat.WAV);

                Assert.IsTrue(mp3Bin.Length > 30000, $"Invalid MP3 size: {mp3Bin.Length} bytes, expected: >30000."); // Depends on platform implementation.
                Assert.IsTrue(oggBin.Length > 30000, $"Invalid OGG size: {oggBin.Length} bytes, expected: >30000."); // Depends on platform implementation.
                Assert.AreEqual(wavBin.Length, 1048420);

                mp3 = AudioLoader.Load(mp3Bin, AudioFormat.MP3);
                ogg = AudioLoader.Load(oggBin, AudioFormat.OGG);
                wav = AudioLoader.Load(wavBin, AudioFormat.WAV);

                Assert.IsTrue(mp3.length > 4, $"Invalid audio size when decoding MP3: {mp3.length}.");
                Assert.IsTrue(ogg.length > 4, $"Invalid audio size when decoding OGG: {mp3.length}.");
                Assert.IsTrue(wav.length > 4, $"Invalid audio size when decoding WAV: {mp3.length}.");

                Debug.Log("All tests passed!");
                TestResult.color = Color.green;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                TestResult.color = Color.red;
            }
        }
    }
}