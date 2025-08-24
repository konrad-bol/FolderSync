using FolderSync.Services;
using FolderSync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public abstract class TestHelper
    {
        protected void ArrangeTestEnvironment(out string sourcePath, out string replicaPath, out FolderSynchronizer folderSync, out string tempRoot)
        {
            tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            sourcePath = Path.Combine(tempRoot, "SourceFolder");
            replicaPath = Path.Combine(tempRoot, "ReplicaFolder");
            Directory.CreateDirectory(sourcePath);
            Directory.CreateDirectory(replicaPath);
            var nestedDirectory = new List<string>();
            nestedDirectory.Add(Path.Combine("Nested_1", "Nested_11", "Nested_111"));
            nestedDirectory.Add(Path.Combine("Nested_1", "Nested_12"));
            nestedDirectory.Add(Path.Combine("Nested_2", "Nested_21"));
            nestedDirectory.Add(Path.Combine("Nested_3"));
            foreach (var dir in nestedDirectory)
            {
                Directory.CreateDirectory(Path.Combine(sourcePath, dir));
                Directory.CreateDirectory(Path.Combine(replicaPath, dir));
            }
            var logger = new FileLogger(Path.Combine(tempRoot, "log.txt"));
            folderSync = new FolderSynchronizer(sourcePath, replicaPath, logger);
        }
        protected void CleanupTestEnvironment(string tempRoot)
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
        protected bool AreSourceAndReplicaEqual(string sourcePath, string replicaPath)
        {
            var sourceFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            var replicaFiles = Directory.GetFiles(replicaPath, "*", SearchOption.AllDirectories);

            for (int i = 0; i < sourceFiles.Length; i++)
            {
                if (!FileComparer.AreFilesEqual(sourceFiles[i], replicaFiles[i]))
                    return false;
            }
            return true;
        }
    }
}
