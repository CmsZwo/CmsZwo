using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

using CmsZwo;
using CmsZwo.Demo;

namespace Demo.www.Pages
{
	public class IndexModel : PageModel
    {
		public IEnumerable<Project> Projects { get; set; }

        public async void OnGet()
        {
			var container = ContainerFactory.Shared;
			var IProjectRepository = container.Get<IProjectRepository>();

			Projects = await IProjectRepository.GetDynamic();
        }
    }
}
