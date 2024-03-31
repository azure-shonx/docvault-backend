using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class AzureStorageHandler
{

    private BlobServiceClient bsc;
    private BlobContainerClient bcc;

    public AzureStorageHandler()
    {
        bsc = new BlobServiceClient(
                new Uri("https://shonxdocvault.blob.core.windows.net"),
                new DefaultAzureCredential());
        bcc = bsc.GetBlobContainerClient("documents");
    }

    public void GetDocument()
    {
        // Call Azure Function to get download URL
    }

    public async void SaveDocument(string FileName, byte[] document)
    {
        BlobClient bc = bcc.GetBlobClient(FileName);
        DownloadableFile fileRef = new DownloadableFile(FileName, document);
        await bc.UploadAsync(fileRef.LocalFilePath, true);
    }

    public async void DeleteDocument(string FileName)
    {
        BlobClient bc = bcc.GetBlobClient(FileName);
        await bc.DeleteAsync();
    }

    public async void RenameDocument(string OldName, string NewName)
    {
        // This is kinda difficult, there's no "rename" it's copy/paste
        // https://stackoverflow.com/questions/3734672/azure-storage-blob-rename/26549519#26549519
        BlobClient oldBlob = bcc.GetBlobClient(OldName);
        BlobClient newBlob = bcc.GetBlobClient(NewName);
        BlobCopyInfo info = await newBlob.SyncCopyFromUriAsync(oldBlob.Uri);
        while (info.CopyStatus.Equals(CopyStatus.Pending))
        {
            await Task.Delay(100);
        }
        switch (info.CopyStatus)
        {
            case CopyStatus.Success:
                {
                    await oldBlob.DeleteAsync();
                    break;
                }
            case CopyStatus.Aborted:
                {
                    await newBlob.DeleteAsync();
                    break;
                }
            case CopyStatus.Failed:
                {
                    await newBlob.DeleteAsync();
                    break;
                }
        }
    }
}