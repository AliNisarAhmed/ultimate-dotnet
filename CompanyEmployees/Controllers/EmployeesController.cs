using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using Entities.DTO;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
	[ApiController]
	[Route("/api/companies/{companyId}/employees")]
	public class EmployeesController : ControllerBase
	{
		private IRepositoryManager _respository;
		private ILoggerManager _logger;
		private IMapper _mapper;

		public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_respository = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetEmployeesForCompany(Guid companyId)
		{
			var company = _respository.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeesFromDb = _respository.Employee.GetEmployees(companyId, trackChanges: false);

				var employeeDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDb);

				return Ok(employeeDto);
			}
		}

		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = _respository.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeeDb = _respository.Employee.GetEmployee(companyId, id, trackChanges: false);

				if (employeeDb == null)
				{
					_logger.LogInfo($"Employee with id {companyId} does not exist in the database");
					return NotFound();
				}

				var employeeDto = _mapper.Map<EmployeeDTO>(employeeDb);

				return Ok(employeeDto);
			}
		}

		[HttpPost]
		public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
		{
			if (employee == null)
			{
				_logger.LogError("EmployeeForCreationDTO object sent from client is null");
				return BadRequest("EmployeeForCreationDTO object is null");
			}

			var company = _respository.Company.GetCompany(companyId, trackChanges: false);

			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}

			var employeeEntity = _mapper.Map<Employee>(employee);

			_respository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
			_respository.Save();

			var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);

			return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = _respository.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} does not exist in the database");
				return NotFound();
			}

			var employeeForCompany = _respository.Employee.GetEmployee(companyId, id, trackChanges: false);

			if (employeeForCompany == null)
			{
				_logger.LogInfo($"Employee with id: {id} does not exist in the database");
				return NotFound();
			}

			_respository.Employee.DeleteEmployee(employeeForCompany);
			_respository.Save();

			return NoContent();
		}
	}
}