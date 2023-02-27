using System;
using UnityEngine;

namespace KitchenPurrBliss.Utils
{
    public static class AudioUtils
    {
        public class Wav
        {
            public byte[] RawData { get; private set; }
            public int ChannelCount { get; private set; }
            public int Frequency { get; private set; }
            public int SampleCount { get; private set; }
            public float[] LeftChannel { get; private set; }
            public float[] RightChannel { get; private set; }

            public Wav(byte[] data)
            {
                RawData = data;

                ChannelCount = BitConverter.ToInt16(data, 22);
                Frequency = BitConverter.ToInt32(data, 24);
                SampleCount = BitConverter.ToInt32(data, 40) / (ChannelCount * 2);

                LeftChannel = new float[SampleCount];
                RightChannel = new float[SampleCount];

                int byteOffset = 44;
                for (int i = 0; i < SampleCount; i++)
                {
                    LeftChannel[i] = (float)BitConverter.ToInt16(data, byteOffset) / 32768f;
                    byteOffset += 2;
                    if (ChannelCount == 2)
                    {
                        RightChannel[i] = (float)BitConverter.ToInt16(data, byteOffset) / 32768f;
                        byteOffset += 2;
                    }
                }
            }
        }

        public static AudioClip LoadWavFromAssetBundle(AssetBundle bundle, string assetPath)
        {
            var asset = bundle.LoadAsset<AudioClip>(assetPath);

            if (asset == null)
            {
                Debug.LogError($"Failed to load asset {assetPath} from AssetBundle");
                return null;
            }

            return asset;
        }
    }
}
