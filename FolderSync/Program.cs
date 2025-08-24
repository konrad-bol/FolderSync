using FolderSync.Services;
using FolderSync.Utils;

FolderSynchronizer folderSync = new FolderSynchronizer(
    "C:\\Users\\kojad\\source\\repos\\test\\source",
    "C:\\Users\\kojad\\source\\repos\\test\\replica",
    new FileLogger("C:\\Users\\kojad\\source\\repos\\test\\log.txt")
);
await folderSync.start_sync();
