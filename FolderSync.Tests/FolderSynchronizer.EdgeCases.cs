using FolderSync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public class FolderSynchronizerEdgeCases : TestHelper
    {
        // This class is intended for edge case tests that do not fit into the standard test categories.
        // It can be used to test unusual scenarios or specific edge cases that may arise during folder synchronization.

        [Fact]
        public void Sync_ShouldHandleEmptySourceAndReplica()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            // Act
            folderSync.Sync();
            // Assert
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Both source and replica should be empty and equal after sync.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }

        [Fact]
        public void Sync_ShouldChangeOnlyOneFile_WhenSourceHasManyFilesButOnlyOneIsModified()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            // Create multiple files in the source and replica
            for (int i = 0; i < 5; i++)
            {
                File.WriteAllText(Path.Combine(sourcePath, $"file{i}.txt"), $"Content {i}");
                File.WriteAllText(Path.Combine(replicaPath, $"file{i}.txt"), $"Content {i}");
            }

            // Modify one file in the source
            File.WriteAllText(Path.Combine(sourcePath, "file2.txt"), "New Content");
            // Act
            folderSync.Sync();
            // Assert
            Assert.True(FileComparer.AreFilesEqual(Path.Combine(sourcePath, "file2.txt"), Path.Combine(replicaPath, "file2.txt")), "Only the modified file should be updated in the replica.");
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync, except for the modified file.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }

        [Fact]
        public void Sync_ShouldHandleMovingAndChangingNameofFile_WhenSourceHasNestedDirectories()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);

            // Create nested directories and files in the source
            var nestedSourceDir1 = Path.Combine(sourcePath, "Nested_1");
            var nestedSourceDir2 = Path.Combine(sourcePath, "Nested_2");
            File.WriteAllText(Path.Combine(nestedSourceDir1, "Nested_11", "Nested_111", "file.txt"), "This is a file in a nested_1 directory.");
            File.WriteAllText(Path.Combine(nestedSourceDir2, "Nested_21", "change_name_file.txt"), "This is a file in a nested_2 directory.");

            var nestedReplicaDir1 = Path.Combine(replicaPath, "Nested_1");
            var nestedReplicaDir2 = Path.Combine(replicaPath, "Nested_2");
            File.WriteAllText(Path.Combine(nestedReplicaDir1, "file.txt"), "This is a file in a nested_1 directory.");
            File.WriteAllText(Path.Combine(nestedReplicaDir2, "file.txt"), "This is a file in a nested_2 directory.");

            // Act
            folderSync.Sync();
            // Assert
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }

        [Fact]
        public void Sync_ShouldHandleChangingManyFilesAddingAndDeletingFiles_WhenSourceHasManyChanges()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            File.WriteAllText(Path.Combine(sourcePath, "file1.txt"), "Same Content");
            File.WriteAllText(Path.Combine(replicaPath, "file1.txt"), "Same Content");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "file2.txt"), "Same Content");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file2.txt"), "Same Content");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "file3.txt"), "New Content 1");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file3.txt"), "Content 1");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "file4.txt"), "new Content 2");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file4.txt"), "Content 2");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "Nested_11", "Nested_111", "file5.txt"), "new Content 3");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "Nested_11", "Nested_111", "file5.txt"), "Content 3");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_2", "file6.txt"), "new file");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_2", "Nested_21", "file7.txt"), "Old Content");



            // Act
            folderSync.Sync();
            // Assert

            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }
        [Fact]
        public void Sync_ShouldHandleAllChanges_WhenSourceHasManyChanges()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            File.WriteAllText(Path.Combine(sourcePath, "Nested_3", "rename_file.txt"), "Same Content");
            File.WriteAllText(Path.Combine(replicaPath, "file1.txt"), "Same Content");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "Nested_12", "file2.txt"), "Same Content");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file2.txt"), "Same Content");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "file3.txt"), "New Content 1");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file3.txt"), "Content 1");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "file4.txt"), "new Content 2");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file4.txt"), "Content 2");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_1", "Nested_11", "Nested_111", "file5.txt"), "new Content 3");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "Nested_11", "Nested_111", "file5.txt"), "Content 3");

            File.WriteAllText(Path.Combine(sourcePath, "Nested_2", "file6.txt"), "new file");
            File.WriteAllText(Path.Combine(replicaPath, "Nested_2", "Nested_21", "file7.txt"), "Old Content");



            // Act
            folderSync.Sync();
            // Assert

            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }
        [Fact]
        public void Sync_ShouldHandleSwapedFiles_WhenSourceHasSwappedFiles()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            File.WriteAllText(Path.Combine(sourcePath, "Nested_1","file1.txt"), "Content 2");
            File.WriteAllText(Path.Combine(sourcePath, "file2.txt"), "Content 1");

            File.WriteAllText(Path.Combine(replicaPath, "Nested_1", "file1.txt"), "Content 1");
            File.WriteAllText(Path.Combine(replicaPath, "file2.txt"), "Content 2");

            // Act
            folderSync.Sync();
            // Assert
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync.");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }
    }


}
