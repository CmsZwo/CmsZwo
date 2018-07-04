using Xunit;

namespace CmsZwo.Extensions.Tests
{
	public class ObjectTests
	{
		[Fact]
		public void ToInt32()
		{
			Assert.Equal(4, "4".ToInt32());
			Assert.Equal(0, "Hello".ToInt32());
			Assert.Equal(1, "Hello".ToInt32(1));
		}

		[Fact]
		public void ToBoolean()
		{
			Assert.True("true".ToBoolean());
			Assert.True("1".ToBoolean());
			Assert.True("Hello".ToBoolean(true));
			Assert.False("Hello".ToBoolean());
		}

		[Fact]
		public void ToDouble()
		{
			Assert.Equal(4.2, "4,2".ToDouble());
			Assert.Equal(0, "Hello".ToDouble());
			Assert.Equal(1.2, "Hello".ToDouble(1.2));
		}

		[Fact]
		public void ToEnum()
		{
			Assert.Equal(TypeEnum.Bike, "Bike".ToEnum<TypeEnum>());
			Assert.Equal(TypeEnum.Car, "Hello".ToEnum<TypeEnum>(TypeEnum.Car));
		}

		[Fact]
		public void ToJson()
		{
			Assert.Equal("{\"Key\":\"Id\",\"Value\":1}", new Item { Key = "Id", Value = 1 }.ToJson());
		}

		[Fact]
		public void ToJson_NumberFormat()
		{
			var json = new Car { Price = 100_000.99 }.ToJson();
			Assert.Contains("\"Price\":100000.99", json);
		}

		[Fact]
		public void ToJsonWithTypeInformation()
		{
			Assert.Equal($"{{\"$type\":\"{typeof(Item).FullName}, {typeof(Item).Namespace}\",\"Key\":\"Id\",\"Value\":1}}", new Item { Key = "Id", Value = 1 }.ToJsonWithTypeInformation());
		}

		[Fact]
		public void ToJsonMd5()
		{
			Assert.Equal("b31e5691695000c1a2eb4a2a93a21b41", new Item { Key = "Id", Value = 1 }.ToJsonMd5());
		}

		[Fact]
		public void ToObjectByJson()
		{
			var json = "{Make:\"BMW\",Price:\"\"}";
			var car = json.ToObjectByJson<Car>();
			Assert.Equal("BMW", car.Make);
		}

		[Fact]
		public void CopyByJson()
		{
			var car = new Car { Make = "BMW" };
			var copy = car.CopyByJson();
			Assert.NotEqual(car, copy);
			Assert.Equal(car.Make, copy.Make);
		}

		[Fact]
		public void EqualsByJson()
		{
			var car = new Car { Make = "BMW", Price = 100_000 };
			var copy = new Car { Make = "BMW", Price = 100_000 };
			Assert.NotEqual(car, copy);
			Assert.True(car.EqualsByJson(copy));
		}
	}
}
