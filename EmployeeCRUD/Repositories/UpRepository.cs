using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    using Core.DbModels;
    using Core.Repositories;
    using Repositories.Mapping;
    #region using

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Threading.Tasks;


    #endregion

    public class UpRepository : DbContext, IUpRepository
    {
        static UpRepository()
        {
            Database.SetInitializer<UpRepository>(null);
        }

        public UpRepository()
            : base("Name=EmployeeContext")
        {
            //// (connectionString)
            this.Configuration.LazyLoadingEnabled = false;
        }

        public IDbSet<Employee> Employees { get; set; }

        public IDbSet<EmployeeResume> EmployeeResumes { get; set; }



        public DbContext DbContext
        {
            get
            {
                return this;
            }
        }

        public override int SaveChanges()
        {
            try
            {
                // Save changes with the default options
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                throw;
            }
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                    }
                }

                throw;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            this.Configuration.LazyLoadingEnabled = false;
            modelBuilder.Configurations.Add(new EmployeeMap());
            modelBuilder.Configurations.Add(new EmployeeResumeMap());

        }
    }
}
