using System;
using System.Threading.Tasks;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
	public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
	{
		private readonly IRepositoryManager _repo;
		private readonly ILoggerManager _logger;

		public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repo)
		{
			_repo = repo;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var method = context.HttpContext.Request.Method;
			var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

			var companyId = (Guid)context.ActionArguments["companyId"];
			var company = await _repo.Company.GetCompanyAsync(companyId, true);

			if (company == null)
			{
				_logger.LogInfo($"Company with id: {companyId} does not exist in the database.");
				context.Result = new NotFoundResult();
				return;
			}

			var id = (Guid)context.ActionArguments["id"];
			var employee = await _repo.Employee.GetEmployeeAsync(companyId, id, trackChanges);

			if (employee == null)
			{
				_logger.LogInfo($"Employee with id: {id} does not exist in the database");
				context.Result = new NotFoundResult();
			}
			else
			{
				context.HttpContext.Items.Add("employee", employee);
				await next();
			}
		}
	}
}