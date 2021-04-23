using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities.DTO;
using LoggerService;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Controllers
{
	[ApiController]
	[Route("api/companies")]
	public class CompaniesController : ControllerBase
	{

		private ILoggerManager _logger;
		private IRepositoryManager _repository;
		private IMapper _mapper;

		public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetCompanies()
		{
			try
			{
				var companies = _repository.Company.GetAllCompanies(trackChanges: false);

				var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

				return Ok(companiesDto);
			}
			catch (Exception e)
			{
				_logger.LogError($"Something went wrong in the {nameof(GetCompanies)} action {e}");
				return StatusCode(500, "Internal Server Error");
			}
		}

	}
}
