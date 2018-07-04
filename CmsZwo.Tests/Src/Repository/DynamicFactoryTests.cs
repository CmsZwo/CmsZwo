using Moq;
using Xunit;

namespace CmsZwo.Repository.Tests
{
	public class DynamicFactoryTests
	{
		public class DummyEntity : Entity { }

		[Fact]
		public void Create_Should_Return_List()
		{
			var service = MoqHelper.CreateWithMocks<DynamicCollectionFactory>();

			var verify = service.Create<DummyEntity>(new[] { "id" });
			Assert.NotNull(verify);
		}
	}
}
