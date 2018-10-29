using Core;
using Core.DbModels;
using Core.Models;
using Core.Repositories;
using Core.Services;
using Core.StorageEntities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUpRepository _upRepository;

        private readonly IFileRepository _fileRepository;
        string storageAccount = "DefaultEndpointsProtocol=https;AccountName=employeesstorage;AccountKey=0WdZIvukhowm7ZxWqzLYA2NN7MMlVcMRhR0u+FU792ckgU6Bj81hIGDnuh95WSj5twrZsWtTdhzzkrn/Fv3EgQ==;EndpointSuffix=core.windows.net";
        public EmployeeService(IUpRepository upRepository,
             IFileRepository fileRepository
            )
        {
            this._upRepository = upRepository;
            this._fileRepository = fileRepository;
        }

        public virtual MethodResult<Employee> Add(Employee model)
        {
            var result = new MethodResult<Employee>();
            try
            {
                if (!model.Id.HasValue)
                {
                    model.Id = Guid.NewGuid();
                    model.IsDeleted = false;
                    model.CreatedDate = DateTime.UtcNow;
                    model.UpdatedDate = model.CreatedDate;
                    this._upRepository.Employees.Add(model);
                }
                else
                {
                    this._upRepository.Employees.Attach(model);
                }

                this._upRepository.SaveChanges();
            }
            catch (Exception ex)
            {

            }

            result.Result = model;
            return result;
        }

        public virtual async Task<UpFile> AddFileAsync(string containerName, string filename, Stream fileStream)
        {
            try
            {
                var path = string.Format(Constants.Azure.BlobPaths.EmployeeResumes, Guid.NewGuid().ToString());
                // http://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension
                var contentType = MimeMapping.GetMimeMapping(filename);
                this._fileRepository.Initialize(storageAccount, containerName);
                var storedFile = await this._fileRepository.StoreFileAsync(path, fileStream, contentType, filename);
                return storedFile;
            }
            catch (Exception ex)
            {
                return new UpFile();
            }

        }


    }
}




