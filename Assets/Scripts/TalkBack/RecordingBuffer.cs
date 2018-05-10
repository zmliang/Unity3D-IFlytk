using System;
using System.Threading;
using UnityEngine;
//using JinkeGroup.Audio;

namespace JinkeGroup.TalkBack{
	public class RecordingBuffer
	{
	    public const string TAG = "RecordingBuffer";

	    public ProcessedSound ProcessedSound;
	    private bool FirstRun = true;

	    public volatile bool Running;
	    public volatile bool DoneCollectingData;
	    private volatile int SamplesCaptured;
	    private float AudioBufferAvgSampleValue;

	    private readonly TalkBackSettings TalkBackSettings;

        //frame analyse
	    private int DataAnalyseIndex = 0;
	    private readonly int BatchSize = 0;
	    private float[] Samples;

        //processed data
	    private float AudioBufferMaxSampleValue = 0;

        //silence detection
	    private readonly int MinSilenceSamples;
	    private readonly bool[] SamplesChunkSilence;
	    private int SamplesChunkIndex;
	    private readonly int MaxSamplesToCapture;
	    private readonly int SampleRate;
	    private readonly float[] AudioBufferLastFramesMax;
	    private int AudioBufferSize;

	    private readonly object AddSamplesLock = new object();
        private readonly object SamplesLock = new object();

	    //==   private JKThread SoundConversionThread;//声音转变线程

        public RecordingBuffer(TalkBackSettings talkBackSettings)
	    {
	        TalkBackSettings = talkBackSettings;

	        SampleRate = TalkBackSettings.SampleRate;
	        MaxSamplesToCapture = SampleRate * TalkBackSettings.MaxRecordingTime;

	        AudioBufferLastFramesMax = new float[TalkBackSettings.ListenAverageFramesCount];
	        BatchSize = Mathf.CeilToInt(SampleRate / (float)TalkBackSettings.TalkFramesPerSecond);
	        MinSilenceSamples = Mathf.RoundToInt(talkBackSettings.MicrophoneRecordingStartDelay * SampleRate);


	        Samples = new float[MaxSamplesToCapture]; // create samples buffer according sample rate

            // output can be double the length of input (if pitch is lowered)
	        ProcessedSound = new ProcessedSound(talkBackSettings);

            SamplesChunkSilence = new bool[MaxSamplesToCapture / MinSilenceSamples];
        }

	    public void ClearBuffer()
	    {
	        Array.Clear(Samples,0,Samples.Length);
            Array.Clear(SamplesChunkSilence,0,SamplesChunkSilence.Length);
	        ProcessedSound.ResetAndClear();
        }

        public void SetDoneCollectingData()
	    {
	        DoneCollectingData = true;
	    }

	    public bool EnoughtSamplesForSilenceDetection
	    {
	        get { return SamplesCaptured > MinSilenceSamples; }
	    }

	    public void Abort()
	    {
	        Running = false;
            /*
            if (SoundConversionThread != null)
	        {
	            lock (AddSamplesLock)
	            {
	                Running = false;
                    Monitor.Pulse(AddSamplesLock);
	            }
	            SoundConversionThread.Join();
	        }
	        SoundConversionThread = null;
            */
	    }

	    public void StartConverting()
	    {
            /*
	        if (SoundConversionThread!=null)
	        {
	            lock (AddSamplesLock)
	            {
	                Running = false;
                    Monitor.Pulse(AddSamplesLock);
	            }
	            SoundConversionThread.Join();
	        }
            SoundConversionThread = new JKThread(this.ProcessSamples);
	        SoundConversionThread.Name = "SoundConversionAndAnalysis";
	        Running = true;
	        SoundConversionThread.Start();
            */
        }

	    public void InitSamplesBuffer()
	    {

	        ProcessedSound.ResetAndClear();

            // reset chunk analitics
            SamplesChunkIndex = 0;

	        // reset captured samples buffer & pointers
	        SamplesCaptured = 0;

	        // reset silence barrier
	        AudioBufferAvgSampleValue = 0;

	        DoneCollectingData = false;
	    }

	    private void CalcNewSilenceAvg()
	    {
	        float a = 0;
	        for (int i = 0; i < AudioBufferSize; i++)
	        {
	            a += AudioBufferLastFramesMax[i];
	        }
	        AudioBufferAvgSampleValue = a / AudioBufferSize;
	    }

