namespace Repositories.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class upateEmployeResume : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EmployeeResumes", "Name", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.EmployeeResumes", "Name");
        }
    }
}
