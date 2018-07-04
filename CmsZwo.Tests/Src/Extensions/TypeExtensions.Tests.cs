using System.Linq;
using Xunit;

namespace CmsZwo.Extensions.Tests
{
	public class TypeTests
	{
		[Fact]
		public void HasInterface()
		{
			Assert.True(typeof(Item).HasInterface<IItem>());
		}

		[Fact]
		public void Inherits()
		{
			Assert.True(typeof(Car).Inherits<Item>());
		}

		[Fact]
		public void GetProperties_WithAttribute()
		{
			Assert.Equal(1, typeof(Item).GetProperties<Important>().CountSafe());
		}

		[Fact]
		public void HasAttribute()
		{
			Assert.True(typeof(Item).HasAttribute<Listed>());
		}

		[Fact]
		public void HasAttribute_Property()
		{
			Assert.True(typeof(Item).GetProperties<Important>().First().HasAttribute<Important>());
		}

		[Fact]
		public void HasDefaultConstructor()
		{
			Assert.True(typeof(Item).HasDefaultConstructor());
			Assert.False(typeof(Cart).HasDefaultConstructor());
		}

		[Fact]
		public void GetAttribute()
		{
			Assert.Equal("Sale", typeof(Item).GetAttribute<Listed>().Category);
		}
	}
}
