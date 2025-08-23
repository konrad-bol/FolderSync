using FolderSync.Services;

Console.WriteLine("start");
FolderSynchronizer folderSync = new FolderSynchronizer(
    "C:\\Users\\kojad\\source\\repos\\test\\source",
    "C:\\Users\\kojad\\source\\repos\\test\\replica",
    new FileLogger("C:\\Users\\kojad\\source\\repos\\test\\log.txt")
);
folderSync.Sync();