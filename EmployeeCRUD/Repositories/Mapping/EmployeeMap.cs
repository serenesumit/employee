using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Mapping
{
    using Core.DbModels;
    #region using

    using System.Data.Entity.ModelConfiguration;


    #endregion

    internal class EmployeeMap : EntityTypeConfiguration<Employee>
    {
        public EmployeeMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            //// this.Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            // Properties
            this.Property(t => t.FirstName).IsRequired().HasMaxLength(200);
            this.Property(t => t.LastName).HasMaxLength(200);

            // Table & Column Mappings
            this.ToTable("Employees");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.FirstName).HasColumnName("FirstName");
            this.Property(t => t.LastName).HasColumnName("LastName");
            this.HasMany(c => c.EmployeeResumes).WithRequired().HasForeignKey(c => c.EmployeeId);

        }
    }
}
