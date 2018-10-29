using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Helpers;
    using Core.Models;
    using Core.Repositories;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;



    #endregion

    public class AzureBlobStorageFileRepository : IFileRepository
    {
        private readonly string _devPath = "/devstoreaccount1/";

        private readonly IObjectMapper _objectMapper;

        private CloudBlobClient _client;

        private string _containerName;

        private bool _isInitialized = false;

        public AzureBlobStorageFileRepository(IObjectMapper objectMapper)
        {
            this._objectMapper = objectMapper;
        }

        public async Task<bool> CopyFilesAsync(string sourcePath, string destinationPath)
        {
            this.CheckIfIsInitialized();
            var start = DateTime.UtcNow;
            var container = this._client.GetContainerReference(this._containerName);
            var sourceItems = container.ListBlobs(sourcePath, true, BlobListingDetails.None);
            var blobs = new List<CloudBlockBlob>(sourceItems.Count());

            foreach (var item in sourceItems)
            {
                var source = item as CloudBlockBlob;
                if (source == null)
                {
                    continue;
                }

                var destination =
                    container.GetBlockBlobReference(source.Name.ToString().Replace(sourcePath, destinationPath));
                await destination.StartCopyAsync(source);

                blobs.Add(destination);
            }

            // Wait a small amount of time to allow the pending operations to complete.   
            if (blobs.Count > 0)
            {
                if (this.IsRunningOnDev(blobs[0].Uri))
                {
                    // this a bug in Azure storage emulator not updating the CopyStatus
                    while (blobs.Count != blobs.Count(b => b.Exists() != false))
                    {
                        await Task.Delay(50);
                    }
                }
                else
                {
                    while (blobs.Count != blobs.Count(b => b.CopyState.Status != CopyStatus.Pending))
                    {
                        await Task.Delay(50);
                    }
                }
            }

            var span = DateTime.UtcNow.Subtract(start);

            return blobs.Any(b => b.CopyState.Status != CopyStatus.Success);
        }

        public async Task<UpFile> CreateSnapshot(string path)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);

            CloudBlockBlob snapshot = await blockBlob.CreateSnapshotAsync();

            var file = new UpFile()
            {
                Path = snapshot.SnapshotQualifiedUri.ToString(),
                CreatedDate = snapshot.SnapshotTime.Value.UtcDateTime
            };
            return file;
        }

        public async Task<bool> DeleteFileAsync(string path)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);

            return await blockBlob.DeleteIfExistsAsync();
        }

        public async Task DeleteFolderAsync(string path)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blobs = container.ListBlobs(path, true, BlobListingDetails.None);

            foreach (var item in blobs)
            {
                var blob = item as CloudBlockBlob;
                if (blob == null)
                {
                    continue;
                }

                await blob.DeleteAsync();
            }
        }

        public async Task<bool> FileExists(string path, string prefix = null)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);

            // Check if the file has been explicitly defined.
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return await container.GetBlockBlobReference(path).ExistsAsync();
            }

            // The caller has requested a search for the file.
            var blob =
                await
                container.ListBlobsSegmentedAsync(
                    string.Format("{0}/{1}", path, prefix),
                    true,
                    BlobListingDetails.None,
                    1,
                    null,
                    null,
                    null);

            return blob.Results.Any();
        }

        public string GetSecureDownloadLink(string filePath, string sharedAccessPolicyName)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(filePath);
            string sasToken = blockBlob.GetSharedAccessSignature(null, sharedAccessPolicyName);
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blockBlob.Uri, sasToken);
        }

        public string GetSecureDownloadLink(
            string filePath,
            SharedAccessBlobPermissions permission,
            int sasMinutesValid)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(filePath);

            var sasToken =
                blockBlob.GetSharedAccessSignature(
                    new SharedAccessBlobPolicy()
                    {
                        Permissions = permission,
                        SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-15),
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(sasMinutesValid),
                    });

            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", blockBlob.Uri, sasToken);
        }



        public void Initialize(string connectionString, string containerName)
        {
            if (this._isInitialized)
            {
                return;
            }

            this._containerName = containerName;

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            this._client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cont = this._client.GetContainerReference(Constants.Azure.Containers.PageAssets);

            cont.CreateIfNotExists();
            this._isInitialized = true;
        }

        public List<UpFile> ListFiles(string prefix)
        {
            this.CheckIfIsInitialized();
            var assets = new List<UpFile>();
            var container = this._client.GetContainerReference(this._containerName);

            var blobs = container.ListBlobs(prefix, true, BlobListingDetails.None);

            foreach (var blob in blobs)
            {
                var uri = blob.Uri;

                if (this.IsRunningOnDev(uri))
                {
                    var changedAbsolutePath = uri.AbsolutePath.Substring(this._devPath.Length);
                    var url = string.Format("{0}://{1}/{2}", uri.Scheme, uri.Authority, changedAbsolutePath);
                    uri = new Uri(url);
                }

                assets.Add(this._objectMapper.AzureBlobUriToUpFile(uri));
            }

            return assets;
        }

        public async Task<string> ReadFileAsStringAsync(string path)
        {
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);
            return await blockBlob.DownloadTextAsync();

            // OR without the container.
            ////var blockBlob = container.GetBlockBlobReference(path);
            ////return await blockBlob.DownloadTextAsync();
        }

        public async Task ReadFileToStreamAsync(string path, Stream stream)
        {
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);
            await blockBlob.DownloadToStreamAsync(stream);
        }

        public async Task StoreFileAsync(string path, string content)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);

            await blockBlob.UploadTextAsync(content);
        }

        public async Task<UpFile> StoreFileAsync(
            string path,
            Stream content,
            string contentType = null,
            string fileName = null,
            Dictionary<string, string> metadata = null)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);
            if (!string.IsNullOrEmpty(contentType))
            {
                blockBlob.Properties.ContentType = contentType;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                blockBlob.Properties.ContentDisposition = "attachment; filename=" + fileName;
            }

            if (metadata != null)
            {
                foreach (var md in metadata)
                {
                    blockBlob.Metadata.Add(md);
                }
            }

            content.Position = 0;
            try
            {
                await blockBlob.UploadFromStreamAsync(content);
            }
            catch (Exception ex)
            {

            }

            var result = new UpFile()
            {
                FullPath = blockBlob.Uri.ToString(),
                Name = blockBlob.Name,
                Size = blockBlob.StreamWriteSizeInBytes,
            };

            return result;
        }

        public async Task StoreFileAsync(string path, byte[] content)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);

            await blockBlob.UploadFromByteArrayAsync(content, 0, content.Length);
        }



        public void UpdateFileProperties(string path, string contentType = null, string contentDisposition = null)
        {
            this.CheckIfIsInitialized();
            var container = this._client.GetContainerReference(this._containerName);
            var blockBlob = container.GetBlockBlobReference(path);

            if (!string.IsNullOrWhiteSpace(this._containerName))
            {
                blockBlob.Properties.ContentType = contentType;
            }

            if (!string.IsNullOrWhiteSpace(contentDisposition))
            {
                blockBlob.Properties.ContentDisposition = contentDisposition;
            }

            blockBlob.SetProperties();
        }

        private void CheckIfIsInitialized(string containerName = null)
        {
            if (!this._isInitialized)
            {
                throw new Exception(
                    "Azure Blob Repository has not bee initialized yet. You must manually initialize the class");
            }

            // this.EnsureContainerExists(containerName);
        }

        private void EnsureContainerExists(string containerName)
        {
            var container = this._client.GetContainerReference(containerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Blob);
        }



        private bool IsRunningOnDev(Uri uri)
        {
            if (uri.AbsolutePath.StartsWith(this._devPath))
            {
                return true;
            }

            return false;
        }
    }
}
