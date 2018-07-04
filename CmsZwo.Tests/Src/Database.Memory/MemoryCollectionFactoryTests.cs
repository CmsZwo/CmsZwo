using CmsZwo.Database.Memory;

using Xunit;

namespace CmsZwo.Repository.Tests
{
	public class MemoryCollectionFactoryTest
	{
		private class DummyEntity : Entity { }

		[Fact]
		public void GetQueryable()
		{
			var service = new MemoryCollectionFactory();

			var result1 = service.GetOrCreateCollection<string>();
			result1.Add("Foo");

			var result2 = service.GetOrCreateCollection<string>();
			result2.Add("Bar");

			Assert.Equal(result1.Count, result2.Count);
		}
	}
}
