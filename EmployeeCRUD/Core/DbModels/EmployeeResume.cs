using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DbModels
{
    public class EmployeeResume : BaseDbModel
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Link { get; set; }

        public virtual Employee Employee { get; set; }

        public Guid EmployeeId { get; set; }
    }
}
