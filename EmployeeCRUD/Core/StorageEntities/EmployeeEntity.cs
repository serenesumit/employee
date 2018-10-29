using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Core.StorageEntities
{
    public class EmployeeEntity : TableEntity
    {
        public EmployeeEntity()
        {
            this.RowType = "Employee";
        }

        public EmployeeEntity(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {
            this.RowType = "Employee";
        }

        public string FirstName { get; set; }

        public Guid CreatedBy { get; set; }

        public string LastName { get; set; }

        public string CreatedByName { get; set; }

        public virtual DateTime CreatedDate { get; set; }

        public virtual bool IsDeleted { get; set; }

        public string RowType { get; set; }
    }
}
