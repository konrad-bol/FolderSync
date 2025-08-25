using FolderSync.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    public class FolderSynchronizerLoggerTests : TestHelper
    {
        [Fact]
        public void FileLogger_ShouldLogMessagesToFile()
        {
            // Arrange
            var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempRoot);
            var logFilePath = Path.Combine(tempRoot, "log.txt");
            var logger = new FileLogger(logFilePath);

            // Act
            logger.start_log();
            logger.log("Test message 1");
            logger.log("Test message 2");
            logger.end_log();

            // Assert
            var logContent = File.ReadAllText(logFilePath);
            Assert.Contains("Test message 1", logContent);
            Assert.Contains("Test message 2", logContent);

            // Cleanup
            Directory.Delete(tempRoot, true);
        }
    }
}
