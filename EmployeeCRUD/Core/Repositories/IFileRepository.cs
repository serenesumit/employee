namespace Core.Repositories
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Core.Models;
    using Microsoft.WindowsAzure.Storage.Blob;

    #endregion

    public interface IFileRepository
    {
      
        Task<bool> DeleteFileAsync(string path);

        Task DeleteFolderAsync(string path);

        Task<bool> FileExists(string path, string prefix = null);

       
        void Initialize(string connectionString, string containerName);

        Task StoreFileAsync(string path, string content);

        Task<UpFile> StoreFileAsync(
            string path,
            Stream content,
            string contentType = null,
            string fileName = null,
            Dictionary<string, string> metadata = null);

        Task StoreFileAsync(string path, byte[] content);

        void UpdateFileProperties(string path, string contentType, string contentDisposition);
    }
}