	    private void AddNewSilenceFrameMax(float value)
	    {

	        AudioBufferSize++;

	        if (AudioBufferSize > TalkBackSettings.ListenAverageFramesCount)
	        {
	            AudioBufferSize = TalkBackSettings.ListenAverageFramesCount;
	        }

	        for (int i = 1; i < TalkBackSettings.ListenAverageFramesCount; i++)
	        {
	            AudioBufferLastFramesMax[i - 1] = AudioBufferLastFramesMax[i];
	        }
	        AudioBufferLastFramesMax[AudioBufferLastFramesMax.Length - 1] = value;
	    }

        public void CalculateSilenceBarrier()
        {
            float max = 0;
            for (int i = 0; i < MinSilenceSamples; i++)
            {

                if (max < Samples[i])
                {
                    max = Mathf.Abs(Samples[i]);
                }
            }

            if (max > 1)
            {
                max = 1;
            }

            AddNewSilenceFrameMax(max);
            CalcNewSilenceAvg();

            // clear current buffer
            SamplesCaptured = 0;
        }

        public bool ShouldStartRecording()
        {
            float maxLimit = AudioBufferAvgSampleValue * TalkBackSettings.ListenSilenceStartFactor;

            if (maxLimit < (AudioBufferAvgSampleValue + TalkBackSettings.ListenSilenceEnviromentAbsDeltaStart))
            {
                maxLimit = AudioBufferAvgSampleValue + TalkBackSettings.ListenSilenceEnviromentAbsDeltaStart;
            }

            float sum = 0;
            int maxVal = 0;
            float t;
            for (int i = 0; i < MinSilenceSamples; i++)
            {
                t = Mathf.Abs(Samples[i]);
                sum += t;

                if (t > maxLimit)
                {
                    maxVal++;
                }
            }
            //Recording starts when there are enough loud samples (More than 20) and  
            float sumLimit = MinSilenceSamples * TalkBackSettings.ListenStartRecordingFactor;
            return ((sum >= sumLimit) && (maxVal > 20));
        }

        public bool ShouldStopRecording()
        {

            int silenceCheckStartPosition = SamplesChunkIndex * MinSilenceSamples;

            if (SamplesCaptured > silenceCheckStartPosition + MinSilenceSamples)
            { // check new chunk for silence

                SamplesChunkSilence[SamplesChunkIndex] = IsSilence(silenceCheckStartPosition);

                SamplesChunkIndex++;
            }

            // check if last N chunk containes silence
            int chunksWithSilence = 0;
            for (int i = SamplesChunkIndex - 1; i > 0; i--)
            {

                if (SamplesChunkSilence[i])
                {
                    chunksWithSilence++;
                }
                else
                {
                    break;
                }
            }

            return chunksWithSilence >= TalkBackSettings.ListenStopMaxSilenceChunks;
        }

        private bool IsSilence(int offset)
        {
            float maxLimit = AudioBufferAvgSampleValue * TalkBackSettings.ListenSilenceEndFactor;

            if (maxLimit < (AudioBufferAvgSampleValue + TalkBackSettings.ListenSilenceEnviromentAbsDeltaEnd))
            {
                maxLimit = AudioBufferAvgSampleValue + TalkBackSettings.ListenSilenceEnviromentAbsDeltaEnd;
            }

            int barrierExeeds = 0;

            for (int i = 0; i < MinSilenceSamples; i++)
            {

                if (Mathf.Abs(Samples[i + offset]) > maxLimit)
                {
                    barrierExeeds++;
                }
            }

            return barrierExeeds < 20;
        }

        public int AddSamples(float[] data, int numOfSamples)
        {


            // get buffer size to copy
            int copyLen = numOfSamples / TalkBackSettings.MicrophoneChannels;
            if (SamplesCaptured + copyLen >= MaxSamplesToCapture)
            {
                copyLen = MaxSamplesToCapture - SamplesCaptured - 1;
            }

            if (copyLen > 0)
            {

                if (TalkBackSettings.MicrophoneChannels == 1)
                {
                    //                    Thread.MemoryBarrier();
                    lock (SamplesLock)
                    {
                        Array.Copy(data, 0, Samples, SamplesCaptured, copyLen);
                    }
                    //                    Thread.MemoryBarrier();
                }
                else
                {
                    //                    Thread.MemoryBarrier();
                    lock (SamplesLock)
                    {
                        for (int i = 0; i < copyLen; i++)
                        { // capture only mono sound, skip other channels
                            Samples[i + SamplesCaptured] = data[i * TalkBackSettings.MicrophoneChannels];//TODO should probably average the samples on more than 1 mic channel
                        }
                    }
                    //                    Thread.MemoryBarrier();
                }

                SamplesCaptured += copyLen;

            }

            //JinkeGroup.Util.Logger.DebugT(Tag, "SamplesCaptured {0} copyLen {1}", SamplesCaptured, copyLen);


            return copyLen;
        }

