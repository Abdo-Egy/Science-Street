using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

namespace JohnsProject.CrossPlatformSave
{
    ///
    ///<summary>
    ///The CrossPlatformSaveManager manager class manages the saving and loading
    ///proccess of the CrossPlatformSaveData class. The content of the CrossPlatformSaveData
    ///class is saved in xml file format and encrypted, prohibiting users to edit
    ///the save file. The save file is stored under 'Application.persistentDataPath + "/CrossPlatformSaveData"'
    ///and named 'save.cpsd'. The path as well as the name of the file can be changed
    ///at any time by editing the 'SAVE_PATH' or the 'SAVE_FILE' variables in the 
    ///CrossPlatformSaveManager class. Please note that there is no need call any method to
    ///load the saved data as this will be done automatically.
    ///</summary>
    ///
    public class CrossPlatformSaveManager
    {
        // change this to change the path of the save file
        private static string SAVE_PATH = Application.persistentDataPath + "/CrossPlatformSaveData";
        // change this to change the name of the save file
        private static string SAVE_FILE = SAVE_PATH + "/save.cpsd";

        private static CrossPlatformSaveManager instance;

        public static CrossPlatformSaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CrossPlatformSaveManager();
                }
                return instance;
            }
        }

        private CrossPlatformSaveData saveData;

        public CrossPlatformSaveData SaveData
        {
            get
            {
                return this.saveData;
            }
        }

        private CrossPlatformSaveManager()
        {
            saveData = new CrossPlatformSaveData();
            // if the save file already exits it will automatically be loaded
            if (File.Exists(SAVE_FILE))
            {
                Load();
            }
            // else a new one will be created
            else
            {
                Save();
            }
        }

        ///
        ///<summary>
        ///Saves the actual state of CrossPlatformSaveData into a xml file.
        ///</summary>
        ///
        public void Save()
        {
            if (!Directory.Exists(SAVE_PATH))
            {
                Directory.CreateDirectory(SAVE_PATH);
            }
            var serializer = new XmlSerializer(typeof(CrossPlatformSaveData));
            var stream = new FileStream(SAVE_FILE, FileMode.Create);
            serializer.Serialize(stream, saveData);
            stream.Close();
            EncryptXmlFile();
        }

        private void Load()
        {
            DecryptXmlFile();
            var serializer = new XmlSerializer(typeof(CrossPlatformSaveData));
            var stream = new FileStream(SAVE_FILE, FileMode.Open);
            saveData = serializer.Deserialize(stream) as CrossPlatformSaveData;
            //saveData.copy(data);
            stream.Close();
            EncryptXmlFile();
        }

        private void EncryptXmlFile()
        {
            if (File.Exists(SAVE_FILE))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(SAVE_FILE);
                XmlElement elmRoot = xmlDoc.DocumentElement;
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(elmRoot.InnerXml);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                string data = Convert.ToBase64String(resultArray, 0, resultArray.Length);
                elmRoot.RemoveAll();
                elmRoot.InnerText = data;
                xmlDoc.Save(SAVE_FILE);
            }
        }

        private void DecryptXmlFile()
        {
            if (File.Exists(SAVE_FILE))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(SAVE_FILE);
                XmlElement elmRoot = xmlDoc.DocumentElement;
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12345678901234567890123456789012");
                byte[] toEncryptArray = Convert.FromBase64String(elmRoot.InnerText);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.Mode = CipherMode.ECB;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                string data = UTF8Encoding.UTF8.GetString(resultArray);
                elmRoot.InnerXml = data;
                xmlDoc.Save(SAVE_FILE);
            }
            else
            {
                CrossPlatformSaveManager.Instance.Save();
            }
        }
    }
}
