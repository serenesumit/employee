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
        Task<UpFile> AddFileAsync(string containerName, Guid resumeId, string filename, Stream fileStream);
        MethodResult<Employee> Add(Employee model);
        Task<List<Employee>> GetAll();
        Employee Get(Guid id);
        Task<Employee> DeleteEmployee(Guid Id);
        bool DeleteEmployeeDocument(Guid Id, Guid resumeId);
    }
}
