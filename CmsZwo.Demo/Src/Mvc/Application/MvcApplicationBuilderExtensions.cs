using Microsoft.AspNetCore.Builder;

namespace CmsZwo.Demo
{
	public static class MvcApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseDummyData(this IApplicationBuilder app)
		{
			var container = ContainerFactory.Shared;

			var IProjectRepository = container.Get<IProjectRepository>();

			var projectGotham = new Project
			{
				Name = "Gotham",
				Description = "The Batman"
			};

			IProjectRepository.SaveAsync(projectGotham).Wait();

			var projectNewYork = new Project
			{
				Name = "New York",
				Description = "Superman"
			};

			IProjectRepository.SaveAsync(projectNewYork).Wait();

			return app;
		}
	}
}