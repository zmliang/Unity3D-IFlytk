using System;

namespace JinkeGroup.Util.Io
{
    public static class JKFile
    {
        public static bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static bool Delete(string path)
        {
            if (!Exists(path))
                return false;
            System.IO.File.Delete(path);
            return true;
        }
        public static void Copy(string sourceFileName,string destFileName)
        {
            System.IO.File.Copy(sourceFileName,destFileName);
        }

        public static DateTime getLastWriteTime(string path)
        {
            return System.IO.File.GetLastWriteTime(path);
        }

        public static string ReadAllText(string path)
        {
            return System.IO.File.ReadAllText(path);
        }

        public static void WriteAllText(string path,string data)
        {
            System.IO.File.WriteAllText(path,data);
        }

        public static byte[] ReadAllBytes(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }

        public static void WriteAllBytes(string path,byte[] data)
        {
            System.IO.File.WriteAllBytes(path,data);
        }

    } 
}
