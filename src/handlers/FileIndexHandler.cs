using System.Collections.ObjectModel;

public class FileIndexHandler
{
    private AzureCosmosHandler handler;
    private List<FileReference> Files;
    private readonly Object Lock = new Object();

    public FileIndexHandler(AzureCosmosHandler handler)
    {
        this.handler = handler;
        var res = LoadIndex().Result;
        if (res is null)
        {
            throw new InvalidOperationException("Files is null.");
        }
        Files = res;
    }

    private Task<List<FileReference>?> LoadIndex()
    {
        Task<List<FileReference>?> task = handler.GetIndex();
        task.Wait();
        return task;
    }

    private Task SaveFile(FileReference file)
    {
        return handler.UpdateFile(file);
    }

    public void AddFile(FileReference file)
    {
        lock (Lock)
        {
            this.Files.Add(file);
            handler.SaveFile(file).Wait();
        }
    }

    public FileReference? GetFile(string FileName)
    {
        lock (Lock)
        {
            foreach (FileReference File in Files)
                if (File.FileName.Equals(FileName))
                    return File;
        }
        return null;
    }

    public bool RemoveFile(FileReference file)
    {
        bool result;
        lock (Lock)
        {
            result = this.Files.Remove(file);
            handler.DeleteFile(file).Wait();
        }
        return result;
    }

    public void UpdateFile(FileReference fUpdate)
    {
        FileReference target = GetFile(fUpdate.FileName) ?? throw new InvalidOperationException("No file to update?");
        RemoveFile(target);
        AddFile(fUpdate);
    }

    public void RenameFile(string oldName, FileReference fUpdate)
    {
        FileReference target = GetFile(oldName) ?? throw new InvalidOperationException("No file to update?");
        RemoveFile(target);
        AddFile(fUpdate);
    }

    public bool RemoveFile(string FileName)
    {
        FileReference? target = GetFile(FileName);
        if (target is null)
            return false;
        return RemoveFile(target);
    }

    public ReadOnlyCollection<FileReference> GetFiles()
    {
        return Files.AsReadOnly();
    }
}