using Core;
using Core.DbModels;
using Core.Models;
using Core.Services;
using EmployeeCRUD.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace EmployeeCRUD.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EmployeeController : ApiController
    {

        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeResumeService _employeeResumeService;

        public EmployeeController(
            IEmployeeService employeeService,
             IEmployeeResumeService employeeResumeService
          )
        {
            this._employeeService = employeeService;
            this._employeeResumeService = employeeResumeService;

        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostEmployee(EmployeeModel model)
        {

            HttpResponseMessage result = null;

            if (string.IsNullOrEmpty(model.FirstName))
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            Employee employee = new Employee();
            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            this._employeeService.Add(employee);

            if (model.Files != null && model.Files.Count > 0)
            {

                foreach (var file in model.Files)
                {
                    //Stream stream = new MemoryStream(file.Buffer);
                    MemoryStream stream = new MemoryStream();

                    stream.Write(file.Buffer, 0, file.Buffer.Length);
                    try
                    {
                        var fileResult = await this._employeeService.AddFileAsync(Constants.Azure.Containers.PageAssets, file.FileName, stream);
                        EmployeeResume employeeResume = new EmployeeResume();
                        employeeResume.Link = fileResult.FullPath;
                        employeeResume.EmployeeId = employee.Id.Value;
                        this._employeeResumeService.Add(employeeResume);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.InnerException.ToString());
                    }

                }

                result = Request.CreateResponse(HttpStatusCode.Created, "");
            }

            return result;

        }
    }
}