        public void Notify()
        {
            lock (AddSamplesLock)
                Monitor.Pulse(AddSamplesLock);
        }

        #region sound processing
        private void ProcessSamples()
        {

            //            bool memBarrier = false;

            try
            {
                // initialize soundtouch
                AudioBufferMaxSampleValue = 0;

                // frame analyse
                DataAnalyseIndex = 0;

                //processed buffer
                int processedDataIndex = 0;

                //==    SoundTouchPlugin.InitSoundTouch(TalkBackSettings.SoundTouchTempo, TalkBackSettings.SoundTouchPitch, TalkBackSettings.SoundTouchRate, SampleRate);

                while (Running)
                {
                    int currentSamplesCaptured = SamplesCaptured;

                    if (processedDataIndex < currentSamplesCaptured)
                    { // new data to process

                        int samplesToProcess = currentSamplesCaptured - processedDataIndex;

                        if (!DoneCollectingData && samplesToProcess > TalkBackSettings.SoundTouchMaxSamplesToProcess)
                        {

                            samplesToProcess = TalkBackSettings.SoundTouchMaxSamplesToProcess;
                        }

                        int toProcess = samplesToProcess;
                        int processedSamples = 0;//==ProcessedSound.Data.Length - ProcessedSound.Length;

                        int normStartIndex = processedDataIndex;
/*                        
 *                        memBarrier = true;
                        lock (SamplesLock)
                        {
                            SoundTouchPlugin.ProcessSound(Samples, processedDataIndex, samplesToProcess, ProcessedSound.Data,
                                ProcessedSound.Length, processedSamples, out processedSamples, DoneCollectingData);
                        }
                        processedDataIndex += toProcess; // update src buffer index
                        ProcessedSound.Length += processedSamples;

                        // normalization 1. step - get max processed value
                        for (int i = normStartIndex; i < processedDataIndex; i++)
                        { // check if max sample val exceeded and save it

                            float val = Mathf.Abs(ProcessedSound.Data[i]);
                            if (val > AudioBufferMaxSampleValue)
                            {
                                AudioBufferMaxSampleValue = val;
                            }
                        }
                        */
                    }

                    AnalyseFrames(processedDataIndex < currentSamplesCaptured);

                    /*
                    if (DoneCollectingData && // in state processing
                        currentSamplesCaptured == processedDataIndex && // did process all input data
                        DataAnalyseIndex == ProcessedSound.Length)
                    { // did analyse all processed data

                        if (FirstRun && AudioBufferMaxSampleValue < 0.5)
                        {
                            AudioBufferMaxSampleValue = TalkBackSettings.ListenNormalizationFactor;
                        }
                        FirstRun = false;

                        int maxEndSamplesCutoff = (int)TalkBackSettings.ListenEndCutoff * MinSilenceSamples;
                        ProcessedSound.Length = Math.Max(ProcessedSound.Length - maxEndSamplesCutoff, 1); // MTA-2596 (crash if ProcessedSound.Length <= 0) 
                        int maxEndSamplesFadeout = (int)TalkBackSettings.ListenEndFadeout * MinSilenceSamples;

                        float normFactor = GetNormFactor(AudioBufferMaxSampleValue);
                        for (int i = 0; i < ProcessedSound.Length; i++)
                        {
                            ProcessedSound.Data[i] = GetSampleValueInSoundNormRange(ProcessedSound.Data[i] * normFactor);

                            int endPos = ProcessedSound.Length - maxEndSamplesFadeout;
                            if (i > endPos)
                            { // fadeout
                                float fadeout = 1 - ((i - endPos) / (float)maxEndSamplesFadeout);
                                ProcessedSound.Data[i] *= fadeout;
                            }
                        }

                        Running = false;
                    }
                    */
                    lock (AddSamplesLock)
                    {
                        if (Running)
                        {
                            //JinkeGroup.Util.Logger.DebugT(Tag, "Waiting");
                            Monitor.Wait(AddSamplesLock);
                            //JinkeGroup.Util.Logger.DebugT(Tag, "Resuming");
                        }
                    }
                }
            }
            catch (Exception e)
            {
               // UnityLogHandler.HandleException("Inside Recording Buffer Thread: " + e.Message, e);
            }
            finally
            {
                //                if (memBarrier)
                //                    Thread.MemoryBarrier();
            }
        }

