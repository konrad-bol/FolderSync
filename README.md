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
git clone https://github.com/your-repo/Foldersync.git
cd Foldersync
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
### 4. Run the tests
```bash
dotnet test ..\FolderSync.Tests\FolderSync.Tests.csproj
```
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

### Sync mechanism
The `Sync` method orchestrates the synchronization process:
- Detects new, modified, and deleted files using `FileComparer`.
- Applies the appropriate operation (copy, update, delete).
- Ensures consistency even when the process is interrupted.

### FileComparer (diff algorithm)
`FileComparer` detects differences between two folders using:
- **Timestamps** – modified date comparison
- **File size** – to detect changes without content inspection
- **Checksums (optional extension)** – for deeper verification in critical scenarios
## 🔄 Synchronization process (`Sync`)

The core synchronization logic is implemented in the `Sync` method.  
It detects changes in the **source** folder and applies the corresponding operations to the **replica**.  
The process is divided into **6 major steps**:

1. **Update modified files**  
   - If a file exists in both `source` and `replica` but their content differs, the file in the replica is overwritten.  
   - The change is logged, including a diff of modified lines.

2. **Detect moved files**  
   - Files with the same **MD5 hash** but located in a different path are recognized as moved.  
   - The file is relocated within the replica to match the new path.  
   - ⚠️ Limitation: when multiple files have identical content and are renamed/moved simultaneously, the algorithm may mismatch them (e.g., `file_1.txt → file_1_renamed.txt` could be confused with another file of the same content).  

3. **Add new files**  
   - Any file that exists in the source but not in the replica is copied.  

4. **Delete removed files**  
   - Files that no longer exist in the source are deleted from the replica.  

5. **Remove empty directories**  
   - Directories in the replica that no longer exist in the source are deleted.  

6. **Create missing directories**  
   - New directories found in the source are created in the replica.  

This ensures that the replica remains an exact mirror of the source folder, even when files are updated, moved, renamed, or removed.

---

## 📝 Diff algorithm (`FileComparer`)

The class `FileComparer` provides methods to calculate differences between files and generate human-readable change messages.

### `GetDifferents(string sourceFilePath, string replicaFilePath)`
- Implements a **line-based diff algorithm**.
- Produces a list of changes as tuples:
  - `(true, line, index)` → line exists only in **source** (added/changed).
  - `(false, line, index)` → line exists only in **replica** (removed/changed).
- The algorithm:
  1. Compares files line by line.
  2. Collects mismatches in a temporary list.
  3. Reduces this list by eliminating false positives (when a line is matched again later).
- The result is a **minimal set of differences**.

### `GetMessages(string sourceFilePath, string replicaFilePath)`
- Converts the raw differences from `GetDifferents` into readable change blocks.  
- Supports:
  - **Changed lines** (single or block ranges)  
    ```
    Changed lines 3–7:
      - old text
      + new text
    ```
  - **Removed lines**  
    ```
    Removed line at 5:
      - obsolete text
    ```
  - **Added lines**  
    ```
    Added line at 10:
      + new content
    ```
- These messages are written into the log file when synchronization detects modifications.

---

## 🧪 Tests

The project includes a comprehensive test suite to ensure correctness and reliability of the synchronization process.

### Covered components

- **FileComparer** – ensures correct detection of added, removed, or changed lines in text files.
- **FolderSynchronizer** – verifies synchronization logic such as copying, moving, and deleting files across directories.
- **Logging** – checks whether log messages are correctly generated and written both to the console and to the log file.

### TestHelper

All tests inherit from an abstract class **`TestHelper`**, which provides common setup and utility methods for consistency and reusability:

- `ArrangeTestEnvironment(out string sourcePath, out string replicaPath, out FolderSynchronizer folderSync, out string tempRoot)`  
  Prepares an isolated temporary test environment, including source and replica folders with nested directories, and initializes the synchronizer.

- `CleanupTestEnvironment(string tempRoot)`  
  Cleans up the temporary environment after test execution to ensure no leftover files or directories remain.

- `AreSourceAndReplicaEqual(string sourcePath, string replicaPath)`  
  Utility method to verify that source and replica directories contain identical files with the same content.

This structure ensures that every test runs in a controlled environment, preventing side effects and making the tests repeatable and reliable.

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
## Planned Improvements
- **Retry Mechanism Tests** - Add tests to verify the retry mechanism, ensuring that in case of an operation failure, the file is correctly re-queued for subsequent attempts.
- **Smarter Diff** - Improve the file comparison algorithm to be faster and provide more detailed change reports.
- **Versioned Backups** - Add a mechanism for versioning replicas (e.g., replica-[datetime]), similar to Git. Each sync push could create a new backup version, allowing you to revert to previous data states.
## ✅ Summary

**FolderSync** is a lightweight folder synchronization tool with:
- Automatic synchronization loop
- File comparison by content (MD5 hash)
- Change detection at file and line level
- Detailed logs (console + file)
- Retry mechanism for file operations

---
