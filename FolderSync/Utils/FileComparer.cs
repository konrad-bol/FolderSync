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
        public static List<(bool, string, int)> GetDifferents(string sourceFilePath, string replicaFilePath)
        {
            var Differents = new List<(bool, string, int)>();
            var sourceLines = File.ReadAllLines(sourceFilePath).ToList();
            var replicaLines = File.ReadAllLines(replicaFilePath).ToList();

            int i = 0, j = 0;


            while (i < sourceLines.Count && j < replicaLines.Count)
            {
                if (sourceLines[i] == replicaLines[j])
                {
                    i++;
                    j++;
                }
                else
                {
                    Differents.Add((true, sourceLines[i], i));
                    Differents.Add((false, replicaLines[j], j));
                    i++;
                    j++;

                }

            }
            while (i < sourceLines.Count)
            {
                Differents.Add((true, sourceLines[i], i));
                i++;
            }
            while (j < replicaLines.Count)
            {
                Differents.Add((false, replicaLines[j], j));
                j++;
            }
            int k = 0;
            while (k < Differents.Count)
            {
                var (flag, line, index) = Differents[k];
                int oldIndex = index;
                for (int l = k + 1; l < Differents.Count; l++)
                {
                    var (nextFlag, nextLine, nextIndex) = Differents[l];
                    if (nextIndex != oldIndex + 1 && nextIndex != oldIndex) break;
                    if (flag != nextFlag && line == nextLine)
                    {
                        Differents.RemoveAt(l);
                        Differents.RemoveAt(k);
                        k--;
                        break;
                    }
                    oldIndex = nextIndex;
                }

                k++;
            }
            return Differents;
        }
        public static List<string> GetMessages(string sourceFilePath, string replicaFilePath)
        {
            var messages = new List<string>();
            var diffs = GetDifferents(sourceFilePath, replicaFilePath);

            int i = 0;
            while (i < diffs.Count)
            {
                var (isSource, line, index) = diffs[i];

                var removed = new List<string>();
                var added = new List<string>();
                int startIndex = index;

                while (i < diffs.Count)
                {
                    var (currFlag, currLine, currIndex) = diffs[i];

                    if (currFlag == true)
                        added.Add(currLine);
                    else
                        removed.Add(currLine);

                    if (i + 1 < diffs.Count)
                    {
                        var (nextFlag, _, nextIndex) = diffs[i + 1];
                        if (Math.Abs(nextIndex - currIndex) > 1)
                        {
                            i++;
                            break;
                        }
                    }
                    i++;
                }

                if (removed.Count > 0 && added.Count > 0)
                {
                    if (removed.Count == 1 || added.Count == 1)
                    {
                        messages.Add($"Changed line at {startIndex + 1}:\n" +
                        $"  - {string.Join("\n  - ", removed)}\n" +
                        $"  + {string.Join("\n  + ", added)}");
                    }
                    else
                        messages.Add(
                        $"Changed lines {startIndex + 1}-{startIndex + removed.Count}:\n" +
                        $"  - {string.Join("\n  - ", removed)}\n" +
                        $"  + {string.Join("\n  + ", added)}"
                    );
                }
                else if (removed.Count > 0)
                {
                    if (removed.Count == 1)
                    {
                        messages.Add($"Removed line at {startIndex + 1}:\n" +
                        $"  - {string.Join("\n  - ", removed)}");

                    }
                    else
                    {
                        messages.Add(
                        $"Removed lines {startIndex + 1}-{startIndex + removed.Count}:\n" +
                        $"  - {string.Join("\n  - ", removed)}"
                    );
                    }
                }
                else if (added.Count > 0)
                {
                    if (added.Count == 1)
                    {
                        messages.Add($"Added line at {startIndex + 1}:\n" +
                        $"  + {string.Join("\n  + ", added)}");
                    }
                    else
                    {
                        messages.Add(
                        $"Added lines {startIndex + 1}-{startIndex + added.Count}:\n" +
                        $"  + {string.Join("\n  + ", added)}"
                    );
                    }

                }
            }

            return messages;
        }
    }
}
