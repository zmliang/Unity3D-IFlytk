using System;
using JinkeGroup.Threading;
using System.Text;
using UnityEngine;

namespace JinkeGroup.Util
{
    public static class Logger
    {
        public enum LogLevel
        {
            VERBOSE,    //任何信息都输出
            DEBUG,      //只输出调试信息
            INFO,       //提示性的信息
            WARN,       //警告信息
            ERROR       //错误信息
        }

        private static int MainThreadID = -1;

        public static int MaxUnityLogLength = 1024 * 5;
        public static LogLevel Level
        {
            get;
            set;
        }

        public static Func<string> CustomLogDataCallback;
        private static readonly char[] StackTraceSplitChars = new char[] { '\n' };
        private static bool IsCustomStackTrace = false;

        public static bool VerboseEnabled
        {
            get
            {
                return LogLevel.VERBOSE >= Level;
            }
        }

        public static void SetMainThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(BuildConfig.IsDevel){
                Application.stackTraceLogType = StackTraceLogType.None;
                IsCustomStackTrace = true;
            }
#endif
            MainThreadID = JKThread.CurrentThreadID;
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message)
        {
            Log(LogLevel.VERBOSE,null,message,null,null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(string message, params object[] args)
        {
            Log(LogLevel.VERBOSE,null,message,null,args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message)
        {
            Log(LogLevel.VERBOSE, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Verbose(Exception e, string message, params object[] args)
        {
            Log(LogLevel.VERBOSE, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message)
        {
            Log(LogLevel.VERBOSE, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, string message, params object[] args)
        {
            Log(LogLevel.VERBOSE, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message)
        {
            Log(LogLevel.VERBOSE, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void VerboseT(string tag, Exception e, string message, params object[] args)
        {
            Log(LogLevel.VERBOSE, tag, message, e, args);
        }

        //######
        // DEBUG

        public static bool DebugEnabled
        {
            get
            {
                return LogLevel.DEBUG >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message)
        {
            Log(LogLevel.DEBUG, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(string message, params object[] args)
        {
            Log(LogLevel.DEBUG, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message)
        {
            Log(LogLevel.DEBUG, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Debug(Exception e, string message, params object[] args)
        {
            Log(LogLevel.DEBUG, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message)
        {
            Log(LogLevel.DEBUG, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, string message, params object[] args)
        {
            Log(LogLevel.DEBUG, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message)
        {
            Log(LogLevel.DEBUG, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void DebugT(string tag, Exception e, string message, params object[] args)
        {
            Log(LogLevel.DEBUG, tag, message, e, args);
        }

        //######
        // INFO

        public static bool InfoEnabled
        {
            get
            {
                return LogLevel.INFO >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message)
        {
            Log(LogLevel.INFO, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(string message, params object[] args)
        {
            Log(LogLevel.INFO, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message)
        {
            Log(LogLevel.INFO, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Info(Exception e, string message, params object[] args)
        {
            Log(LogLevel.INFO, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message)
        {
            Log(LogLevel.INFO, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, string message, params object[] args)
        {
            Log(LogLevel.INFO, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message)
        {
            Log(LogLevel.INFO, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void InfoT(string tag, Exception e, string message, params object[] args)
        {
            Log(LogLevel.INFO, tag, message, e, args);
        }

        //########
        // WARNING

        public static bool WarnEnabled
        {
            get
            {
                return LogLevel.WARN >= Level;
            }
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message)
        {
            Log(LogLevel.WARN, null, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(string message, params object[] args)
        {
            Log(LogLevel.WARN, null, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message)
        {
            Log(LogLevel.WARN, null, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void Warn(Exception e, string message, params object[] args)
        {
            Log(LogLevel.WARN, null, message, e, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message)
        {
            Log(LogLevel.WARN, tag, message, null, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, string message, params object[] args)
        {
            Log(LogLevel.WARN, tag, message, null, args);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message)
        {
            Log(LogLevel.WARN, tag, message, e, null);
        }

#if STRIP_LOGS
        [ConditionalAttribute("FALSE")]
#endif
        public static void WarnT(string tag, Exception e, string message, params object[] args)
        {
            Log(LogLevel.WARN, tag, message, e, args);
        }

        //######
        // ERROR

        public static bool ErrorEnabled
        {
            get
            {
                return LogLevel.ERROR >= Level;
            }
        }

        public static void Error(string message)
        {
            Log(LogLevel.ERROR, null, message, null, null);
        }

        public static void Error(string message, params object[] args)
        {
            Log(LogLevel.ERROR, null, message, null, args);
        }

        public static void Error(Exception e, string message)
        {
            Log(LogLevel.ERROR, null, message, e, null);
        }

        public static void Error(Exception e, string message, params object[] args)
        {
            Log(LogLevel.ERROR, null, message, e, args);
        }

        public static void ErrorT(string tag, string message)
        {
            Log(LogLevel.ERROR, tag, message, null, null);
        }

        public static void ErrorT(string tag, string message, params object[] args)
        {
            Log(LogLevel.ERROR, tag, message, null, args);
        }

        public static void ErrorT(string tag, Exception e, string message)
        {
            Log(LogLevel.ERROR, tag, message, e, null);
        }

        public static void ErrorT(string tag, Exception e, string message, params object[] args)
        {
            Log(LogLevel.ERROR, tag, message, e, args);
        }

        private static void Log(LogLevel level, string tag, string format, Exception e, params object[] args)
        {
            if (level < Level)
                return;
            StringBuilder sb = new StringBuilder(512);
#if !UNITY_EDITOR
            sb.AppendFormat("{0,8}",level);
#endif
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            sb.Append(now);
            sb.Append(" (t");
            sb.Append(JKThread.CurrentThreadID);
            if(JKThread.CurrentThreadID == MainThreadID)
            {
                sb.Append(" f");
                sb.Append(Time.frameCount);
            }
            if (CustomLogDataCallback != null)
            {
                string customData = CustomLogDataCallback();
                if (!string.IsNullOrEmpty(customData))
                {
                    sb.Append(" ");
                    sb.Append(customData);
                }
            }

            sb.Append(") -- ");
            if (tag != null)
            {
                sb.Append("#");
                sb.Append(tag);
                sb.Append(" -- ");
            }
            string msg = format;
            if (args != null)
                msg = string.Format(format,args);

            sb.Append(msg);
            if (e != null)
            {
                sb.Append(" <");
                sb.Append(e.ToString());
                sb.Append(">");
            }

            if (IsCustomStackTrace)
            {
                string stackTrace = StackTraceUtility.ExtractStackTrace();
                stackTrace = stackTrace.Split(StackTraceSplitChars,3)[2];
                sb.AppendLine();
                sb.Append(stackTrace);
            }

            string log = sb.ToString();

            FileLogger.Log(log,level == LogLevel.ERROR);

            if (log.Length > MaxUnityLogLength)
            {
                log = log.Substring(0,MaxUnityLogLength);
            }

            switch (level)
            {
                case LogLevel.VERBOSE:
                case LogLevel.DEBUG:
                case LogLevel.INFO:
                    UnityEngine.Debug.Log(log);
                    break;
                case LogLevel.WARN:
                    UnityEngine.Debug.LogWarning(log);
                    break;
                case LogLevel.ERROR:
                    UnityEngine.Debug.LogError(log);
                    break;
            }

        }

    }
}
