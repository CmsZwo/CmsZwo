using Xunit;

namespace CmsZwo.Container.Tests
{
	public class SettingsServiceTests
	{
		[Fact]
		public void Set_Get()
		{
			var settings = new TestSettings { Enabled = true };

			var service = new SettingsService();
			service.Set(settings);

			var _settings = service.Get<TestSettings>();
			Assert.True(_settings.Enabled);
		}
	}
}
