using Moq;
using Xunit;

namespace CmsZwo.Testing.Tests
{
	public class MoqHelperTests
	{
		[Fact]
		public void CreateWithMocks()
		{
			var provider = MoqHelper.CreateWithMocks<TestProvider>();
			Assert.NotNull(provider.ITestService);
		}

		public class ITestService : IService { }

		public class TestProvider : Injectable
		{
			[Inject]
			public ITestService ITestService { get; set; }
		}
	}
}
