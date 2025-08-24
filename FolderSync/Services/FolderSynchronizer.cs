using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using FolderSync.Utils;
using System.Net.NetworkInformation;

namespace FolderSync.Services
{
    public class FolderSynchronizer
    {
        private string sourcePath;
        private string replicaPath;
        private ILogger logger;

        public FolderSynchronizer(string sourcePath, string replicaPath, ILogger logger)
        {
            this.sourcePath = sourcePath;
            this.replicaPath = replicaPath;
            this.logger = logger;
        }
        public async Task start_sync()
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
            while (await timer.WaitForNextTickAsync())
            {
                logger.start_log();
                logger.log($"Synchronization started from {sourcePath} to {replicaPath}");
                Sync();
                logger.log($"Synchronization completed from {sourcePath} to {replicaPath}");
                logger.end_log();
            }
        }
        public void Sync()
        {
            Dictionary<string, List<string>> sourceFilesDir = new();
            Dictionary<string, List<string>> replicaFilesDir = new();

            var sourceFiles = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories);
            var replicaFiles = Directory.EnumerateFiles(replicaPath, "*", SearchOption.AllDirectories);

            // 1. update files in replica that exist in source but have different content
            foreach (var file in sourceFiles)
            {
                string replicafilePath = file.Replace(sourcePath, replicaPath);
                if (File.Exists(replicafilePath) && !FileComparer.AreFilesEqual(file, replicafilePath))
                {
                    logger.log($"File {file} exists in replica but content is different. Updating...");
                    logger.log_change(FileComparer.GetMessages(file, replicafilePath));
                    File.Copy(file, replicafilePath, true);
                }
                string hash = FileComparer.GetMD5(file);
                if (!sourceFilesDir.ContainsKey(hash))
                    sourceFilesDir[hash] = new List<string>();
                sourceFilesDir[hash].Add(file);

            }



            // 3. compare file with the same content in source and replica
            foreach (var file in replicaFiles)
            {
                string hash = FileComparer.GetMD5(file);
                if (!replicaFilesDir.ContainsKey(hash))
                    replicaFilesDir[hash] = new List<string>();
                replicaFilesDir[hash].Add(file);
            }


            foreach (var kvp in sourceFilesDir)
            {
                string hash = kvp.Key;
                var sourceFilesWithHash = kvp.Value;
                if (!replicaFilesDir.TryGetValue(hash, out var replicaFilesWithHash))
                    continue;

                var sourceRemaining = new List<string>(sourceFilesWithHash);
                var replicaRemaining = new List<string>(replicaFilesWithHash);

                foreach (var sourceFile in sourceFilesWithHash.ToList())
                {
                    string expectedReplicaPath = sourceFile.Replace(sourcePath, replicaPath);
                    var replicaMatch = replicaRemaining.FirstOrDefault(r => Path.GetFileName(r) == Path.GetFileName(sourceFile));
                    if (replicaMatch != null && replicaMatch != expectedReplicaPath)
                    {
                        logger.log($"Moving {replicaMatch} → {expectedReplicaPath}");
                        Directory.CreateDirectory(Path.GetDirectoryName(expectedReplicaPath)!);
                        File.Move(replicaMatch, expectedReplicaPath, true);

                        sourceRemaining.Remove(sourceFile);
                        replicaRemaining.Remove(replicaMatch);
                    }
                }

                while (sourceRemaining.Count > 0 && replicaRemaining.Count > 0)
                {
                    string sourceFile = sourceRemaining[0];
                    string replicaFile = replicaRemaining[0];
                    string expectedReplicaPath = sourceFile.Replace(sourcePath, replicaPath);
                    if (replicaFile != expectedReplicaPath)
                    {
                        logger.log($"Moving {replicaFile} → {expectedReplicaPath}");
                        Directory.CreateDirectory(Path.GetDirectoryName(expectedReplicaPath)!);
                        File.Move(replicaFile, expectedReplicaPath, true);
                    }

                    sourceRemaining.RemoveAt(0);
                    replicaRemaining.RemoveAt(0);
                }

            }



            // 4. New files in source that do not exist in replica
            foreach (var filesource in sourceFiles)
            {
                string replicaFilePath = filesource.Replace(sourcePath, replicaPath);
                if (!File.Exists(replicaFilePath))
                {
                    logger.log($"File {filesource} does not exist in replica. Copying...");
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(replicaFilePath)); // Ensure directory exists
                    File.Copy(filesource, replicaFilePath);
                }
            }
            // 5. delete files in replica that do not exist in source

            foreach (var file in replicaFiles)
            {
                string sourceFilePath = file.Replace(replicaPath, sourcePath);
                if (!File.Exists(sourceFilePath))
                {
                    logger.log($"File {file} exists in replica but not in source. Deleting...");
                    File.Delete(file);
                }
            }

            // 6. delete empty directories in replica
            var replicaDirectories = Directory.EnumerateDirectories(replicaPath, "*", SearchOption.AllDirectories);
            var sourceDirectories = Directory.EnumerateDirectories(sourcePath, "*", SearchOption.AllDirectories);
            foreach (var dir in replicaDirectories)
            {
                if (!sourceDirectories.Contains(dir.Replace(replicaPath, sourcePath)))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                        logger.log($"Deleted empty directory: {dir}");
                    }
                    catch (Exception ex)
                    {
                        logger.log($"Failed to delete directory {dir}: {ex.Message}");
                    }
                }
            }
            // 7. create directories in replica that exist in source but not in replica
            foreach (var dir in sourceDirectories)
            {
                string replicaDir = dir.Replace(sourcePath, replicaPath);
                if (!Directory.Exists(replicaDir))
                {
                    try
                    {
                        Directory.CreateDirectory(replicaDir);
                        logger.log($"Created directory: {replicaDir}");
                    }
                    catch (Exception ex)
                    {
                        logger.log($"Failed to create directory {replicaDir}: {ex.Message}");
                    }
                }
            }
        }
    }
}
