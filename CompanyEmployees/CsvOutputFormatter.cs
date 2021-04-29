using Microsoft.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using Entities.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CompanyEmployees
{
	public class CsvOutputFormatter : TextOutputFormatter
	{
		public CsvOutputFormatter()
		{
			SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
			SupportedEncodings.Add(Encoding.UTF8);
			SupportedEncodings.Add(Encoding.Unicode);
		}

		protected override bool CanWriteType(Type type)
		{
			if (typeof(CompanyDTO).IsAssignableFrom(type) ||
						typeof(IEnumerable<CompanyDTO>).IsAssignableFrom(type))
			{
				return base.CanWriteType(type);
			}

			return false;
		}

		public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
		{
			var response = context.HttpContext.Response;
			var buffer = new StringBuilder();

			if (context.Object is IEnumerable<CompanyDTO>)
			{
				foreach (var company in (IEnumerable<CompanyDTO>)context.Object)
				{
					FormatCsv(buffer, company);
				}
			}
			else
			{
				FormatCsv(buffer, (CompanyDTO)context.Object);
			}

			await response.WriteAsync(buffer.ToString());
		}

		public static void FormatCsv(StringBuilder buffer, CompanyDTO company)
		{
			buffer.AppendLine($"{company.Id},\"{company.Name},\"{company.FullAddress}\"");
		}
	}
}