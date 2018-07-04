using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace CmsZwo.Container
{
	public interface IInjector
	{
		void Inject<T>(T service)
			where T : IInjectable;

		T Create<T>()
			where T : IInjectable, new();
	}

	public class Injector : IInjector
	{
		#region Construct

		private readonly IContainer _IRegistry;

		public Injector(IContainer registry)
		{
			_IRegistry = registry;
		}

		#endregion

		#region Tools

		private bool IsInjected<T>(T service)
			where T : IInjectable
			=> service.IContainer != null;

		private Dictionary<Type, List<PropertyInfo>> _PropertyCache
			= new Dictionary<Type, List<PropertyInfo>>();

		private List<PropertyInfo> GetInjectProperties<T>(T service)
		{
			var type = service.GetType();

			if (!_PropertyCache.ContainsKey(type))
			{
				var properties =
					type
						.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						.Where(x => x.GetCustomAttribute<Inject>() != null)
						.ToList();

				_PropertyCache[type] = properties;
			}

			return _PropertyCache[type];
		}

		#endregion

		#region IInjector

		public void Inject<T>(T service)
			where T : IInjectable
		{
			if (IsInjected(service))
				return;

			service.IContainer = _IRegistry;

			var properties = GetInjectProperties(service);
			foreach (var p in properties)
			{
				var injectService = _IRegistry.Get(p.PropertyType);
				p.SetValue(service, injectService);
			}
		}

		public T Create<T>()
			where T : IInjectable, new()
		{
			var result = new T();
			Inject(result);
			return result;
		}

		#endregion
	}
}
