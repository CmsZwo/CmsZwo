using ServiceStack.Redis;

using Xunit;
using Moq;

namespace CmsZwo.Cache.Tests
{
	public class RedisClientFactoryTests
	{
		[Fact]
		public void CreateClient_Should_Try_To_Connect_To_Host()
		{
			var ISettingsService = new Mock<ISettingsService>();
			ISettingsService
				.Setup(x => x.Get<CacheServiceSettings>())
				.Returns(new CacheServiceSettings
				{
					Redis = new CacheServiceSettings.RedisSettings
					{
						Enabled = true,
						ConnectionString = "localhost"
					}
				});


			var service = new RedisClientFactory
			{
				ISettingsService = ISettingsService.Object
			};

			Assert.Throws<RedisException>(() =>
			{
				using (var client = service.GetClient())
				{
				}
			});
		}
	}
}
