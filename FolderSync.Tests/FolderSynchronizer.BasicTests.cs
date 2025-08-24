using FolderSync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSync.Services;

namespace FolderSync.Tests
{
    public class FolderSynchronizerBasicTests : TestHelper
    {
        [Fact]
        public void sync_ShouldCreateFilesInReplica_WhenSourceHasNewFiles()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);

            //Create a new file in the source
            var fileInSource = Path.Combine(sourcePath, "newfile.txt");
            File.WriteAllText(fileInSource, "This is a new file in source.");
            var fileInReplica = Path.Combine(replicaPath, "newfile.txt");

            // Act
            folderSync.Sync();
            // Assert
            Assert.True(FileComparer.AreFilesEqual(fileInSource, fileInReplica),"File form source should be create when it dosent exist in replica ");
            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }
        [Fact]
        public void Sync_ShouldDeleteFilesInReplica_WhenSourceHasNoCorrespondingFiles()
        {
            //Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);

            //Create file in the replica
            var fileInReplica = Path.Combine(replicaPath, "fileToDelete.txt");
            File.WriteAllText(fileInReplica, "This file should be deleted in replica.");

            //Act
            folderSync.Sync();
            //Assert
            Assert.False(File.Exists(fileInReplica), "File in replica should be deleted when it does not exist in source.");
            Assert.True(AreSourceAndReplicaEqual(sourcePath, replicaPath), "Source and replica should be equal after sync, except for the deleted file.");
            //Cleanup
            CleanupTestEnvironment(tempRoot);
        }
        [Fact]
        public void sync_ShouldUpdateFilesInReplica_WhenSourceHasDifferentContent()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);

            // Create a file in the source with some content
            var fileInSource = Path.Combine(sourcePath, "file.txt");
            File.WriteAllText(fileInSource, "Hello world!");
            // Create a file in the replica with different content
            var fileInReplica = Path.Combine(replicaPath, "file.txt");
            File.WriteAllText(fileInReplica, "Goodbye world!");
            // Act
            folderSync.Sync();
            // Assert
            Assert.True(FileComparer.AreFilesEqual(fileInSource, fileInReplica));
            // Cleanup
            CleanupTestEnvironment(tempRoot);

        }
        [Fact]
        public void Syns_ShouldMoveFilesInReplica_WhenSourceHasSameContentAndDifferentPath()
        {
            // Arrange

            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            // Create a file in the source
            var fileInSource = Path.Combine(sourcePath, "dir", "file.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(fileInSource)); // Ensure the directory exists
            File.WriteAllText(fileInSource, "This is a moved file");

            var fileInReplica = Path.Combine(replicaPath, "file.txt");
            File.WriteAllText(fileInReplica, "This is a moved file");

            var newPathInReplica = Path.Combine(replicaPath, "dir","file.txt");
            // Act
            folderSync.Sync();
            // Assert
            Assert.True(FileComparer.AreFilesEqual(fileInSource, newPathInReplica), "File in replica should be moved to the new path when it has the same content as in source.");

            // Cleanup
            CleanupTestEnvironment(tempRoot);
        }
        
    }
}
