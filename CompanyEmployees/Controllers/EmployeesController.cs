using System;
using AutoMapper;
using Contracts;
using Entities.DTO;
using LoggerService;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
	[ApiController]
	[Route("/api/{companyId}/employees")]
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

				var employeeDto = _mapper.Map<EmployeeDTO>(employeesFromDb);

				return Ok(employeeDto);
			}
		}

		[HttpGet("{id}")]
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
	}
}