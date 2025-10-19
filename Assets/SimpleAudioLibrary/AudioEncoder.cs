using System;
using Assets.SimpleAudioLibrary.ThirdParty;
using NAudio.Wave.WZT;
using OggVorbis;
using UnityEngine;

namespace Assets.SimpleAudioLibrary
{
    /// <summary>
    /// Performs audio clip encoding.
    /// </summary>
    public static class AudioEncoder
    {
        /// <summary>
        /// Performs audio clip encoding to the selected format.
        /// </summary>
        public static byte[] Encode(AudioClip clip, AudioFormat format, int bitRate = 128, float quality = 0.4f)
        {
            switch (format)
            {
                case AudioFormat.WAV: return EncodeToWav(clip);
                case AudioFormat.MP3: return EncodeToMp3(clip, bitRate);
                case AudioFormat.OGG: return EncodeToOgg(clip, quality);
                default: throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Saves AudioClip as Waveform Audio File (WAV) and returns a byte array.
        /// </summary>
        public static byte[] EncodeToWav(AudioClip clip)
        {
            return WavWriter.SaveToBinary(clip);
        }

        /// <summary>
        /// Saves AudioClip as MP3 and returns a byte array.
        /// </summary>
        public static byte[] EncodeToMp3(AudioClip clip, int bitRate = 128)
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

            using var stream = WavWriter.SaveToStream(clip);

            stream.Seek(0, System.IO.SeekOrigin.Begin); // Important for WaveFileReader.

            var reader = new WaveFileReader(stream);

            using var ms = new System.IO.MemoryStream();
            var lame = new NAudio.Lame.LameMP3FileWriter(ms, reader.WaveFormat, bitRate);

            reader.CopyTo(lame);

            return ms.ToArray();

            #else // For unsupported platforms, managed C# codecs are used.

            var data = new float[clip.samples * clip.channels];

            clip.GetData(data, 0);

            return ManagedCodecs.Mp3Encoder.Encode(data, clip.channels, clip.frequency, bitRate);
            
            #endif
        }

        /// <summary>
        /// Saves AudioClip as Ogg Vorbis (OGG) and returns a byte array.
        /// </summary>
        public static byte[] EncodeToOgg(AudioClip clip, float quality = 0.4f)
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

            return VorbisPlugin.GetOggVorbis(clip, quality);

            #else // For unsupported platforms, managed C# codecs are used.

            var data = new float[clip.samples * clip.channels];

            clip.GetData(data, 0);

            return ManagedCodecs.OggEncoder.Encode(data, clip.channels, clip.frequency, quality);
            
            #endif
        }

        #if !UNITY_WEBGL || UNITY_EDITOR

        /// <summary>
        /// Performs audio clip encoding to the selected format.
        /// </summary>
        public static void Encode(string path, AudioClip clip, AudioFormat format, int bitRate = 128, float quality = 0.4f)
        {
            System.IO.File.WriteAllBytes(path, Encode(clip, format, bitRate, quality));
        }

        /// <summary>
        /// Saves AudioClip as Waveform Audio File (WAV) to the specified path.
        /// </summary>
        public static void EncodeToWav(AudioClip clip, string path)
        {
            WavWriter.SaveToFile(path, clip);
        }

        /// <summary>
        /// Saves AudioClip as MP3 to the specified path.
        /// </summary>
        public static void EncodeToMp3(AudioClip clip, string path, int bitRate = 128)
        {
            System.IO.File.WriteAllBytes(path, EncodeToMp3(clip, bitRate));
        }

        /// <summary>
        /// Saves AudioClip as Ogg Vorbis (OGG) to the specified path.
        /// </summary>
        public static void EncodeToOgg(AudioClip clip, string path, float quality = 0.4f)
        {
            System.IO.File.WriteAllBytes(path, EncodeToOgg(clip, quality));
        }

        #endif
    }
}