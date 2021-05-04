using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.DTO
{
	public abstract class CompanyForManipulationDTO
	{
		[Required(ErrorMessage = "Company name is a required field")]
		[MaxLength(30, ErrorMessage = "Maximum length for name is 30 characters")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Company address is a required field.")]
		public string Address { get; set; }

		[Required(ErrorMessage = "Country is a required field.")]
		public string Country { get; set; }

		public IEnumerable<EmployeeForCreationDTO> Employees { get; set; }
	}
}