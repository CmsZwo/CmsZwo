using System;
using System.Linq;
using System.Globalization;

using Xunit;

namespace CmsZwo.Extensions.Tests
{
	public class StringTests
	{
		[Fact]
		public void HasContent()
		{
			Assert.True("Hello".HasContent());
			Assert.False("".HasContent());
		}

		[Fact]
		public void Base64ToByteArray()
		{
			Assert.Equal(500, "SGVsbG8=".Base64ToByteArray().Sum(x => x));
		}

		[Fact]
		public void IsAlphaNumeric()
		{
			Assert.True("Hello1".IsAlphaNumeric());
			Assert.False("Hello, World!".IsAlphaNumeric());
		}

		[Fact]
		public void IsInt()
		{
			Assert.True("123".IsAlphaNumeric());
			Assert.False("Hello, World!".IsAlphaNumeric());
		}

		[Fact]
		public void IsNumeric()
		{
			var numberGroupSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
			var numberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

			Assert.True("123".IsNumeric());
			Assert.True($"123{numberDecimalSeparator}45".IsNumeric());
			Assert.True($"123{numberGroupSeparator}45".IsNumeric());
			Assert.True($"123{numberGroupSeparator}45{numberDecimalSeparator}67".IsNumeric());

			Assert.False("abc".IsNumeric());
			Assert.False($"123{numberDecimalSeparator}45{numberDecimalSeparator}67".IsNumeric());
			Assert.False($"123{numberDecimalSeparator}45{numberGroupSeparator}67".IsNumeric());
		}

		[Fact]
		public void IsValidEMail()
		{
			Assert.True("max.muster@test.com".IsValidEMail());
			Assert.False("Hello, World!".IsValidEMail());
		}

		[Fact]
		public void IsIban()
		{
			Assert.True("DE27100777770209299700".IsIban());
			Assert.False("DE27100777770209299701".IsIban());
			Assert.False("Hello, World".IsIban());
			Assert.False("1231231231231231231231".IsIban());
		}

		[Fact]
		public void LengthSafe()
		{
			Assert.Equal("He...", "Hello, World!".LengthSafe(5));
			Assert.Equal("HeXXX", "Hello, World!".LengthSafe(5, "XXX"));
		}

		[Fact]
		public void FillLeft()
		{
			Assert.Equal("XX123", "123".FillLeft("X", 5));
		}

		[Fact]
		public void FilterAlpha()
		{
			Assert.Equal("XXX", "X1X2X3".FilterAlpha());
		}

		[Fact]
		public void FilterAlphaNumeric()
		{
			Assert.Equal("X1X2X3", "X1!X2?X3".FilterAlphaNumeric());
		}

		[Fact]
		public void FilterUrlAllowed()
		{
			Assert.Equal("Hello-World-This-Is-a-test", "Hello-World/This.Is a test".FilterUrlAllowed());
		}

		[Fact]
		public void EqualsSafe()
		{
			Assert.True("123".EqualsSafe("123"));
			Assert.False("123".EqualsSafe(null));
			Assert.False((null as string).EqualsSafe("Hello"));
			Assert.False("123".EqualsSafe("Hello"));
		}

		[Fact]
		public void EqualsIgnoreCase()
		{
			Assert.True("Hello, World".EqualsIgnoreCase("hello, WORLD"));
		}

		[Fact]
		public void ContainsIgnoreCase()
		{
			Assert.True("Hello, World".ContainsIgnoreCase("HeLLO"));
		}

		[Fact]
		public void ContainsAnyIgnoreCase()
		{
			Assert.True("Hello, World".ContainsAnyIgnoreCase(new[] { "WORLD", "XXX" }));
		}

		[Fact]
		public void Contains()
		{
			Assert.Contains("Hello", "Hello, World");
		}

		[Fact]
		public void StartsWithIgnoreCase()
		{
			Assert.True("Hello, World".StartsWithIgnoreCase("helLO"));
		}

		[Fact]
		public void EndsWithIgnoreCase()
		{
			Assert.True("Hello, World".EndsWithIgnoreCase("WOrld"));
		}

