using FolderSync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public class FileComparerTest
    {
        [Fact]
        public void Files_WithSameContent_ShouldBeEqual()
        {
            // Arrange
            var file1 = Path.GetTempFileName();
            var file2 = Path.GetTempFileName();
            File.WriteAllText(file1, "Hello world!");
            File.WriteAllText(file2, "Hello world!");

            // Act
            bool result = FileComparer.AreFilesEqual(file1, file2);

            // Assert
            Assert.True(result);

            // Cleanup
            File.Delete(file1);
            File.Delete(file2);
        }
    }
}
