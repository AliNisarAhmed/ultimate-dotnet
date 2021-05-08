using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DTO;
using Entities.Models;
using Entities.RequestFeatures;
using LoggerService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
		public async Task<IActionResult> GetEmployeesForCompany(
			Guid companyId,
			[FromQuery] EmployeeParameters employeeParameters)
		{
			if (!employeeParameters.ValidAgeRange)
			{
				return BadRequest("Max age cannot be less than the min age.");
			}

			var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeesFromDb = await _repo.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);

				Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employeesFromDb.MetaData));

				var employeeDto = _mapper.Map<IEnumerable<EmployeeDTO>>(employeesFromDb);

				return Ok(employeeDto);
			}
		}

		[HttpGet("{id}", Name = "GetEmployeeForCompany")]
		public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
		{
			var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}
			else
			{
				var employeeDb = await _repo.Employee.GetEmployeeAsync(companyId, id, trackChanges: false);

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
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
		{
			var company = await _repo.Company.GetCompanyAsync(companyId, trackChanges: false);

			if (company == null)
			{
				_logger.LogInfo($"Company with id {companyId} does not exist in the database");
				return NotFound();
			}

			var employeeEntity = _mapper.Map<Employee>(employee);

			_repo.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
			await _repo.SaveAsync();

			var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);

			return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
		}

		[HttpDelete("{id}")]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
		{
			var employeeForCompany = HttpContext.Items["employee"] as Employee;

			if (employeeForCompany == null)
			{
				_logger.LogInfo($"Employee with id: {id} does not exist in the database");
				return NotFound();
			}

			_repo.Employee.DeleteEmployee(employeeForCompany);
			await _repo.SaveAsync();

			return NoContent();
		}

		[HttpPut("{id}")]
		[ServiceFilter(typeof(ValidationFilterAttribute))]
		[ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
		public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDTO employee)
		{
			var employeeEntity = HttpContext.Items["employee"] as Employee;

			if (employeeEntity == null)
			{
				_logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			_mapper.Map(employee, employeeEntity);

			await _repo.SaveAsync();

			return NoContent();
		}

		[HttpPatch("{id}")]
		public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc)
		{
			if (patchDoc == null)
			{
				_logger.LogError("patchDoc object sent from client is null");
				return BadRequest("patchDoc object is null");
			}

			var employeeEntity = HttpContext.Items["employee"] as Employee;
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

			await _repo.SaveAsync();

			return NoContent();
		}
	}
}