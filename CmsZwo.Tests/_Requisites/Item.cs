using System;

namespace CmsZwo.Extensions.Tests
{
	public interface IItem
	{
		string Key { get; set; }
		int Value { get; set; }
	}

	public class Listed : Attribute
	{
		public Listed(string category)
		{
			Category = category;
		}

		public string Category { get; private set; }
	};

	public class Important : Attribute { };

	[Listed("Sale")]
	public class Item : IItem
	{
		public Item() { }

		public string Key { get; set; }

		[Important]
		public int Value { get; set; }
	}
}