		[Fact]
		public void IndexOfWithIgnoreCase()
		{
			Assert.Equal(7, "Hello, World".IndexOfWithIgnoreCase("WO"));
		}

		[Fact]
		public void NthIndexOfWithIgnoreCase()
		{
			Assert.Equal(8, "abc abc abc abc".NthIndexOfWithIgnoreCase(3, "ABC"));
		}

		[Fact]
		public void ReplaceAll()
		{
			Assert.Equal("xx1 xx2 cc3 dd4", "aa1 bb2 cc3 dd4".ReplaceAll(new[] { "a", "b" }, "x"));
		}

		[Fact]
		public void ToDateTime()
		{
			var format = "dd.MM.yyyy HH:mm:ss";
			var now = DateTime.Now;
			var str = now.ToString(format);
			var dt = str.ToDateTime().Value;

			Assert.Equal(now.ToString(format), dt.ToString(format));
		}

		[Fact]
		public void TryParseDate()
		{
			var now = DateTime.Now;

			void validate(string source, DateTime target)
			{
				var dt = source.TryParseDate();
				Assert.NotNull(dt);
				Assert.Equal(target.Year, dt.Value.Year);
				Assert.Equal(target.Month, dt.Value.Month);
				Assert.Equal(target.Day, dt.Value.Day);
				Assert.Equal(0, dt.Value.Hour);
				Assert.Equal(0, dt.Value.Minute);
				Assert.Equal(0, dt.Value.Second);
			}

			validate(now.ToString("dd.MM"), now);
			validate(now.ToString("dd.MM."), now);
			validate(now.ToString("dd.MM.yyyy"), now);
			validate(now.ToString("yyyy-MM-dd"), now);
			validate(now.ToString("MM-dd"), now);
		}

		[Fact]
		public void ToTimeSpan()
		{
			var ts = "20:15:07".ToTimeSpan();
			Assert.Equal(20, ts.Hours);
			Assert.Equal(15, ts.Minutes);
			Assert.Equal(7, ts.Seconds);
		}

		[Fact]
		public void Reverse()
		{
			Assert.Equal("abc", "cba".Reverse());
		}

		[Fact]
		public void SubstringSafe()
		{
			Assert.Equal("World!", "Hello, World!".SubstringSafe(7, 10));
		}

		[Fact]
		public void Left_Length()
		{
			Assert.Equal("Hello", "Hello, World!".Left(5));
		}

		[Fact]
		public void Left_Until()
		{
			Assert.Equal("Hello", "Hello, World!".Left(","));
		}

		[Fact]
		public void Right()
		{
			Assert.Equal("World!", "Hello, World!".Right(6));
		}

		[Fact]
		public void ToXml()
		{
			var item = new Item { Key = "Hello", Value = 100 };
			Assert.Equal(
				"<?xml version=\"1.0\" encoding=\"utf-8\"?><Item><Key>Hello</Key><Value>100</Value></Item>".Replace(" ", ""),
				item.ToXml().ReplaceAll(new[] { "\r", "\n", " " }, "")
			);
		}

		[Fact]
		public void ToObjectByXml()
		{
			var item = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Item><Key>Hello</Key><Value>100</Value></Item>".ToObjectByXml<Item>();
			Assert.Equal("Hello", item.Key);
			Assert.Equal(100, item.Value);
		}

		[Fact]
		public void SplitSafe_Char()
		{
			var token = "Hello, World!".SplitSafe(',');
			Assert.Equal("Hello", token[0]);
			Assert.Equal("World!", token[1]);
		}

		[Fact]
		public void SplitSafe_String()
		{
			var token = "Hello, World!".SplitSafe(",");
			Assert.Equal("Hello", token[0]);
			Assert.Equal("World!", token[1]);
		}

		[Fact]
		public void SplitSafe_Separators()
		{
			var token = "Hello, World! - Test".SplitSafe(new[] { ",", "-" });
			Assert.Equal("Hello", token[0]);
			Assert.Equal("World!", token[1]);
			Assert.Equal("Test", token[2]);
		}
	}
}
