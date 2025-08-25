# FolderSync

## 📌 Project Description

**FolderSync** is a C# application for synchronizing the contents of a source folder with a replica folder.  
It ensures the replica always reflects the current state of the source by detecting changes, additions, and deletions of files and directories.  
The application provides detailed logs (both in the terminal and in log files) with information about synchronization operations.

---

## ⚙️ Requirements

- .NET 6.0 or newer (recommended: .NET 9)
- Windows, Linux, or macOS

---

## 🚀 Setup & Execution

### 1. Clone the repository
```bash
git clone https://github.com/your-repo/foldersync.git
cd foldersync
```

### 2. Build the project

```bash
dotnet build
```

### 3. Run the synchronizer

Provide:
- source directory path
- replica directory path
- log file path
```bash
dotnet run -- "C:\path\to\source" "C:\path\to\replica" "C:\path\to\log.txt"
```

The program runs in a loop, periodically checking for changes (default: every 10 seconds).

---

## 📚 Components & Methods

### `ILogger` (interface)
- `start_log()` – marks the beginning of a synchronization cycle
- `end_log()` – marks the end of a synchronization cycle
- `log(string message)` – writes a single log message
- `log_change(List<string> messages)` – logs a list of line-by-line file differences

### `FileLogger` (implementation of `ILogger`)
- Logs messages to the console and to a log file

### `FolderSynchronizer`
- Handles synchronization logic
- `Task StartSync()` – starts the infinite synchronization loop with a periodic timer
- `void SyncIteration()` – performs a single synchronization cycle (start log → sync → end log)
- `void Sync()` – main synchronization method. Detects file changes, copies, deletions, and moves files/directories
- `private void ActionFileWithRetry(FileCase, string source, string destination, int retryCount = 3, int delayMs = 100)`  
  Handles file operations (copy/move/delete) with retries in case of temporary errors (e.g., locked files)

### `FileComparer` (static class in Utils)
- Utility class for comparing files
- `GetMD5(string filePath)` – returns MD5 hash of a file
- `AreFilesEqual(string file1, string file2)` – compares two files by content
- `GetDifferents(string sourceFilePath, string replicaFilePath)` – returns a list of differences between two files (line by line)
- `GetMessages(string sourceFilePath, string replicaFilePath)` – returns human-readable messages about changes (added/removed/changed lines)

---

## 🧪 Tests

The project includes unit tests for:
- **FileComparer** – ensuring correct detection of added, removed, or changed lines
- **FolderSynchronizer** – verifying synchronization logic (copying, moving, and deleting files)
- **Logging** – checking if log messages are correctly generated and written
- **Retry mechanism** – confirming that locked files are retried before failing

**Example test (xUnit):**
```csharp
        [Fact]
        public async Task StartSync_ShouldSynchronizePeriodically()
        {
            // Arrange
            ArrangeTestEnvironment(out var sourcePath, out var replicaPath, out var folderSync, out var tempRoot);
            var testFile = Path.Combine(sourcePath, "test.txt");
            File.WriteAllText(testFile, "periodic sync");

            // Act
            var syncTask = folderSync.StartSync();
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
```

---

## 📖 Example Logs

**Example Log File (`log.txt`):**
```
Synchronization started from C:\Users\kojad\source\repos\test\source to C:\Users\kojad\source\repos\test\replica
[Changed file: lorem_ipsum.txt]
Changed lines 7-8:
  - 
  - Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua
  + Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, 
  + totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. 
Synchronization completed from C:\Users\kojad\source\repos\test\source to C:\Users\kojad\source\repos\test\replica
[25.08.2025 13:25:55] Log ended.
```

---

## ✅ Summary

**FolderSync** is a lightweight folder synchronization tool with:
- Automatic synchronization loop
- File comparison by content (MD5 hash)
- Change detection at file and line level
- Detailed logs (console + file)
- Retry mechanism for file operations

---
