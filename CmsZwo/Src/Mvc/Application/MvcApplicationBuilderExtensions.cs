using Microsoft.AspNetCore.Builder;

namespace CmsZwo
{
	public static class MvcApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseContainer(this IApplicationBuilder app)
		{
			var container = ContainerFactory.Shared;
			container.Register("CmsZwo");
			return app;
		}
	}
}