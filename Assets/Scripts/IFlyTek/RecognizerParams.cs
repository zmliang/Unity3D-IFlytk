using System;

namespace JinkeGroup.IFlyTek
{
    public class RecognizerParams
    {
        private string resultType = "json";
        private string language = "zh_cn";
        private string accent = "mandarin";
        private string vadBos = "4000";
        private string vadEos = "1000";
        private string asrPtt = "1";
        private string audioFormat = "wav";
        private string asrAudioPath = null;
        private string engineType = SpeechContant.TYPE_CLOUD;

        public string EngineType
        {
            get { return engineType; }
            set { engineType = value; }
        }

        public string ResultType
        {
            get { return resultType; }
            set { resultType = value; }
        }

        public string Language
        {
            get { return language; }
            set { language = value; }
        }

        public string Accent
        {
            get { return accent; }
            set { accent = value; }
        }

        public string VadBos
        {
            get { return vadBos; }
            set { vadBos = value; }
        }

        public string VadEos
        {
            get { return vadEos; }
            set { vadEos = value; }
        }

        public string AsrPtt
        {
            get { return asrPtt; }
            set { asrPtt = value; }
        }

        public string AudioFormat
        {
            get { return audioFormat; }
            set { audioFormat = value; }
        }

        public string AsrAudioPath
        {
            get { return asrAudioPath; }
            set { asrAudioPath = value; }
        }


    }
}
