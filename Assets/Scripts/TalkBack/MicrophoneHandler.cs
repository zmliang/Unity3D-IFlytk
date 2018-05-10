#if !UNITY_ANDROID || UNITY_EDITOR || NATIVE_SIM
#define READHEADPOSITION
#endif

using System;
using UnityEngine;
using JinkeGroup.Logic;

namespace JinkeGroup.TalkBack{
	public class MicrophoneHandler:BucketUpdateBehaviour{
		public const string TAG = "MicrophoneHandler";

		public enum MicState{
			Idle,
			AquiringMicrophone,
			AquiringSilenceBarrier,
			AnalysingSound,
			Recording,
			Processing,
			Processed
		}

	    public TalkBackSettings TalkBackSettings;

		public Action OnRecordingStarted = null;
		//处理声音
	    public Action<ProcessedSound> OnConvertingDone;

		#if READHEADPOSITION
		private int ReadHeadPosition;
		#endif

		private AudioClip RecordedClip;

		private RecordingBuffer RecordingBuffer;
		private float RecordingStartTime;
		private MicState microphoneState;

		private float[] DataBuffer;
		private const int DefaultDataBufferSize = 2000;

		private bool Active{
			get{
				return !(MicrophoneState == MicState.Idle || MicrophoneState == MicState.Processed);
			}
		}

		public void Init(){
			RecordingBuffer = new RecordingBuffer (TalkBackSettings);
		}

#if  UNITY_EDITOR
	    private bool Prerecorded = false;
	    public void PlayPrerecordedSound(AudioClip clip)
	    {
	        Prerecorded = true;
            float[] data = new float[clip.samples];
	        clip.GetData(data, 0);
	        RecordingBuffer.AddSamples(data,data.Length);

	        MicrophoneState = MicState.Processing;
	        RecordingBuffer.StartConverting();
	    }
#endif

	    public MicState MicrophoneState
	    {
	        
            get { return microphoneState; }
	        private set
	        {
	            microphoneState = value;
#if DEBUG_VERBOSE
                
#endif
	            switch (microphoneState)
	            {
                    case MicState.Processed:
                        if (OnConvertingDone != null)
                            OnConvertingDone(RecordingBuffer.ProcessedSound);
                        break;
                    case MicState.Recording:
                        if (OnRecordingStarted != null)
                            OnRecordingStarted();
                        break;
                    case MicState.Processing:
                        RecordingBuffer.SetDoneCollectingData();
                        break;
                    default:
                        break;
	            }
            }
	    }

	    public void Stop()
	    {
	        RecordingBuffer.Abort();
	        RecordingBuffer.ClearBuffer();

	        ReleaseMicrophone();
#if !UNITY_EDITOR && UNITY_ANDROID
            //Android has to release the microphone on app pause.
#else
            Microphone.End(TalkBackSettings.MicrophoneDeviceName);
#if DEBUG_VERBOSE
	        Debug.Log("Microphone stoped");
#endif
#endif
        }

	    public void Reset()
	    {
	        if (RecordingBuffer!=null)
	        {
	            RecordingBuffer.ClearBuffer();
	        }
	    }

	    public void AcquireMicrophone()
	    {
	        if (Active)
	        {
	            return;
	        }
#if UNITY_EDITOR
	        Prerecorded = false;
#endif
	        ReleaseMicrophone();
#if READHEADPOSITION
	        ReadHeadPosition = 0;
#endif
	        RecordingStartTime = Time.time;
            if(Microphone.devices.Length == 0)
                return;
	        bool isRecording = false;
#if UNITY_ANDROID && !(UNITY_EDITOR || NATIVE_SIM)
            isRecording = JinkeGroup.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>("getAudioManager", "resetBuffer");
#else
	        isRecording = Microphone.IsRecording(TalkBackSettings.MicrophoneDeviceName);
#endif
	        if (!isRecording)
	        {
#if DEBUG_VERBOSE
                Debug.Log("Microphone started");
#endif

#if UNITY_ANDROID && !(UNITY_EDITOR || NATIVE_SIM)
                JinkeGroup.Util.AndroidPluginManager.Instance.CallAnActivityRef("getAudioManager", "acquireMicrophone");
#else
	            RecordedClip = Microphone.Start(TalkBackSettings.MicrophoneDeviceName, true,
	            TalkBackSettings.MinRecordingTime,TalkBackSettings.SampleRate);
#endif
	            if (DataBuffer == null)
	                DataBuffer = new float[DefaultDataBufferSize];
	        }
	        RecordingBuffer.InitSamplesBuffer();
	        MicrophoneState = MicState.AquiringMicrophone;
	    }

