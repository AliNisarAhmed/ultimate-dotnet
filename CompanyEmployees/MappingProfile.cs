using AutoMapper;
using Entities.DTO;
using Entities.Models;

namespace CompanyEmployees
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// ---- COMPANY ----

			CreateMap<Company, CompanyDTO>()
					.ForMember(c => c.FullAddress,
							opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

			CreateMap<CompanyForCreationDTO, Company>();

			CreateMap<CompanyForUpdateDTO, Company>();


			// ---- EMPLOYEE ----

			CreateMap<Employee, EmployeeDTO>();

			CreateMap<EmployeeForCreationDTO, Employee>();

			CreateMap<EmployeeForUpdateDTO, Employee>().ReverseMap();

		}

	}
}