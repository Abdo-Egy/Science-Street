using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.SimpleAudioLibrary
{
    public static class AudioEditor
    {
        /// <summary>
        /// Copies AudioClip and returns a new AudioClip instance.
        /// </summary>
        public static AudioClip Copy(AudioClip source) // Workaround to get a full clip length when saving.
        {
            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var copy = AudioClip.Create(source.name + "_copy", source.samples, source.channels, source.frequency, false);

            copy.SetData(data, 0);

            return copy;
        }

        /// <summary>
        /// Cuts AudioClip by start and end time given (in seconds) and returns a new AudioClip instance.
        /// </summary>
        public static AudioClip Cut(AudioClip source, float start, float end)
        {
            var frequency = source.frequency;
            var timeLength = end - start;
            var samplesLength = (int) (frequency * timeLength);
            var data = new float[samplesLength * source.channels];

            source.GetData(data, (int) (frequency * start));

            var cut = AudioClip.Create(source.name + "_cut", samplesLength, source.channels, frequency, false);

            cut.SetData(data, 0);

            return cut;
        }

        /// <summary>
        /// Forces AudioClip (with 2 channels) to mono and returns a new AudioClip instance. Halves stereo AudioClip size.
        /// </summary>
        public static AudioClip ToMono(AudioClip source, bool average = true)
        {
            if (source.channels == 1) return source;

            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var mono = AudioClip.Create(source.name + "_mono", source.samples, 1, source.frequency, false);
            var dataMono = new float[source.samples];

            for (var i = 0; i < source.samples; i++)
            {
                dataMono[i] = average ? (data[i * source.channels] + data[i * source.channels + 1]) / 2 : data[i * source.channels];
            }

            mono.SetData(dataMono, 0);

            return mono;
        }

        /// <summary>
        /// Normalize AudioClip returns a new AudioClip instance. Used to make audio louder.
        /// </summary>
        public static AudioClip Normalize(AudioClip source)
        {
            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var max = Math.Max(data.Max(), -data.Min());

            if (Mathf.Approximately(max, 1)) return source;

            var factor = 1f / max;

            for (var i = 0; i < data.Length; i++)
            {
                data[i] *= factor;
            }

            var target = AudioClip.Create(source.name + "_normalized", source.samples, source.channels, source.frequency, false);

            target.SetData(data, 0);

            return target;
        }

        /// <summary>
        /// Changes AudioClip frequency and returns a new AudioClip instance.
        /// </summary>
        public static AudioClip SetSampleRate(AudioClip source, int frequency)
        {
            if (source.frequency == frequency) return source;
            if (source.channels != 1 && source.channels != 2) return source;

            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var samplesLength = (int) (frequency * source.length);
            var sourceChannels = new List<float[]>();
            var targetChannels = new List<float[]>();

            if (source.channels == 1)
            {
                sourceChannels.Add(data);
                targetChannels.Add(new float[(int) (frequency * source.length)]);
            }
            else
            {
                sourceChannels.Add(new float[source.samples]);
                sourceChannels.Add(new float[source.samples]);

                targetChannels.Add(new float[(int) (frequency * source.length)]);
                targetChannels.Add(new float[(int) (frequency * source.length)]);

                for (var i = 0; i < data.Length; i++)
                {
                    sourceChannels[i % 2][i / 2] = data[i];
                }
            }

            for (var c = 0; c < source.channels; c++)
            {
                var index = 0;
                var sum = 0f;
                var count = 0;
                var sourceData = sourceChannels[c];
                var targetData = targetChannels[c];

                for (var i = 0; i < sourceData.Length; i++)
                {
                    var index2 = (int) ((float) i / sourceData.Length * targetData.Length);

                    if (index2 == index)
                    {
                        sum += sourceData[i];
                        count++;
                    }
                    else
                    {
                        targetData[index] = sum / count;
                        index = index2;
                        sum = sourceData[i];
                        count = 1;
                    }
                }
            }

            if (source.channels == 1)
            {
                data = targetChannels[0];
            }
            else
            {
                data = new float[targetChannels[0].Length + targetChannels[1].Length];

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = targetChannels[i % 2][i / 2];
                }
            }

            var target = AudioClip.Create(source.name + "_" + frequency, samplesLength, source.channels, frequency, false);

            target.SetData(data, 0);

            return target;
        }

        /// <summary>
        /// Changes AudioClip speed and returns a new AudioClip instance. Default playback speed is 1.
        /// </summary>
        public static AudioClip SetSpeed(AudioClip source, float speed)
        {
            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var sourceChannels = new List<float[]>();
            var targetChannels = new List<float[]>();

            if (source.channels == 1)
            {
                sourceChannels.Add(data);
                targetChannels.Add(new float[(int) (data.Length / speed)]);
            }
            else
            {
                sourceChannels.Add(new float[source.samples]);
                sourceChannels.Add(new float[source.samples]);

                targetChannels.Add(new float[Mathf.FloorToInt(source.samples / speed)]);
                targetChannels.Add(new float[Mathf.FloorToInt(source.samples / speed)]);

                for (var i = 0; i < data.Length; i++)
                {
                    sourceChannels[i % 2][i / 2] = data[i];
                }
            }

            for (var c = 0; c < source.channels; c++)
            {
                var sourceData = sourceChannels[c];
                var targetData = targetChannels[c];

                for (var i = 0; i < targetData.Length; i++)
                {
                    targetData[i] = sourceData[Mathf.FloorToInt(i * speed)];
                }
            }

            if (source.channels == 1)
            {
                data = targetChannels[0];
            }
            else
            {
                data = new float[targetChannels[0].Length + targetChannels[1].Length];

                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = targetChannels[i % 2][i / 2];
                }
            }

            var target = AudioClip.Create(source.name + "_x" + speed, Mathf.FloorToInt(source.samples / speed), source.channels, source.frequency, false);

            target.SetData(data, 0);

            return target;
        }

        /// <summary>
        /// Removes silence (empty samples) from the beginning and from the end of AudioClip and returns a new AudioClip instance.
        /// </summary>
        public static AudioClip TrimSilence(AudioClip source, float min = 0.005f)
        {
            var data = new float[source.samples * source.channels];

            source.GetData(data, 0);

            var start = Array.FindIndex(data, i => Mathf.Abs(i) > min);
            var end = Array.FindLastIndex(data, i => Mathf.Abs(i) > min);
            var channels = source.channels;
            var frequency = source.frequency;

            if (start == 0 && end == data.Length - 1) return source;

            var trim = AudioClip.Create(source.name + "_trim", (end - start + 1) / channels, channels, frequency, stream: false);
            
            trim.SetData(data[start..end], 0);

            return trim;
        }

        /// <summary>
        /// Reads ID3 tags from audio files.
        /// </summary>
        public static TagLib.Tag ReadMeta(string path)
        {
            var file = TagLib.File.Create(path);

            return file.Tag;
        }

        /// <summary>
        /// Creates a histogram for AudioClip and returns a Texture2D instance.
        /// </summary>
        public static Texture2D CreateHistogramTexture(AudioClip clip, Color32 color, Color32 background, int width = 512, int height = 32)
        {
            var data = new float[clip.samples * clip.channels];

            clip.GetData(data, 0);

            var valuesPositive = new float[width];
            var valuesNegative = new float[width];
            var factor = (float) width / data.Length;

            if (clip.channels == 1)
            {
                for (var i = 0; i < data.Length; i += 10)
                {
                    var index = (int) (i * factor);
                    var sample = data[i];

                    if (sample > valuesPositive[index]) valuesPositive[index] = sample;
                    else if (sample < valuesNegative[index]) valuesNegative[index] = sample;
                }
            }
            else
            {
                for (var i = 0; i < data.Length; i += 20)
                {
                    var index = (int) (i * factor);
                    var sample = (data[i] + data[i + 1]) / 2f;

                    if (sample > valuesPositive[index]) valuesPositive[index] = sample;
                    else if (sample < valuesNegative[index]) valuesNegative[index] = sample;
                }
            }

            var colors = new Color32[width * height];
            
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    colors[x + y * width] = background;
                }

                for (var y = 0; y < (int) (valuesPositive[x] * height / 2f); y++)
                {
                    colors[x + (y + height / 2) * width] = color;
                }

                for (var y = (int) (valuesNegative[x] * height / 2f); y < 0; y++)
                {
                    colors[x + (y + height / 2) * width] = color;
                }
            }

            var texture = new Texture2D(width, height) { filterMode = FilterMode.Point };
            
            texture.SetPixels32(colors);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Creates a sprite for AudioClip and returns a Sprite instance.
        /// </summary>
        public static Sprite CreateHistogramSprite(AudioClip clip, Color32 color, Color32 background, int width = 512, int height = 32)
        {
            var texture = CreateHistogramTexture(clip, color, background, width, height);

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        }
    }
}