using UnityEngine;
using System;

namespace JinkeGroup.TalkBack
{
    public class ProcessedSound
    {
        public float[] Data;
        public int Length;
        public float[] TalkFrames;
        public int TalkFramesLength;

        public readonly int Channels;
        public readonly int SampleRate;
        public readonly int TalkFramesPerSecond;

        public ProcessedSound(TalkBackSettings listenAndRepeatSettings)
        {
            int maxDataLen = listenAndRepeatSettings.SampleRate * listenAndRepeatSettings.MaxRecordingTime * 2;
            int maxTalkFrames = Mathf.RoundToInt(listenAndRepeatSettings.MaxRecordingTime * listenAndRepeatSettings.TalkFramesPerSecond) + 1;

            Data = new float[maxDataLen];
            TalkFrames = new float[maxTalkFrames];
            SampleRate = listenAndRepeatSettings.SampleRate;
            TalkFramesPerSecond = listenAndRepeatSettings.TalkFramesPerSecond;
            Channels = listenAndRepeatSettings.MicrophoneChannels;
        }
        public void CopyTo(ProcessedSound processedSound)
        {
            if (processedSound.Channels != Channels)
                throw new InvalidOperationException("Can't copy!");
            if (processedSound.SampleRate != SampleRate)
                throw new InvalidOperationException("Can't copy!");
            if (processedSound.TalkFramesPerSecond != TalkFramesPerSecond)
                throw new InvalidOperationException("Can't copy!");
            if (processedSound.Data.Length != Data.Length)
                throw new InvalidOperationException("Can't copy!");
            if (processedSound.TalkFrames.Length != TalkFrames.Length)
                throw new InvalidOperationException("Can't copy!");

            processedSound.Length = Length;
            processedSound.TalkFramesLength = TalkFramesLength;
            Array.Copy(Data, processedSound.Data, Data.Length);
            Array.Copy(TalkFrames, processedSound.TalkFrames, TalkFrames.Length);
        }

        public void ResetAndClear()
        {
            Array.Clear(TalkFrames, 0, TalkFrames.Length);
            Array.Clear(Data, 0, Data.Length);
            TalkFramesLength = 0;
            Length = 0;
        }

        public float TalkFrame(float time)
        {
            int pos = Mathf.RoundToInt(time / (SampleRate / TalkFramesPerSecond));
            if (pos >= TalkFrames.Length)
            {
                pos = TalkFrames.Length - 1;
            }
            return TalkFrames[pos];
        }
    }
}