        private float GetSampleValueInSoundNormRange(float val)
        {
            if (val > TalkBackSettings.ListenNormalizationFactor)
                val = TalkBackSettings.ListenNormalizationFactor;
            else if (val < (-TalkBackSettings.ListenNormalizationFactor))
                val = -TalkBackSettings.ListenNormalizationFactor;

            return val;
        }

        private float GetNormFactor(float maxVal)
        {
            if (maxVal != 0)
            {
                return TalkBackSettings.ListenNormalizationFactor / maxVal;
            }
            else
            {
                return TalkBackSettings.ListenNormalizationFactor;
            }
        }

        private void AnalyseFrames(bool doPartial)
        {
            /*
            while (DataAnalyseIndex < ProcessedSound.Length)
            {

                int AUDIO_CHUNK_AMP_BINS = TalkBackSettings.TalkFrameBins;
                int AUDIO_CHUNK_FRAME_SIZE;

                if (DataAnalyseIndex + BatchSize > ProcessedSound.Length)
                {

                    if (!DoneCollectingData)
                    { // wait for more data, stop analysing

                        break;
                    }

                    AUDIO_CHUNK_FRAME_SIZE = ProcessedSound.Length - DataAnalyseIndex;

                    if (AUDIO_CHUNK_FRAME_SIZE < BatchSize && doPartial)
                    {

                        break;
                    }

                }
                else
                {

                    AUDIO_CHUNK_FRAME_SIZE = BatchSize;
                }

                float AUDIO_CHUNK_AMP_BIN_SIZE_FLOAT = (1.0f / AUDIO_CHUNK_AMP_BINS);

                int[] bins = new int[AUDIO_CHUNK_AMP_BINS];
                for (int i = 0; i < AUDIO_CHUNK_AMP_BINS; i++)
                {
                    bins[i] = 0;
                }

                float maxSample = 0;
                for (int i = 0; i < AUDIO_CHUNK_FRAME_SIZE; i++)
                {
                    int tBinNum = Mathf.RoundToInt(Mathf.Abs(ProcessedSound.Data[i + DataAnalyseIndex]) / AUDIO_CHUNK_AMP_BIN_SIZE_FLOAT);
                    if (tBinNum >= AUDIO_CHUNK_AMP_BINS || tBinNum < 0)
                    {
                        tBinNum = AUDIO_CHUNK_AMP_BINS - 1;
                    }

                    bins[tBinNum]++;
                }

                for (int i = 0; i < AUDIO_CHUNK_AMP_BINS; i++)
                {

                    if (bins[i] > (AUDIO_CHUNK_FRAME_SIZE / 50))
                        maxSample = i;
                }

                maxSample = maxSample * AUDIO_CHUNK_AMP_BIN_SIZE_FLOAT;

                float NOISE_LIMIT = TalkBackSettings.ListenNoiseLimit;

                float audioNum = 0;

                // new talk with logaritmic animation
                if (maxSample < NOISE_LIMIT)
                {
                    audioNum = 0;
                }
                else
                {

                    audioNum = (maxSample - NOISE_LIMIT) / (1 - NOISE_LIMIT);

                    const float boost = 0.8f;

                    // boost talk frames
                    if (audioNum > boost)
                    {
                        audioNum = boost;
                    }
                    else
                    {
                        audioNum = audioNum / boost;
                    }
                }

                audioNum = Mathf.Pow(audioNum, 1.5f);

                DataAnalyseIndex += AUDIO_CHUNK_FRAME_SIZE;
              
                if (ProcessedSound.TalkFramesLength < ProcessedSound.TalkFrames.Length)
                {//This happens when you record for more than the meximum allowed time (15s currently)
                    ProcessedSound.TalkFrames[ProcessedSound.TalkFramesLength] = audioNum;
                    ProcessedSound.TalkFramesLength++;
                }
           

            }
            */
        }
    }

    #endregion

}
