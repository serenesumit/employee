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
using System.Data.Entity;

namespace Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUpRepository _upRepository;

        private readonly IFileRepository _fileRepository;
        string storageAccount = System.Configuration.ConfigurationManager.AppSettings["StorageAccount"];
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
                    Employee employee = this._upRepository.Employees.Where(x => x.Id == model.Id.Value).FirstOrDefault();
                    if (employee != null)
                    {
                        employee.FirstName = model.FirstName;
                        employee.LastName = model.LastName;
                        employee.UpdatedDate = DateTime.UtcNow;
                    }
                }

                this._upRepository.SaveChanges();
            }
            catch (Exception ex)
            {

            }

            result.Result = model;
            return result;
        }

        public async Task<List<Employee>> GetAll()
        {
            var data = this._upRepository.Employees.Include(p => p.EmployeeResumes).ToList();
            return data;
        }

        public Employee Get(Guid id)
        {
            return this._upRepository.Employees.Include(p => p.EmployeeResumes).Where(p => p.Id == id).FirstOrDefault();
        }

        public async Task<Employee> DeleteEmployee(Guid Id)
        {
            var employeeResumes = this._upRepository.EmployeeResumes.Where(p => p.EmployeeId == Id).ToList();
            foreach (var resume in employeeResumes)
            {
                try
                {
                    var isFileDeleted = await this.DeleteFileAsync(resume.Name);
                    if (isFileDeleted)
                    {
                        this._upRepository.EmployeeResumes.Remove(resume);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            var employee = this._upRepository.Employees.Where(p => p.Id == Id).FirstOrDefault();
            this._upRepository.Employees.Remove(employee);
            this._upRepository.SaveChanges();
            return employee;
        }

        public bool DeleteEmployeeDocument(Guid Id, Guid resumeId)
        {
            var employeeResume = this._upRepository.EmployeeResumes.Where(p => p.Id == resumeId).FirstOrDefault();
            var isFileDeleted = this.DeleteFileAsync(employeeResume.Name);
            this._upRepository.EmployeeResumes.Remove(employeeResume);
            this._upRepository.SaveChanges();
            return true;
        }


        public virtual async Task<bool> DeleteFileAsync(string path)
        {
            try
            {
                this._fileRepository.Initialize(storageAccount, Constants.Azure.Containers.PageAssets);
                bool isDeleted = await this._fileRepository.DeleteFileAsync(path);
                return isDeleted;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public virtual async Task<UpFile> AddFileAsync(string containerName, Guid resumeId, string filename, Stream fileStream)
        {
            try
            {
                var path = string.Format(Constants.Azure.BlobPaths.EmployeeResumes, resumeId.ToString());
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




