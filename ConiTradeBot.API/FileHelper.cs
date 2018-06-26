using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace ConiTradeBot.API
{
    /// <summary>
    /// This class is thread-safe for avoiding exceptions such as that file is used and that file access is denied and so on.
    /// </summary>
    public class FileHelper
    {
        static FileHelper()
        {
        }

        public static void SaveStrToFile(string str, string filePath, bool throwException = true, bool isSetToEveryoneFullControlPermission = true)
        {
            try
            {
                //Console.WriteLine("Write file:" + filePath);
                ReadyForSaveNewFile(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(str);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                }
                if (isSetToEveryoneFullControlPermission)
                    GrantEveryoneFullControlAccess(filePath);

            }
            catch (Exception e)
            {
                if (throwException) throw e;
            }
        }


        public static string Read(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string s = sr.ReadToEnd();
                return s;
            }

        }

        public static void SaveObjectToFile(byte[] bytes, string filePath, bool isSetToEveryoneFullControlPermission = true)
        {

            ReadyForSaveNewFile(filePath);
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                fs.Write(bytes, 0, bytes.Length);
                fs.Close();
            }
            if (isSetToEveryoneFullControlPermission)
                GrantEveryoneFullControlAccess(filePath);

        }

        public static void AppendStrToFile(string str, string filePath, bool throwException = true, bool isSetToEveryoneFullControlPermission = true)
        {
            try
            {

                if (!File.Exists(filePath))
                {
                    SaveStrToFile(str, filePath, throwException, isSetToEveryoneFullControlPermission);
                    return;
                }

                using (FileStream fs = new FileStream(filePath, FileMode.Append))
                {
                    byte[] bytes = Encoding.Default.GetBytes(str);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                }
                if (isSetToEveryoneFullControlPermission)
                    GrantEveryoneFullControlAccess(filePath);

            }
            catch (Exception e)
            {
                if (throwException) throw e;
            }
        }

        public static void CreateDirectoryForFile(string filePath, bool isSetToEveryoneFullControlPermission = true)
        {
            var directoryPath = filePath.Remove(filePath.LastIndexOf("\\"));
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public static void Delete(string filePath)
        {

            if (File.Exists(filePath))
                File.Delete(filePath);

        }

        public static void GrantEveryoneFullControlAccess(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            FileSecurity fSecurity = fileInfo.GetAccessControl();
            fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            fileInfo.SetAccessControl(fSecurity);
        }

        private static void ReadyForSaveNewFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                GrantEveryoneFullControlAccess(filePath);
                File.Delete(filePath);
            }
            var dic = new FileInfo(filePath).Directory.FullName;
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
        }

        public enum FileExtension
        {
            CSV
        }

    }
}
