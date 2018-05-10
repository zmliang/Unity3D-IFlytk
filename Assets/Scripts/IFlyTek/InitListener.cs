using UnityEngine;
using JinkeGroup.Util;

namespace JinkeGroup.IFlyTek
{
    //语音识别初始化监听回调
    public class InitListener: AndroidJavaProxy
    {
        public InitListener() : base("com.iflytek.cloud.InitListener"){}

        public void onInit(int code)
        {
            if (code != ErrorCode.SUCCESS)
                AndroidPluginManager.Instance.showTip("初始化失败");
            else
                AndroidPluginManager.Instance.showTip("初始化成功");
        }
    }
    
}