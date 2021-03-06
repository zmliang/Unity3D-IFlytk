﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using JinkeGroup.Util.Io;

namespace JinkeGroup.Util
{
    public static class FileLogger
    {
        private const string LogFilePathsPrefKey = "FileLogger.LogFilePaths";
        private static readonly object Lock = new object();
        private static StreamWriter File;

        public static string LogPath { get; private set; }

        public static List<string> LogPaths { get; private set; }
        static FileLogger()
        {
            CreateNewLogPath();
            if (BuildConfig.IsProdOrDevel)
            {
                Debug.Log("### Log path: "+LogPath);
            }
        }

        private static void CreateNewLogPath()
        {
            LogPath = Application.persistentDataPath + "/JinkeAppLog" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")+".log";
            LogPaths = UserPrefs.GetCollectionAsList(LogFilePathsPrefKey,null);
            if (LogPaths == null)
                LogPaths = new List<string>();

            if(LogPaths.Count == 0 || LogPaths[LogPaths.Count - 1] != LogPath)
            {
                LogPaths.Add(LogPath);
            }
            UserPrefs.SetCollection(LogFilePathsPrefKey,LogPaths);
            UserPrefs.Save();
        }

        public static void Start()
        {
            lock (Lock)
            {
                if (File != null)
                    return;
                File = new StreamWriter(LogPath,true);
                File.AutoFlush = true;
                File.WriteLine("===================== STARTED NEW LOG SESSION ======================");
            }
        }

        public static void Stop()
        {
            lock (Lock)
            {
                if (File == null)
                    return;
                File.Flush();
                File.Dispose();
                File = null;
            }
        }

        public static void Clear()
        {
            lock (Lock)
            {
                Stop();
                for(int i = 0; i < LogPaths.Count; i++)
                {
                    JKFile.Delete(LogPaths[i]);
                }
                LogPaths.Clear();
                UserPrefs.Remove(LogFilePathsPrefKey);
                UserPrefs.Save();
                Start();
            }
        }

        public static void CreateNewLogFile()
        {
            lock (Lock)
            {
                Stop();
                CreateNewLogPath();
            }
        }

        public static void Log(string msg)
        {
            Log(msg,false);
        }

        public static void Log(string msg,bool flushImmediately)
        {
            lock (Lock)
            {
                if (File == null)
                    Start();
                File.WriteLine(msg);
                if (flushImmediately)
                    File.Flush();
            }
        }

    }
}
