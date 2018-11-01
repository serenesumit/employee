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
    using Core.Models;
    using Core.Repositories;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;



    #endregion

    public class AzureBlobStorageFileRepository : IFileRepository
    {
        private readonly string _devPath = "/devstoreaccount1/";

      

        private CloudBlobClient _client;

        private string _containerName;

        private bool _isInitialized = false;


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
