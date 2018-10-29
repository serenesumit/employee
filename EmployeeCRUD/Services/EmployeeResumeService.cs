﻿using Core.DbModels;
using Core.Models;
using Core.Repositories;
using Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class EmployeeResumeService : IEmployeeResumeService
    {
        private readonly IUpRepository _upRepository;

        public EmployeeResumeService(IUpRepository upRepository
           )
        {
            this._upRepository = upRepository;
        }

        public virtual MethodResult<EmployeeResume> Add(EmployeeResume model)
        {
            var result = new MethodResult<EmployeeResume>();

            if (!model.Id.HasValue)
            {
                model.Id = Guid.NewGuid();
                model.IsDeleted = false;
                model.CreatedDate = DateTime.UtcNow;
                model.UpdatedDate = model.CreatedDate;
                this._upRepository.EmployeeResumes.Add(model);
            }
            else
            {
                this._upRepository.EmployeeResumes.Attach(model);
            }

            this._upRepository.SaveChanges();
            result.Result = model;
            return result;
        }

    }
}
