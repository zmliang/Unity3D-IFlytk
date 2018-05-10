using UnityEngine;
using JinkeGroup.Util;

namespace JinkeGroup.IFlyTek
{
    public class SpeechRecognizer
    {
        private static  SpeechRecognizer instance = null;
        private static AndroidJavaObject speechRecognizer = null;

        private AndroidJavaObject Recognizer
        {
            get
            {
                if (speechRecognizer == null)
                {
                    AndroidJavaClass cls = new AndroidJavaClass("com.iflytek.cloud.SpeechRecognizer");
                    speechRecognizer = cls.CallStatic<AndroidJavaObject>("createRecognizer",
                        AndroidPluginManager.Instance.CurrentActivity, new InitListener());
                   
                }
                return speechRecognizer;
            }
        }
        static SpeechRecognizer()
        {
            
        }

        private SpeechRecognizer()
        {
           
        }

        public static SpeechRecognizer Instance
        {
            get
            {
                if (instance == null)
                    instance = new SpeechRecognizer();
                return instance;
            }
        }

        public void cancel()
        {
            Recognizer.Call("cancel");
        }

        public void destroy()
        {
            Recognizer.Call("destroy");
        }


        public void startListening(SpeechRecognizerListener listener)
        {
           
            int result = Recognizer.Call<int>("startListening",listener);
            if (result != ErrorCode.SUCCESS)
                AndroidPluginManager.Instance.showTip("听写错误，错误码:" +result);
            else
                AndroidPluginManager.Instance.showTip("请开始说话");
        }

        public void clearParams()
        {
            //清空参数
            Recognizer.Call<bool>("setParameter", SpeechContant.PARAMS, null);
        }

        public void setParams(RecognizerParams param)
        {
            clearParams();
            // 设置听写引擎
            Recognizer.Call<bool>("setParameter", SpeechContant.ENGINE_TYPE, param.EngineType);
            // 设置返回结果格式
            Recognizer.Call<bool>("setParameter", SpeechContant.RESULT_TYPE, param.ResultType);
            // 设置语言
            Recognizer.Call<bool>("setParameter", SpeechContant.LANGUAGE, param.Language);
            //设置区域
            Recognizer.Call<bool>("setParameter", SpeechContant.ACCENT, param.Accent);
            //设置语音前断电：静音超时时间，即用户多长时间不说话当作超时处理
            Recognizer.Call<bool>("setParameter", SpeechContant.VAD_BOS, param.VadBos);
            //设置用户停止说话多长时间内即认为不再输入，停止录音。
            Recognizer.Call<bool>("setParameter", SpeechContant.VAD_EOS, param.VadEos);
            //设置标点符号，0 表示返回结果无标点符号。1 表示有
            Recognizer.Call<bool>("setParameter", SpeechContant.ASR_PTT, param.AsrPtt);
            //音频格式
            Recognizer.Call<bool>("setParameter", SpeechContant.AUDIO_FORMAT, param.AudioFormat);
            //音频保存路径
            Recognizer.Call<bool>("setParameter", SpeechContant.ASR_AUDIO_PATH, param.AsrAudioPath);
        }

    }

}
