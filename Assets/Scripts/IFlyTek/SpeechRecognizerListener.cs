using UnityEngine;
using JinkeGroup.Util;

namespace JinkeGroup.IFlyTek
{
    public class SpeechRecognizerListener : AndroidJavaProxy
    {
        public SpeechRecognizerListener() : base("com.iflytek.cloud.RecognizerListener")
        {
        }

        void onVolumeChanged(int volume, byte[] data)
        {
            AndroidPluginManager.Instance.showTip("音量:"+volume);
        }

        void onBeginOfSpeech()
        {
            AndroidPluginManager.Instance.showTip("onBeginOfSpeech");
        }
        void onEndOfSpeech()
        {

        }
        void onResult(AndroidJavaObject results, bool isLast)
        {
            string jsonText = results.Call<string>("getResultString");
            AndroidPluginManager.Instance.showTip(jsonText);
        }

        void onError(AndroidJavaObject error)
        {
            AndroidPluginManager.Instance.showTip("onError");
        }

        void onEvent(int evenType, int arg1, int arg2, AndroidJavaObject obj)
        {
            // 以下代码用于获取与云端的会话id，当业务出错时将会话id提供给技术支持人员，可用于查询会话日志，定位出错原因
            // 若使用本地能力，会话id为null
            //	if (SpeechEvent.EVENT_SESSION_ID == eventType) {
            //		String sid = obj.getString(SpeechEvent.KEY_EVENT_SESSION_ID);
            //		Log.d(TAG, "session id =" + sid);
            //	}

        }

    }
}