	    public void ReleaseMicrophone()
	    {
	        MicrophoneState = MicState.Idle;
	        if (RecordingBuffer!=null)
	        {
	            RecordingBuffer.Abort();
	        }
	    }

	    private bool IsMicrophoneAcquired()
	    {
#if UNITY_ANDROID && !(UNITY_EDITOR || NATIVE_SIM)
            return JinkeGroup.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>("getAudioManager", "isMicrophoneAcquired");
#else
	        return Microphone.GetPosition(TalkBackSettings.MicrophoneDeviceName) > 0;
#endif
	    }

	    private void Update()
	    {
	        switch (MicrophoneState)
	        {
                case MicState.AquiringMicrophone:
                    if (IsMicrophoneAcquired())
                    {
#if READHEADPOSITION
                        ReadHeadPosition = Microphone.GetPosition(TalkBackSettings.MicrophoneDeviceName); // reset read head
#endif
                        MicrophoneState = MicState.AquiringSilenceBarrier;
                    }
                    break;
                case MicState.AquiringSilenceBarrier:
                    AddSamplesToRecordingBuffer();
                    if (RecordingBuffer.EnoughtSamplesForSilenceDetection)
                    {
                        RecordingBuffer.CalculateSilenceBarrier();
                        // start analysing sound
                        MicrophoneState = MicState.AnalysingSound;
                    }
                    break;
                case MicState.AnalysingSound:
                    AddSamplesToRecordingBuffer();
                    if (RecordingBuffer.EnoughtSamplesForSilenceDetection)
                    {
                        if (RecordingBuffer.ShouldStartRecording())
                        { // start recording and keep already recorded samples
                            MicrophoneState = MicState.Recording;
                            RecordingBuffer.StartConverting();
                        }
                        else
                        { // clear buffer and keep analysing
                            RecordingBuffer.CalculateSilenceBarrier();
                        }
                    }
                    break;
                case MicState.Recording:
                    int addedSamples = AddSamplesToRecordingBuffer();
                    if (addedSamples < 0)
                        break;
                    if (addedSamples == 0 || RecordingBuffer.ShouldStopRecording())
                    { // samples array is full, stop recording
                        MicrophoneState = MicState.Processing;
                    }
                    break;
                case MicState.Processing:
                    RecordingBuffer.Notify();
                    if (!RecordingBuffer.Running)
                        MicrophoneState = MicState.Processed;
                    break;
                case MicState.Idle:
                    break;
                case MicState.Processed:
                    microphoneState = MicState.Idle;
                    break;
                default:
                    throw new InvalidProgramException("Unhandled microphone state.");
            }
	    }

	    private int AddSamplesToRecordingBuffer()
	    {
	        try
	        {
#if UNITY_EDITOR
	            if (Prerecorded)
	            {
	                return -1;
	            }
#endif
	            if (RecordingStartTime + TalkBackSettings.MicrophoneRecordingStartDelay > Time.time)
	            {
	                return -1;
	            }

	            // read audio from mic
	            float[] data;
	            int numOfSamples = 0;
#if UNITY_ANDROID && !(UNITY_EDITOR || NATIVE_SIM)
                data = JinkeGroup.Util.AndroidPluginManager.Instance.CallAnActivityRefGetArray<float[]>("getAudioManager", "getData");
                if (data != null)
                    numOfSamples = data.Length;
#else
	            int writeHeadPosition = Microphone.GetPosition(TalkBackSettings.MicrophoneDeviceName);
	            numOfSamples = (RecordedClip.samples + writeHeadPosition - ReadHeadPosition) % RecordedClip.samples;
	            if (numOfSamples == 0)
	                return -1;

	            // if the clip sample number changes (
	            if (DataBuffer.Length < numOfSamples)
	            {
	                DataBuffer = new float[numOfSamples * 2];
	            }
	            data = DataBuffer;
	            RecordedClip.GetData(data, ReadHeadPosition);
	            ReadHeadPosition = (ReadHeadPosition + numOfSamples) % RecordedClip.samples;
#endif

	            // return if no data read
	            if (data == null || data.Length == 0)
	            {
	                return -1;
	            }

	            // copy data to samples buffer
	            return RecordingBuffer.AddSamples(data, numOfSamples);
	        }
	        finally
	        {
	            RecordingBuffer.Notify();
	        }
        }

	    private void OnDestroy()
	    {
	        ReleaseMicrophone();
#if UNITY_ANDROID && !UNITY_EDITOR
	        //Destroyed in native
#else
            Microphone.End(TalkBackSettings.MicrophoneDeviceName);
#endif
        }

	    private void OnApplicationPause(bool paused)
	    {
	        if (paused)
	        {
	            Stop();
	        }
	        else
	        {
	            Reset();
	        }
	    }

    }
}
