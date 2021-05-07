using System;
using System.Threading.Tasks;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
	public class ValidateCompanyExistsAttribute : IAsyncActionFilter
	{
		private readonly IRepositoryManager _repo;
		private readonly ILoggerManager _logger;

		public ValidateCompanyExistsAttribute(IRepositoryManager repo, ILoggerManager logger)
		{
			_repo = repo;
			_logger = logger;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
			var id = (Guid)context.ActionArguments["id"];
			var company = await _repo.Company.GetCompanyAsync(id, trackChanges);

			if (company == null)
			{
				_logger.LogInfo($"Company with id: {id} does not exist in the database.");
				context.Result = new NotFoundResult();
			}
			else
			{
				context.HttpContext.Items.Add("company", company);
				await next();
			}
		}
	}
}