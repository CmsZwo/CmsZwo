using System.Linq;
using Xunit;

namespace CmsZwo.Extensions.Tests
{
	public class IEnumerableTests
	{
		[Fact]
		public void AverageSafe()
		{
			var items = new[] { new Item { Value = 10 }, new Item { Value = 20 } };
			Assert.Equal(15, items.AverageSafe(x => x.Value));
			Assert.Equal(0, new Item[] { }.AverageSafe(x => x.Value));
		}

		[Fact]
		public void HasContent()
		{
			var items = new[] { new Item() };
			Assert.True(items.HasContent());
			Assert.False(new Item[] { }.HasContent());
		}

		[Fact]
		public void CountSafe_String()
		{
			var items = new[] { "Hello", "", "World" };
			Assert.Equal("Hello;World", items.JoinSafe(";"));
			Assert.Equal("Hello;;World", items.JoinSafe(";", false));
		}

		[Fact]
		public void JoinSafe_Int()
		{
			var items = new[] { 1, 2, 3 };
			Assert.Equal("1, 2, 3", items.JoinSafe());
		}

		[Fact]
		public void ContainsIgnoreCase()
		{
			var items = new[] { "Hello" };
			Assert.True(items.ContainsIgnoreCase("HELLO"));
			Assert.False(items.ContainsIgnoreCase("World"));
		}

		[Fact]
		public void FirstWithContent()
		{
			var items = new[] { "", null, "Hello" };
			Assert.Equal("Hello", items.FirstWithContent());
		}

		[Fact]
		public void TakeLast()
		{
			var items = new[] { "Hello", "World", "!" };
			Assert.Equal("!", items.TakeLast(1).Last());
			Assert.Equal("World", items.TakeLast(2).First());
			Assert.Equal("Hello", items.TakeLast(10).First());
		}

		[Fact]
		public void PickRandom()
		{
			var items = new[] { "Hello", "World", "!" };
			var pick = items.PickRandom();
			Assert.Contains(pick, items);
		}

		[Fact]
		public void IndexOf()
		{
			var items = new[] { "Hello", "World", "!" };
			Assert.Equal(1, items.IndexOf(x => x == "World"));
			Assert.Equal(-1, items.IndexOf(x => x == "404"));
		}

		[Fact]
		public void Copy()
		{
			var items = new[] { "Hello", "World", "!" };
			var copy = items.Copy(x => x.Length > 1);
			Assert.Equal(2, copy.Count);
		}

		[Fact]
		public void Distinct()
		{
			var items = new[] { "Hello", "Hello", "World", "World" };
			Assert.Equal(2, items.Distinct().Count());
		}

		[Fact]
		public void WhereHasContent()
		{
			var items = new[] { "Hello", null, "", "World" };
			Assert.Equal(2, items.WhereHasContent().Count());
		}

		[Fact]
		public void Safe()
		{
			var items = new[] { "Hello", "World" };
			Assert.True(items.Safe() != null);

			var nullItems = null as string[];
			Assert.True(items.Safe() != null);
		}

		[Fact]
		public void SequenceEqualSafe()
		{
			var items = new[] { "Hello", "World" };
			var compare = new[] { "Hello", "World" };
			Assert.True(items.SequenceEqualSafe(compare));
			Assert.False(items.SequenceEqualSafe(null));
		}
	}
}
