using FolderSync.Services;
using FolderSync.Utils;

Console.WriteLine("start");
FolderSynchronizer folderSync = new FolderSynchronizer(
    "C:\\Users\\kojad\\source\\repos\\test\\source",
    "C:\\Users\\kojad\\source\\repos\\test\\replica",
    new FileLogger("C:\\Users\\kojad\\source\\repos\\test\\log.txt")
);
folderSync.Sync();

var source = Path.GetTempFileName();
var replica = Path.GetTempFileName();
File.WriteAllLines(source, new[] { "1", "New content", "still new content", "B", "3", "C" });
File.WriteAllLines(replica, new[] { "1", "Old content", "....", "Y", "3","Change","added" });
var mess = FileComparer.GetMessages(source, replica);
foreach (var message in mess)
{
    Console.WriteLine(message);
}

// Cleanup
File.Delete(source);
File.Delete(replica);