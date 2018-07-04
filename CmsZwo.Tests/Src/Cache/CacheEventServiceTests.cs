using System.Threading.Tasks;
using System.Collections.Generic;

using Xunit;
using Moq;

namespace CmsZwo.Cache.Tests
{
	public class CacheEventServiceTests
	{
		[Fact]
		public async void Register_Should_Add_Key_To_Event()
		{
			var service = MoqHelper.CreateWithMocks<CacheEventService>();
			var ICacheService = Mock.Get(service.ICacheService);

			await service.Register("*", "id");
			ICacheService.Verify(x => x.AddToSetAsync(It.Is<string>(y => y.Contains("*")), "id"), Times.Once);
		}

		[Fact]
		public async void Trigger_Should_Remove_Keys_Of_Event()
		{
			var service = MoqHelper.CreateWithMocks<CacheEventService>();
			var ICacheService = Mock.Get(service.ICacheService);

			var keys = new HashSet<string> { "id1", "id2" };
			ICacheService
				.SetupIgnoreArgs(x => x.GetSetAsync(null, null))
				.Returns(Task.FromResult(keys));

			await service.Trigger("*");
			ICacheService.Verify(x => x.RemoveAsync(keys), Times.Once);
		}
	}
}
