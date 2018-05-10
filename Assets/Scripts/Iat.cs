using UnityEngine;
using UnityEngine.UI;
using JinkeGroup.IFlyTek;
using JinkeGroup.Util;
using System.Threading;
using System.Collections.Generic;
using System;

namespace JinkeGroup.Test
{
    public class Iat : MonoBehaviour
    {
        public GameObject cube;
        public Text text;
        object lockd = new object();
        private float speed = 1f;


        //============================
        public AudioSource audio;
        public bool audioclipWorking = false;
        public string audioIsPlaying;
        public bool microphoneWorking = false;
        private List<float> samplesList;

        public float samplesMaxValue;
        public int ClipLength;

        void goThread()
        {
            int index = 0;
            while (true)
            {
                lock (lockd)
                {
                    Debug.Log("In thread " + index);
                    index++;
                    Thread.Sleep(500);
                    float vol = Volume();
                    AndroidPluginManager.Instance.showTip("Volume:" + vol);
                }
            }
        }

        private void Awake()
        {

        }

        void startThread()
        {
            Thread thread = new Thread(new ThreadStart(goThread));
            thread.IsBackground = true;
            thread.Start();
        }

        // Use this for initialization
        void Start()
        {
            samplesList = new List<float>();
            startRecord();
        }

        // Update is called once per frame
        void Update()
        {
            audioIsPlaying = audio.isPlaying.ToString();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (microphoneWorking)
            {
                if (!Microphone.IsRecording(null))
                {
                    microphoneWorking = false;
                    JudgeRecord();
                }
            }
            if (audioclipWorking)
            {
                if (audio.isPlaying == false)
                {
                    samplesList.Clear();
                    startRecord();
                    audioclipWorking = false;
                }
            }
        }

        private void startRecord()
        {
            audio.clip = Microphone.Start(null, false, 1, 44100);
            while (Microphone.GetPosition(null) <= 0) ;
            microphoneWorking = true;
        }

        public void JudgeRecord()
        {
            int sampleSize = 128;
            float max = 0;
            float[] tempSamples; //临时数据存储
            tempSamples = new float[audio.clip.samples * audio.clip.channels];
            audio.clip.GetData(tempSamples, 0);

            foreach (float s in tempSamples)
            {
                float m = Mathf.Abs(s);
                if (max < m)
                {
                    max = m; //刚刚录制音频数据的取最大值,这里有问题，应该取平均值更好。
                }
            }

            samplesMaxValue = max;
            Debug.Log("samplesMaxValue:" + samplesMaxValue);
            if (max > 0.6f)
            {
                Debug.Log("有人说话:" + max);
                //判断有人说话
#if UNITY_ANDROID
                AndroidPluginManager.Instance.showTip("检测到有人说话,音量大小为：" + max);
#endif
                startRecord();
                foreach (float e in tempSamples)
                {
                    samplesList.Add(e);//保存数据
                }

            }
            else
            {

                //无人说话
                if (audio.clip != null && samplesList != null)
                {
                    if (samplesList.Count > 1000)
                    {
                        AudioClip myClip = AudioClip.Create("tom", samplesList.Count, 1, 44100, false, false);
                        myClip.SetData(samplesList.ToArray(), 0);
                        audio.clip = myClip;
                        audio.loop = false;
                        audio.pitch = 1.2f;
                        audio.Play();
                        audioclipWorking = true;
                    }
                    else
                    {
                        startRecord();
                    }
                }
                else
                {
                    startRecord();
                }
            }
        }



        private void FixedUpdate()
        {
            Vector3 point = Camera.main.WorldToScreenPoint(cube.transform.position);
            if (point.x > 1080)
                right = false;
            if (point.x < 0)
                left = false;
            if (point.y > 1920)
                up = false;
            if (point.y < 0)
                down = false;
            if (up)
            {
                text.text = Camera.main.WorldToScreenPoint(cube.transform.position).ToString();
                cube.gameObject.transform.Translate(Vector3.up * speed * Time.deltaTime);
            }
            else if (down)
            {
                text.text = Camera.main.WorldToScreenPoint(cube.transform.position).ToString();
                cube.gameObject.transform.Translate(Vector3.down * speed * Time.deltaTime);
            }
            else if (right)
            {
                text.text = Camera.main.WorldToScreenPoint(cube.transform.position).ToString();
                cube.gameObject.transform.Translate(Vector3.right * speed * Time.deltaTime);
            }
            else if (left)
            {
                text.text = Camera.main.WorldToScreenPoint(cube.transform.position).ToString();
                cube.gameObject.transform.Translate(Vector3.left * speed * Time.deltaTime);
            }
        }

        private void OnGUI()
        {
            Debug.Log("OnGUI..");
            callAndroidMethod();
        }

        void callAndroidMethod()
        {
            if (GUI.Button(new Rect(200, 200, 200, 80), "点击开始说话"))
            {
                SpeechRecognizer.Instance.setParams(new RecognizerParams());
                SpeechRecognizer.Instance.startListening(new SpeechRecognizerListener());
            }
        }

        bool up = false;
        bool down = false;
        bool right = false;
        bool left = false;


        //moveUp called by android 
        public void moveUp(string t)
        {
            resetDir();
            text.text = t;
            if (t.Contains("上"))
                up = true;
            if (t.Contains("下"))
                down = true;
            if (t.Contains("左"))
                left = true;
            if (t.Contains("右"))
                right = true;
        }

        void resetDir()
        {
            left = false;
            right = false;
            up = false;
            down = false;
        }

        public float Volume()
        {
            if (Microphone.IsRecording(null))
            {
                /*
                int sampleSize = 128;
                float[] samples = new float[sampleSize];
                int startPosition = Microphone.GetPosition(null) - (sampleSize+1);

                this.GetComponent<AudioClip>().GetData(samples,startPosition);
                float levelMax = 0;
                for (int i=0;i<sampleSize;++i)
                {
                    float wavePeak = samples[i];
                    if (levelMax < wavePeak)
                        levelMax = wavePeak;
                }
                return levelMax * 99;
                */

            }
            return 0;
        }

        private void OnDestroy()
        {

        }

    }
}
