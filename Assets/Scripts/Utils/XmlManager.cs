using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class XmlManager
{

    ////xml数据存储和读取  
    //public void XmlLocalStorage()
    //{
    //    string _fileName = Application.persistentDataPath + "/UnityUserData";

    //    UserData user = new UserData();
    //    user.userName = "乐逍遥";
    //    user.onlyId = 1;
    //    //存储数据  
    //    string s = SerializeObject(user, typeof(UserData));
    //    //创建XML文件且写入数据  
    //    CreateTextFile(_fileName, s, false);

    //    //读取数据  
    //    try
    //    {
    //        string strTemp = LoadTextFile(_fileName, false);
    //        //反序列化对象  
    //        UserData userD = DeserializeObject(strTemp, typeof(UserData)) as UserData;

    //    }
    //    catch
    //    {
    //        Debug.Log("系统读取XML出现错误，请检查");
    //    }
    //}

    ////xml数据存储和读取  
    //public void XmlLocalStorage()
    //{
    //    string _fileName = Application.persistentDataPath + "/UnityUserData";
    //    //存储数据  
    //    // string s = SerializeObject(user, typeof(UserData));
    //    string s = "";
    //    //创建XML文件且写入数据  
    //    CreateTextFile(_fileName, s, false);
    //}
    //public void XmlLocalRead()
    //{
    //    string _fileName = Application.persistentDataPath + "/UnityUserData";
    //    //读取数据  
    //    try
    //    {
    //        string strTemp = LoadTextFile(_fileName, false);
    //        //反序列化对象  
    //        UserData userD = DeserializeObject(strTemp, typeof(UserData)) as UserData;

    //    }
    //    catch
    //    {
    //        Debug.Log("系统读取XML出现错误，请检查");
    //    }
    //}


    /// 创建文本文件  
    public void CreateTextFile(string fileName, string strFileData, bool isEncryption)
    {
        StreamWriter writer;                               //写文件流  
        string strWriteFileData;
        strWriteFileData = strFileData;             //写入的文件数据  

        writer = File.CreateText(fileName);
        writer.Write(strWriteFileData);
        writer.Close();                                    //关闭文件流  
    }


    /// 读取文本文件  
    public string LoadTextFile(string fileName, bool isEncryption)
    {
        StreamReader sReader;                              //读文件流  
        string dataString;                                 //读出的数据字符串  

        sReader = File.OpenText(fileName);
        dataString = sReader.ReadToEnd();
        sReader.Close();                                   //关闭读文件流  
        return dataString;


    }
    ///// 加密方法  
    ///// 描述： 加密和解密采用相同的key,具体值自己填，但是必须为32位  
    //public string Encrypt(string toE)
    //{
    //    byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
    //    RijndaelManaged rDel = new RijndaelManaged();
    //    rDel.Key = keyArray;
    //    rDel.Mode = CipherMode.ECB;
    //    rDel.Padding = PaddingMode.PKCS7;
    //    ICryptoTransform cTransform = rDel.CreateEncryptor();
    //    byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toE);
    //    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

    //    return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    //}

    ///// 解密方法  
    ///// 描述： 加密和解密采用相同的key,具体值自己填，但是必须为32位  
    //public string Decrypt(string toD)
    //{
    //    byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
    //    RijndaelManaged rDel = new RijndaelManaged();
    //    rDel.Key = keyArray;
    //    rDel.Mode = CipherMode.ECB;
    //    rDel.Padding = PaddingMode.PKCS7;
    //    ICryptoTransform cTransform = rDel.CreateDecryptor();
    //    byte[] toEncryptArray = Convert.FromBase64String(toD);
    //    byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

    //    return UTF8Encoding.UTF8.GetString(resultArray);
    //}


    // 数据对象转换xml字符串  
    public string SerializeObject(object pObject, System.Type ty)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(ty);
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    /// xml字符串转换数据对象  
    public object DeserializeObject(string pXmlizedString, System.Type ty)
    {
        XmlSerializer xs = new XmlSerializer(ty);
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xs.Deserialize(memoryStream);
    }
    //UTF8字节数组转字符串  
    public string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    //字符串转UTF8字节数组  
    public byte[] StringToUTF8ByteArray(string pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }
}
