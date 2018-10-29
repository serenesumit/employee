using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DbModels
{
    public class Employee : BaseDbModel
    {
        public Employee()
        {
            this.EmployeeResumes = new List<EmployeeResume>();
        }

        public Guid? Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual ICollection<EmployeeResume> EmployeeResumes { get; set; }
    }
}
