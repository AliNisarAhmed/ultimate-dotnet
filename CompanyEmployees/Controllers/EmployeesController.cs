using System;
using System.Collections.Generic;
using AutoMapper;
using Contracts;
using Entities.DTO;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
	[ApiController]
	[Route("/api/companies/{companyId}/employees")]
	public class EmployeesController : ControllerBase
	{
		private IRepositoryManager _repo;
		private ILoggerManager _logger;
		private IMapper _mapper;

		public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repo = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetEmployeesForCompany(Guid companyId)
		{
			var company = _repo.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeesFromDb = _repo.Employee.GetEmployees(companyId, trackChanges: false);

				var employeeDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDb);

				return Ok(employeeDto);
			}
		}

		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = _repo.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeeDb = _repo.Employee.GetEmployee(companyId, id, trackChanges: false);

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

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the EmployeeForCreationDto object");
				return UnprocessableEntity(ModelState);
			}

			var company = _repo.Company.GetCompany(companyId, trackChanges: false);

			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}

			var employeeEntity = _mapper.Map<Employee>(employee);

			_repo.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
			_repo.Save();

			var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);

			return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = _repo.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} does not exist in the database");
				return NotFound();
			}

			var employeeForCompany = _repo.Employee.GetEmployee(companyId, id, trackChanges: false);

			if (employeeForCompany == null)
			{
				_logger.LogInfo($"Employee with id: {id} does not exist in the database");
				return NotFound();
			}

			_repo.Employee.DeleteEmployee(employeeForCompany);
			_repo.Save();

			return NoContent();
		}

		[HttpPut("{id}")]
		public IActionResult UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDTO employee)
		{
			if (employee == null)
			{
				_logger.LogError("EmployeeForUpdateDto object sent from client is null.");
				return BadRequest("EmployeeForUpdateDto object is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model for the EmployeeForUpdateDTO object");
				return UnprocessableEntity(ModelState);
			}

			var company = _repo.Company.GetCompany(companyId, trackChanges: false);

			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeEntity = _repo.Employee.GetEmployee(companyId, id, trackChanges: true);

			if (employeeEntity == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			_mapper.Map(employee, employeeEntity);

			_repo.Save();

			return NoContent();
		}

		[HttpPatch("{id}")]
		public IActionResult PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
		{
			if (patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null");
				return BadRequest("patchDoc object is null");
			}

			var company = _repo.Company.GetCompany(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
				return NotFound();
			}

			var employeeEntity = _repo.Employee.GetEmployee(companyId, id, trackChanges: true);
			if (employeeEntity == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity);

			patchDoc.ApplyTo(employeeToPatch, ModelState);

			TryValidateModel(employeeToPatch);

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the patch document");
				return UnprocessableEntity(ModelState);
			}

			_mapper.Map(employeeToPatch, employeeEntity);

			_repo.Save();

			return NoContent();
		}
	}
}