using JinkeGroup.Logic;
using UnityEngine;

namespace JinkeGroup.TalkBack{
	public class TalkBackSettings:BucketUpdateBehaviour{
	    protected const float maxFloatInt = 32767;

	    public float SoundTouchTempo = 0.0f;
	    public float SoundTouchPitch = 0.0f;
	    public float SoundTouchRate = 0.0f;

	    public int MinRecordingTime = 5;
	    public int MaxRecordingTime = 15;
	    public int TalkFramesPerSecond = 15;
	    public float TalkFrameFrequencyThreshold = 150.0f;
	    public float MinTalkFrameVolume = 0.01f;

	    public int SoundTouchMaxSamplesToProcess = 1024;
	    public float ListenSilenceStartFactor = 1.3f;
	    public float ListenSilenceEndFactor = 1.2f;
	    public int ListenAverageFramesCount = 5;
	    public float ListenStartRecordingFactor = 50.0f / maxFloatInt;
	    public int ListenStopMaxSilenceChunks = 5;
	    public float ListenNormalizationFactor = 25000.0f / maxFloatInt;
	    public float ListenNoiseLimit = 900 / maxFloatInt;
	    public float ListenSilenceEnviromentAbsDeltaStart = 2000.0f / maxFloatInt;
	    public float ListenSilenceEnviromentAbsDeltaEnd = 1000.0f / maxFloatInt;
	    public int TalkFrameBins = 32;
	    public int MicrophoneChannels = 1;
	    public string MicrophoneDeviceName = null;
	    public float MicrophoneRecordingStartDelay = 0.1f;
	    public float ListenEndCutoff = 0.0f;
	    public float ListenEndFadeout = 0.0f;

	    public virtual bool ListeningEnabled
	    {
	        get
	        {
	            return Microphone.devices != null && Microphone.devices.Length > 0;
	        }
	    }

	    public virtual int SampleRate
	    {
	        get
	        {
#if UNITY_ANDROID && !(UNITY_EDITOR || NATIVE_SIM)
                    return JinkeGroup.Util.AndroidPluginManager.Instance.CallAnActivityRef<int>("getAudioManager","getMicrophoneSampleRate");
#else
	            return 16000;
#endif
	        }
	    }
    }
}
