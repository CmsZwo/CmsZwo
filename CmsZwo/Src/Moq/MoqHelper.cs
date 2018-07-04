using System;
using System.Linq;

using CmsZwo;

namespace Moq
{
	public static class MoqHelper
	{
		public static T CreateWithMocks<T>()
			where T : IInjectable, new()
		{
			var result = new T();

			var properties =
				result
					.GetType()
					.GetProperties()
					.Where(x => x.HasAttribute<Inject>());

			var mockType = typeof(Mock<>);

			foreach (var property in properties)
			{
				var genericMockType = mockType.MakeGenericType(new[] { property.PropertyType });

				var constructor = genericMockType.GetConstructor(new Type[] { });
				var service = constructor.Invoke(null);

				var objectProperty = genericMockType.GetProperty("Object", property.PropertyType);
				var serviceObject = objectProperty.GetValue(service);

				property.SetValue(result, serviceObject);
			}

			return result;
		}
	}
}
