using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DTO;
using Entities.Models;
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
			var companies = _repository.Company.GetAllCompanies(trackChanges: false);

			var companiesDto = _mapper.Map<IEnumerable<CompanyDTO>>(companies);

			return Ok(companiesDto);
		}

		[HttpGet("{id}", Name = "CompanyById")]
		public IActionResult GetCompany(Guid id)
		{
			var company = _repository.Company.GetCompany(id, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {id} does not exist in the database");
				return NotFound();
			}
			else
			{
				var companyDto = _mapper.Map<CompanyDTO>(company);
				return Ok(companyDto);
			}
		}

		[HttpGet("collection/({ids})", Name = "CompanyCollection")]
		public IActionResult GetCompanyCollection(IEnumerable<Guid> ids)
		{
			if (ids == null)
			{
				_logger.LogError("Parameter id is null");
				return BadRequest("Parameter id is null");
			}

			var companyEntities = _repository.Company.GetByIds(ids, trackChanges: false);

			if (ids.Count() != companyEntities.Count())
			{
				_logger.LogError("Some ids are not valid in collection");
				return NotFound();
			}

			var companiesToReturn = _mapper.Map<IEnumerable<CompanyDTO>>(companyEntities);
			return Ok(companiesToReturn);
		}

		[HttpPost]
		public IActionResult CreateCompany([FromBody] CompanyForCreationDTO company)
		{
			if (company == null)
			{
				_logger.LogError("CompanyForCreationDTO object sent from client is null");
				return BadRequest("CompanyForCreationDTO is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the CompanyForCreationDTO object");
				return UnprocessableEntity(ModelState);
			}

			var companyEntity = _mapper.Map<Company>(company);

			_repository.Company.CreateCompany(companyEntity);
			_repository.Save();

			var companyToReturn = _mapper.Map<CompanyDTO>(companyEntity);

			return CreatedAtRoute("CompanyById", new { id = companyToReturn.Id }, companyToReturn);
		}

		[HttpPost("collection")]
		public IActionResult CreateCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<CompanyForCreationDTO> companyCollection)
		{
			if (companyCollection == null)
			{
				_logger.LogError("Company collection sent from client is null.");
				return BadRequest("Company collection is null");
			}

			var companyEntities = _mapper.Map<IEnumerable<Company>>(companyCollection);
			foreach (var company in companyEntities)
			{
				_repository.Company.CreateCompany(company);
			}

			_repository.Save();

			var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDTO>>(companyEntities);

			var ids = string.Join(",", companyCollectionToReturn.Select(c => c.Id));

			return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
		}

		[HttpDelete("{id}")]
		public IActionResult DeleteCompany(Guid id)
		{
			var company = _repository.Company.GetCompany(id, trackChanges: false);
			if (company == null)
			{
				_logger.LogInfo($"Company with id: {id} does not exist in the database");
				return NotFound();
			}

			_repository.Company.DeleteCompany(company);
			_repository.Save();

			return NoContent();
		}

		[HttpPut("{id}")]
		public IActionResult UpdateCompany(Guid id, [FromBody] CompanyForUpdateDTO company)
		{
			if (company == null)
			{
				_logger.LogError("CompanyForUpdateDto object sent from client is null.");
				return BadRequest("CompanyForUpdateDto object is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogError("Invalid model state for the CompanyForUpdateDTO object");
				return UnprocessableEntity(ModelState);
			}

			var companyEntity = _repository.Company.GetCompany(id, trackChanges: true);

			if (companyEntity == null)
			{
				_logger.LogInfo($"Company with id: {id} doesn't exist in the database.");
				return NotFound();
			}

			// mapping to a tacked entity updates its fields, as well as add any Employees embedded within CompanyForUpdateDTO

			_mapper.Map(company, companyEntity);

			_repository.Save();

			return NoContent();
		}

	}
}
