//#define DEBUG_VERBOSE

using UnityEngine;
using System;
using JinkeGroup.Logic;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace JinkeGroup.TalkBack
{
    public class TalkBackHandler : BucketUpdateBehaviour{
        private const string Tag = "TalkBackHandler";
        public struct TalkFrame
        {
            public float Frequency;
            public float Volume;
            public float StartTime;
            public float EndTime;
        }
        public List<TalkFrame> TalkFrames = new List<TalkFrame>(50);
        public TalkBackSettings TalkBackSettings;
        public MicrophoneHandler MicrophoneHandler;
        public AudioMixerGroup TalkBackMixerGroup;//混音器

        public Action CallbackRecordingStarted = null;
        public Action<float> CallbackRecordingStopped = null;
        public Action<bool> CallbackTalkingStopped = null;
        public Action<float> CallbackTalk = null;

        private AudioSource AudioSource { get; set; }
        private ProcessedSound ProcessedSound;
        private bool Playing;
        public bool Listening { get; private set; }

        public bool CanTalk { get; private set; }
        public bool Talking
        {
            get
            {
                return AudioSource.isPlaying;
            }
        }

        public float Length
        {
            get
            {
                return AudioSource.clip != null ? AudioSource.clip.length : 0.0f;
            }
        }
        public float TalkPosition
        {
            get
            {
                if (!Talking)
                    return 0;
                return ProcessedSound.TalkFrame(AudioSource.timeSamples);
            }
        }

        public bool Mute
        {
            get { return AudioSource.mute; }
            set { AudioSource.mute = value; }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.outputAudioMixerGroup = TalkBackMixerGroup;

            ProcessedSound = new ProcessedSound(TalkBackSettings);
            MicrophoneHandler.OnRecordingStarted = RecordingStarted;
            MicrophoneHandler.Init();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            StopListening();
            StopRepeating(true);
        }

        public override void OnPostUpdate(float deltaTime)
        {
            base.OnPostUpdate(deltaTime);
            if (Playing)
            {
                if (AudioSource.isPlaying)
                    UpdateTalkFrame();
                else
                    TalkingStopped();
            }
        }

        public void Stop()
        {
            StopListening();
            StopRepeating(false);
        }

#if UNITY_EDITOR
        public void PlayPrerecordedSound(AudioClip clip)
        {
            StopListening();
            TryStartListening();
            MicrophoneHandler.PlayPrerecordedSound(clip);
        }
#endif
        public TalkFrame GetTalkFrame(float offset)
        {
            float time = Talking ? (float)AudioSource.timeSamples / (float)ProcessedSound.SampleRate : 0.0f;
            time += offset;
            for(int i = 0; i < TalkFrames.Count; ++i)
            {
                TalkFrame talkFrame = TalkFrames[i];
                if (time > talkFrame.StartTime && time < talkFrame.EndTime)
                    return talkFrame;
            }
            return new TalkFrame();
        }

        private float CalculateZeroCrossingFrequency(int sampleRate, float[] audioData, int start,int end)
        {
            int numSamples = end - start;
            int numCrossing = 0;
            for(int p = start; p < end - 1; p++)
            {
                if((audioData[p]>0 && audioData[p + 1] <= 0) ||
                    (audioData[p]<0 && audioData[p+1]>=0))
                {
                    numCrossing++;

                }
            }
            float numSecondsRecorded = (float)numSamples / (float)sampleRate;
            float numCycles = numCrossing / 2;
            float frequency = numCycles / numSecondsRecorded;
            return frequency;

        }
        private void BuildTalkFrames()
        {
            TalkFrames.Clear();
            float previousFrequency = -1.0f;
            int frameIndex = 0;
            float frameVolume = -1.0f;
            for (int i = 0; i < ProcessedSound.TalkFramesLength - 1; ++i)
            {
                float volume = ProcessedSound.TalkFrames[i];
                int start = i * (ProcessedSound.SampleRate / ProcessedSound.TalkFramesPerSecond);
                int end = (i + 1) * (ProcessedSound.SampleRate / ProcessedSound.TalkFramesPerSecond);
                float frequency = CalculateZeroCrossingFrequency(ProcessedSound.SampleRate, ProcessedSound.Data, start, end);
                frameVolume = Mathf.Max(volume, frameVolume);
                if (previousFrequency > 0 && frameVolume > TalkBackSettings.MinTalkFrameVolume && (i - frameIndex) > 5 && (Mathf.Abs(previousFrequency - frequency) > TalkBackSettings.TalkFrameFrequencyThreshold || i == ProcessedSound.TalkFramesLength - 2))
                {
                    TalkFrame talkFrame = new TalkFrame();
                    talkFrame.Frequency = frequency;
                    talkFrame.Volume = frameVolume;
                    talkFrame.StartTime = (float)frameIndex / (float)ProcessedSound.TalkFramesPerSecond;
                    talkFrame.EndTime = (float)i / (float)ProcessedSound.TalkFramesPerSecond;
                    TalkFrames.Add(talkFrame);
                    frameIndex = i;
                    frameVolume = -1.0f;
                }
                previousFrequency = frequency;
            }
#if DEBUG_VERBOSE
            for (int i = 0; i < TalkFrames.Count; ++i) {
                TalkFrame talkFrame = TalkFrames[i];
                JinkeGroup.Util.Logger.DebugT(Tag, "TalkFrame #{0} {1} {2} {3} {4}", i, talkFrame.Volume, talkFrame.Frequency, talkFrame.StartTime, talkFrame.EndTime);
            }
#endif
        }
        private void OnConvertingDone(ProcessedSound processedSound)
        {
            Listening = false;
            CanTalk = true;

            processedSound.CopyTo(ProcessedSound);

            AudioClip ac = AudioClip.Create("Recorded sample", ProcessedSound.Length, ProcessedSound.Channels, TalkBackSettings.SampleRate, false);
            ac.SetData(ProcessedSound.Data, 0);

            AudioSource.clip = ac;

            if (CallbackRecordingStopped != null)
            {
                CallbackRecordingStopped(ac.length);
            }

            BuildTalkFrames();

#if DEBUG_VERBOSE
            JinkeGroup.Util.Logger.DebugT(Tag, "OnConvertingDone: Data Length: {0}", ProcessedSound.Length);
#endif
        }

        public void TryStartListening()
        {
            if (MicrophoneHandler.MicrophoneState != MicrophoneHandler.MicState.Idle)
            {
                return;
            }

            MicrophoneHandler.AcquireMicrophone();
            MicrophoneHandler.OnConvertingDone = OnConvertingDone;

            CanTalk = false;
#if DEBUG_VERBOSE
            O7Log.DebugT(Tag, "TryStartListening");
#endif
        }

        public void StopListening()
        {
            Listening = false;

            MicrophoneHandler.ReleaseMicrophone();
            MicrophoneHandler.OnConvertingDone = null;
        }

        public void StopRepeating(bool interrupted)
        {
            if (AudioSource != null)
            {
                AudioSource.Stop();
            }

            CanTalk = false;

            if (Playing)
            {
                TalkingStopped(interrupted);
            }
#if DEBUG_VERBOSE
            O7Log.DebugT(Tag, "StopRepeating {0}", interrupted);
#endif
        }

        private void PlayRecordedSound()
        {
            Listening = false;
            CanTalk = false;

            AudioSource.Stop();

#if DEBUG_VERBOSE
            O7Log.DebugT(Tag, "PlayRecordedSound: Data Length: {0}", ProcessedSound.Length);
#endif

            AudioSource.volume = 1.0f;
            AudioSource.loop = false;
            AudioSource.Play();

            Playing = true;

            UpdateTalkFrame();
        }

        public void StartTalking()
        {
#if DEBUG_VERBOSE
            O7Log.DebugT(Tag, "StartTalking: {0}", CanTalk);
#endif
            if (!CanTalk)
            {
                return;
            }
            CanTalk = false;
            PlayRecordedSound();

        }

        void UpdateTalkFrame()
        {
            float talkFrame = ProcessedSound.TalkFrame(AudioSource.timeSamples);

            if (CallbackTalk != null)
            {
                CallbackTalk(talkFrame);
            }
        }

        void RecordingStarted()
        {
            Listening = true;

            if (CallbackRecordingStarted != null)
            {
                CallbackRecordingStarted();
            }
        }

        void TalkingStopped()
        {
            TalkingStopped(false);
        }

        void TalkingStopped(bool interruped)
        {
            Playing = false;

            if (CallbackTalkingStopped != null)
            {
                CallbackTalkingStopped(interruped);
            }
        }



    }
}