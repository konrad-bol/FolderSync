using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public class FolderSynchronizerPeriodicSyncTests : TestHelper
    {
        [Fact]
        public void SyncIteration_ShouldSynchronizeFiles()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            var testFile = Path.Combine(sourcePath, "test.txt");
            File.WriteAllText(testFile, "First sync");

            // Act
            folderSync.SyncIteration();

            // Assert
            var replicaFile = Path.Combine(replicaPath, "test.txt");
            Assert.True(File.Exists(replicaFile));
            Assert.Equal("First sync", File.ReadAllText(replicaFile));
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath));

            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }

        [Fact]
        public async Task StartSync_ShouldSynchronizePeriodically()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            var testFile = Path.Combine(sourcePath, "test.txt");
            File.WriteAllText(testFile, "periodic sync");

            // Act
            var syncTask = folderSync.start_sync();
            await Task.Delay(25000);

            // Assert
            var replicaFile = Path.Combine(replicaPath, "test.txt");
            Assert.True(File.Exists(replicaFile));
            Assert.Contains($"[Created file: {Path.GetRelativePath(sourcePath, testFile)}]", File.ReadAllText(Path.Combine(tempRoot, "log.txt")));
            Assert.Equal(2, File.ReadAllLines(Path.Combine(tempRoot, "log.txt"))
                .Count(line => line.StartsWith("Synchronization completed")));

            Assert.Equal("periodic sync", File.ReadAllText(replicaFile));
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath));

            CleanupTestEnvironment(tempRoot);
        }
    }
}
