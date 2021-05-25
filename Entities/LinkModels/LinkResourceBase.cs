using System.Collections.Generic;

namespace Entities.LinkModels
{
	public class LinkResourceBase
	{
		public LinkResourceBase()
		{

		}

		public List<Link> Link { get; set; } = new List<Link>();
	}

}