using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Utils
{
    public static class FileComparer
    {
        public static string GetMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        public static bool AreFilesEqual(string filePath1, string filePath2)
        {
            if (!File.Exists(filePath1) || !File.Exists(filePath2)) return false;
            return GetMD5(filePath1) == GetMD5(filePath2);

        }
    }
}
