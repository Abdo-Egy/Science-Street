using UnityEngine;

namespace Assets.SimpleAudioLibrary
{
    /// <summary>
    /// Extends AudioClip class with new saving features provided by AudioClipManager.
    /// </summary>
    public static class Extensions
    {
        public static byte[] Encode(this AudioClip clip, AudioFormat format, int bitRate = 128, float quality = 0.4f)
        {
            return AudioEncoder.Encode(clip, format, bitRate, quality);
        }

        #if !UNITY_WEBGL

        public static void Save(this AudioClip clip, string path, AudioFormat format, int bitRate = 128, float quality = 0.4f)
        {
            AudioEncoder.Encode(path, clip, format, bitRate, quality);
        }

        #endif
    }
}