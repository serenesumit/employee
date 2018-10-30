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
    [RoutePrefix("api/Employee")]
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
                    EmployeeResume employeeResume = new EmployeeResume();
                    employeeResume.Id = Guid.NewGuid();
                    var fileResult = await this._employeeService.AddFileAsync(Constants.Azure.Containers.PageAssets, employeeResume.Id.Value, file.FileName, stream);

                    employeeResume.Link = fileResult.FullPath;
                    employeeResume.Name = fileResult.Name;
                    employeeResume.EmployeeId = employee.Id.Value;
                    this._employeeResumeService.Add(employeeResume);
                    employee.EmployeeResumes.Add(employeeResume);
                }

                result = Request.CreateResponse(HttpStatusCode.Created, employee);
            }

            return result;

        }

        public async Task<IEnumerable<Employee>> Get()
        {
            var result = await this._employeeService.GetAll();
            List<Employee> model = new List<Employee>();
            foreach (var employee in result)
            {
                Employee employeeModel = new Employee();
                foreach (var resume in employee.EmployeeResumes)
                {
                    EmployeeResume employeeResume = new EmployeeResume();
                    employeeResume.EmployeeId = resume.EmployeeId;
                    employeeResume.Name = resume.Name;
                    employeeResume.Link = resume.Link;
                    employeeResume.Id = resume.Id;
                    employeeModel.EmployeeResumes.Add(employeeResume);
                }

                employeeModel.FirstName = employee.FirstName;
                employeeModel.LastName = employee.LastName;
                employeeModel.Id = employee.Id;
                model.Add(employeeModel);
            }

            return model;
        }

        [HttpGet]
        public Employee Get(Guid id)
        {
            return this._employeeService.Get(id);
        }

        [HttpPut]
        public async Task<HttpResponseMessage> PutEmployee(EmployeeModel model)
        {
            HttpResponseMessage result = null;

            if (!model.Id.HasValue)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            Employee employee = this._employeeService.Get(model.Id.Value);
            if (employee == null)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            this._employeeService.Add(employee);

            if (model.Files != null && model.Files.Count > 0)
            {

                foreach (var file in model.Files)
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Write(file.Buffer, 0, file.Buffer.Length);
                    EmployeeResume employeeResume = new EmployeeResume();
                    employeeResume.Id = Guid.NewGuid();
                    var fileResult = await this._employeeService.AddFileAsync(Constants.Azure.Containers.PageAssets, employeeResume.Id.Value, file.FileName, stream);

                    employeeResume.Link = fileResult.FullPath;
                    employeeResume.Name = fileResult.Name;
                    employeeResume.EmployeeId = employee.Id.Value;
                    this._employeeResumeService.Add(employeeResume);
                }


                result = Request.CreateResponse(HttpStatusCode.Created, employee);
            }

            return result;
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteEmployee(Guid? id)
        {
            HttpResponseMessage result = null;
            if (!id.HasValue)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            await this._employeeService.DeleteEmployee(id.Value);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("{employeeId:guid}/employee-resume/{resumeId:guid}")]
        public async Task<HttpResponseMessage> DeleteEmployee(Guid? employeeId, Guid? resumeId)
        {
            HttpResponseMessage result = null;
            if (!employeeId.HasValue || !resumeId.HasValue)
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            this._employeeService.DeleteEmployeeDocument(employeeId.Value, resumeId.Value);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
