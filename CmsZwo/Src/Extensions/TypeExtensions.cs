using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace CmsZwo
{
	public static class TypeExtensions
	{
		#region Inheritance

		public static bool HasInterface<T>(this Type instance)
			=> Inherits<T>(instance);

		public static bool Inherits<T>(this Type instance)
			=> typeof(T).IsAssignableFrom(instance);

		#endregion

		#region Properties

		public static IEnumerable<PropertyInfo> GetProperties<TAttribute>(this Type instance)
		{
			var attributeType = typeof(TAttribute);
			var result =
				instance
				.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
				.Where(x => Attribute.IsDefined(x, attributeType, true))
				.ToList();

			return result.Safe();
		}

		#endregion

		#region Interfaces

		public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
		{
			if (includeInherited || type.BaseType == null)
				return type.GetInterfaces();

			return
				type
					.GetInterfaces()
					.Except(
						type.BaseType.GetInterfaces()
					);
		}

		#endregion

		#region Attributes

		public static bool HasAttribute<T>(this Type instance, bool inherit = true)
			where T : Attribute
			=> Attribute.IsDefined(instance, typeof(T), inherit);

		public static bool HasAttribute<T>(this PropertyInfo instance, bool inherit = true)
			where T : Attribute
			=> Attribute.IsDefined(instance, typeof(T), inherit);

		public static bool HasDefaultConstructor(this Type instance)
			=> instance.GetConstructor(Type.EmptyTypes) != null;

		public static T GetAttribute<T>(this Type instance, bool inherit = true)
			where T : Attribute
		{
			var result = instance
				.GetCustomAttributes(typeof(T), inherit)
				.FirstOrDefault() as T;

			return result;
		}

		#endregion
	}
}