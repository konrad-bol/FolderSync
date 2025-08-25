using FolderSync.Services;
using FolderSync.Utils;


if (args.Length < 3)
{
    Console.WriteLine("Usage: dotnet run -- <sourcePath> <replicaPath> <logFilePath>");
    return;
}

string sourcePath = args[0];
string replicaPath = args[1];
string logFilePath = args[2];

FolderSynchronizer folderSync = new FolderSynchronizer(
    sourcePath,
    replicaPath,
    new FileLogger(logFilePath)
);

await folderSync.StartSync();
