// This software is based on SavWav.cs from https://gist.github.com/darktable/2317063 (Copyright (c) 2012 Calvin Rien).
// Hippo Games made bug fixes and code refactoring (Copyright (c) 2021 Oleg Mekekechko).

using System;
using System.IO;
using UnityEngine;

namespace Assets.SimpleAudioLibrary.ThirdParty
{
    /// <summary>
    /// Used to save AudioClip in Waveform Audio File (WAV) format.
    /// </summary>
    public static class WavWriter
    {
        /// <summary>
        /// Saves AudioClip to Waveform Audio File (WAV) and returns MemoryStream.
        /// </summary>
        public static MemoryStream SaveToStream(AudioClip clip)
        {
            var stream = new MemoryStream();

            WriteHeader(stream, clip);
            WriteSamples(stream, clip);

            return stream;
        }

        /// <summary>
        /// Saves AudioClip to Waveform Audio File (WAV) and a byte array.
        /// </summary>
        public static byte[] SaveToBinary(AudioClip clip)
        {
            using (var stream = SaveToStream(clip))
            {
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Saves AudioClip as Waveform Audio File (WAV) to the specified path.
        /// </summary>
        public static void SaveToFile(string path, AudioClip clip)
        {
            File.WriteAllBytes(path, SaveToBinary(clip));
        }

        private static void WriteHeader(Stream stream, AudioClip clip)
        {
            const int HEADER_SIZE = 44;

            for (var i = 0; i < HEADER_SIZE; i++)
            {
                stream.WriteByte(0);
            }

            stream.Seek(0, SeekOrigin.Begin);

            var riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
            stream.Write(riff, 0, 4);

            var chunkSize = BitConverter.GetBytes(stream.Length - 8);
            stream.Write(chunkSize, 0, 4);

            var wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
            stream.Write(wave, 0, 4);

            var fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
            stream.Write(fmt, 0, 4);

            var subChunk1 = BitConverter.GetBytes(16);
            stream.Write(subChunk1, 0, 4);

            var audioFormat = BitConverter.GetBytes(1);
            stream.Write(audioFormat, 0, 2);

            var numChannels = BitConverter.GetBytes(clip.channels);
            stream.Write(numChannels, 0, 2);

            var sampleRate = BitConverter.GetBytes(clip.frequency);
            stream.Write(sampleRate, 0, 4);

            var byteRate = BitConverter.GetBytes(clip.frequency * clip.channels * 2);
            stream.Write(byteRate, 0, 4);

            var blockAlign = (ushort) (clip.channels * 2);
            stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

            const ushort bps = 16;
            var bitsPerSample = BitConverter.GetBytes(bps);
            stream.Write(bitsPerSample, 0, 2);

            var data = System.Text.Encoding.UTF8.GetBytes("data");
            stream.Write(data, 0, 4);

            var subChunk2 = BitConverter.GetBytes(clip.samples * clip.channels * 2);
            stream.Write(subChunk2, 0, 4);
        }

        private static void WriteSamples(Stream steam, AudioClip clip)
        {
            var samples = new float[clip.samples * clip.channels];

            clip.GetData(samples, 0);

            var shorts = new short[samples.Length];
            var data = new byte[samples.Length * 2];
            const int rescaleFactor = 32767;

            for (var i = 0; i < samples.Length; i++)
            {
                shorts[i] = (short) (samples[i] * rescaleFactor);
                BitConverter.GetBytes(shorts[i]).CopyTo(data, i * 2);
            }

            steam.Write(data, 0, data.Length);
        }
    }
}