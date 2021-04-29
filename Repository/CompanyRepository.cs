using System.Collections.Generic;
using Contracts;
using Entities;
using Entities.Models;
using System.Linq;
using System;

namespace Repository
{
	public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
	{
		public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
		{

		}

		public IEnumerable<Company> GetAllCompanies(bool trackChanges) =>
			FindAll(trackChanges)
			.OrderBy(c => c.Name)
			.ToList();


		public Company GetCompany(Guid companyId, bool trackChanges) =>
			FindByCondition(c => c.Id.Equals(companyId), trackChanges)
			.SingleOrDefault();

	}
}