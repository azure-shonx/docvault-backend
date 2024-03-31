using System.IO;

public class DownloadableFile
{

    public string FileName { get; }
    public byte[] Data { get; }
    public string LocalFilePath { get; }

    public DownloadableFile(string FileName, byte[] Data)
    {
        this.FileName = FileName;
        this.Data = Data;
        LocalFilePath = Path.Combine("/app/temp/", Guid.NewGuid().ToString() + "_" + FileName);
        using (FileStream fs = File.Create(LocalFilePath))
        {
            fs.Write(Data);
        }
    }

    public override string ToString()
    {
        return LocalFilePath;
    }
}