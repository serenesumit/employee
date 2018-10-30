using Core.DbModels;
using MultipartDataMediaFormatter.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EmployeeCRUD.Models
{
    public class EmployeeModel
    {
        public Guid? Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<HttpFile> Files { get; set; }

        public List<EmployeeResume> EmployeeResumes { get; set; }
    }
}