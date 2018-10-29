using Core.DbModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IEmployeeService
    {
        Task<UpFile> AddFileAsync(string containerName, string filename, Stream fileStream);
        MethodResult<Employee> Add(Employee model);
    }
}
