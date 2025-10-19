using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;

namespace Assets.SimpleAudioLibrary
{
    public static class AudioLoader
    {
        private static readonly Dictionary<string, AudioFormat> SupportedFormats = new()
        {
            { ".mp3",  AudioFormat.MP3 },
            { ".ogg",  AudioFormat.OGG },
            { ".wav",  AudioFormat.WAV }
        };

        /// <summary>
        /// Synchronously loads an audio clip from the specified URL and returns AudioClip instance. On large files code execution may freeze, consider using LoadFromWebAsync().
        /// </summary>
        public static AudioClip LoadFromWeb(string url, AudioType type)
        {
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);

            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null) throw new Exception(webRequest.error);

            var clip = DownloadHandlerAudioClip.GetContent(webRequest);

            if (clip == null) throw new Exception("Unable to load audio.");

            // TODO: As clips are loaded async, you may need to wait for `clip.loadState == AudioDataLoadState.Loaded`. It's better to use coroutines for this.
            // https://discussions.unity.com/t/unitywebrequestmultimedia-getaudioclip-not-working-on-webgl/1550213/4
            while (clip.loadState != AudioDataLoadState.Loaded) {}

            clip.name = url.Split('/').Last().Split('?').Last();

            return clip;
        }

        /// <summary>
        /// Asynchronously loads an audio clip from the specified URL and returns AudioClip instance in callback. Should be started as a coroutine with StartCoroutine().
        /// </summary>
        public static IEnumerator LoadFromWebAsync(string url, AudioType type, Action<bool, string, AudioClip> callback)
        {
            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(webRequest);

                if (clip == null)
                {
                    callback(false, "Unable to load audio.", null);
                }
                else
                {
                    while (clip.loadState == AudioDataLoadState.Loading) yield return null;

                    clip.name = url.Split('/').Last().Split('?').Last();
                    callback(true, null, clip);
                }
            }
            else
            {
                callback(false, webRequest.error, null);
            }
        }

        #if UNITY_WEBGL && !UNITY_EDITOR

        public static AudioClip Load(string path)
        {
            throw new NotSupportedException();
        }

        public static AudioClip Load(byte[] binary, AudioFormat format)
        {
            throw new NotSupportedException();
        }

        public static IEnumerator LoadAsync(string path, Action<bool, string, AudioClip> callback)
        {
            throw new NotSupportedException();
        }

        public static IEnumerator LoadAsync(byte[] binary, AudioFormat format, Action<bool, string, AudioClip> callback)
        {
            throw new NotSupportedException();
        }

        #else

        /// <summary>
        /// Synchronously loads an audio clip from the specified path and returns AudioClip instance. On large files code execution may freeze, consider using LoadAsync().
        /// </summary>
        public static AudioClip Load(string path)
        {
            var extension = System.IO.Path.GetExtension(path);
            var type = AudioType.UNKNOWN;

            if (extension != null && SupportedFormats.ContainsKey(extension))
            {
                switch (SupportedFormats[extension])
                {
                    case AudioFormat.MP3: type = AudioType.MPEG; break;
                    case AudioFormat.OGG: type = AudioType.OGGVORBIS; break;
                    case AudioFormat.WAV: type = AudioType.WAV; break;
                }
            }

            var url = $"file://{path}";

            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);

            webRequest.SendWebRequest();

            while (!webRequest.isDone) { }

            if (webRequest.error != null) throw new Exception(webRequest.error);

            var clip = DownloadHandlerAudioClip.GetContent(webRequest);

            if (clip == null) throw new Exception("Unable to load audio.");

            // TODO: As clips are loaded async, you may need to wait for `clip.loadState == AudioDataLoadState.Loaded`. It's better to use coroutines for this.
            // https://discussions.unity.com/t/unitywebrequestmultimedia-getaudioclip-not-working-on-webgl/1550213/4
            while (clip.loadState != AudioDataLoadState.Loaded) {}

            clip.name = System.IO.Path.GetFileName(path);

            return clip;
        }

        /// <summary>
        /// Synchronously loads an audio clip from bytes and returns AudioClip instance. On large files code execution may freeze, consider using LoadAsync().
        /// </summary>
        public static AudioClip Load(byte[] binary, AudioFormat format)
        {
            if (SupportedFormats.All(i => i.Value != format)) throw new NotSupportedException("Unsupported format.");

            var path = System.IO.Path.Combine(Application.persistentDataPath, "temp" + SupportedFormats.Single(i => i.Value == format).Key);

            System.IO.File.WriteAllBytes(path, binary);

            try
            {
                return Load(path);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Asynchronously loads an audio clip from the specified path and returns AudioClip instance in callback. Should be started as a coroutine with StartCoroutine().
        /// </summary>
        public static IEnumerator LoadAsync(string path, Action<bool, string, AudioClip> callback)
        {
            var extension = System.IO.Path.GetExtension(path);
            var type = AudioType.UNKNOWN;

            if (extension != null && SupportedFormats.ContainsKey(extension))
            {
                switch (SupportedFormats[extension])
                {
                    case AudioFormat.MP3: type = AudioType.MPEG; break;
                    case AudioFormat.OGG: type = AudioType.OGGVORBIS; break;
                    case AudioFormat.WAV: type = AudioType.WAV; break;
                }
            }

            var url = $"file://{path}";

            using var webRequest = UnityWebRequestMultimedia.GetAudioClip(url, type);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var clip = DownloadHandlerAudioClip.GetContent(webRequest);

                if (clip == null)
                {
                    callback(false, "Unable to load audio.", null);
                }
                else
                {
                    while (clip.loadState == AudioDataLoadState.Loading) yield return null;

                    clip.name = System.IO.Path.GetFileName(path);
                    callback(true, null, clip);
                }
            }
            else
            {
                callback(false, webRequest.error, null);
            }
        }

        /// <summary>
        /// Asynchronously loads audio an clip from bytes and returns AudioClip instance in callback. Should be started as a coroutine with StartCoroutine().
        /// </summary>
        public static IEnumerator LoadAsync(byte[] binary, AudioFormat format, Action<bool, string, AudioClip> callback)
        {
            if (SupportedFormats.All(i => i.Value != format)) throw new NotSupportedException("Unsupported format.");

            var path = System.IO.Path.Combine(Application.persistentDataPath, "temp" + SupportedFormats.Single(i => i.Value == format).Key);

            System.IO.File.WriteAllBytes(path, binary);

            try
            {
                yield return LoadAsync(path, callback);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        #endif
    }
}