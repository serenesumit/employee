using Core.DbModels;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IEmployeeResumeService
    {
        MethodResult<EmployeeResume> Add(EmployeeResume model);
    }
}
