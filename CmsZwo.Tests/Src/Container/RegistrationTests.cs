using Xunit;

using CmsZwo.Database.Memory;

namespace CmsZwo.Repository.Tests
{
	public class RegistrationTests
	{
		[Fact]
		public void Register_Should_Get_Services()
		{
			var container = new Container.Container();

			var service = new AutoIdRepository();
			container.Register(service);

			Assert.NotNull(container.Get<IAutoIdRepository>());
		}
	}
}
