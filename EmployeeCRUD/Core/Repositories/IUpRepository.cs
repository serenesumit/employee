using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    using Core.DbModels;
    #region using

    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;

    #endregion

    public interface IUpRepository : IDisposable
    {

        IDbSet<Employee> Employees { get; set; }

        IDbSet<EmployeeResume> EmployeeResumes { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync();
    }
}
