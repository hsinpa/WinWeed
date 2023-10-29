using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Hsinpa.Utility
{
    public class IOUtility 
    {
        public static string GetFileText(string textFilePath) {
            if (!System.IO.File.Exists(textFilePath)) return "";

            return System.IO.File.ReadAllText(textFilePath);
        }

        public static void SaveFileText(string filePath, string fullText) {
            System.IO.File.WriteAllText(filePath, fullText);
        }

        public static void DeleteFile(string filePath) {
            System.IO.File.Delete(filePath);
        }

        public static List<T> GetAllJSONFromDirectory<T>(string directoryPath) where T : struct {
            if (!System.IO.Directory.Exists(directoryPath)) return null;

            string[] files = System.IO.Directory.GetFiles(directoryPath);
            List<T> fileContents = new List<T>();

            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string rawContent = GetFileText(files[i]);

                    if (string.IsNullOrEmpty(rawContent))
                        continue;

                    fileContents.Add(JsonUtility.FromJson<T>(rawContent));
                }
            }
            catch {
                Debug.LogWarning("Files in " + directoryPath +" is corrupted");
            }
  
            return fileContents;
        }

        public static string FilterPathStringWithSpace(string old_path, bool withDoubleQuote)
        {
            string espace_space = old_path.Replace("%20", " ");

            if (!withDoubleQuote) return espace_space;

            return $"\"{espace_space}\"";
        }

        public static async Task<bool> CopyAndModifiedFilesAsync(string template_path, string destinate_path, System.Func<string, string> operation) {
            try
            {
                if (File.Exists(template_path) && File.Exists(template_path))
                {
                    string full_config_text = File.ReadAllText(template_path);

                    full_config_text = operation(full_config_text);

                    using (StreamWriter writer = new StreamWriter(destinate_path))
                    {
                        await writer.WriteAsync(full_config_text);
                    }

                    return true;
                }
            }
            catch { 
                Debug.Log("CopyAndModifiedFiles FIle not exist");
            }

            return false;
        }

        public static bool CreateDirectoryIfNotExist(string rootPath, params string[] folderName) {

            if (!System.IO.Directory.Exists(rootPath)) return false;

            int folderLength = folderName.Length;

            string path = rootPath;

            for (int i = 0; i < folderLength; i++) {

                path = System.IO.Path.Combine(path, folderName[i]);

                if (!System.IO.Directory.Exists(path)) {
                    var directoryInfo = System.IO.Directory.CreateDirectory(path);

                    if (!directoryInfo.Exists) return false;
                }
            }

            return true;
        }
    }
}