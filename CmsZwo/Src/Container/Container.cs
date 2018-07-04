using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace CmsZwo.Container
{
	public interface IContainer
	{
		void Register(Assembly assembly);
		void Register(IService service);
		void Register(string assemblyContainsIgnoreCase);

		void Register<T>()
			where T : class, IService, new();

		T Get<T>()
			where T : IService;

		object Get(Type type);

		T Create<T>()
			where T : IInjectable, new();
	}

	public class Container : IContainer
	{
		#region Contruct

		private readonly IInjector _IInjector;

		public Container()
		{
			_IInjector = new Injector(this);
			Register("CmsZwo.");
		}

		#endregion

		#region Tools

		private readonly Dictionary<Type, IService> _Services
			= new Dictionary<Type, IService>();

		private static Type[] _NullDefaultTypes = new[] {
			typeof(string)
		};

		private static object Create(Type type)
		{
			if (type.IsInterface) return null;
			if (_NullDefaultTypes.Contains(type)) return null;

			var result = Activator.CreateInstance(type);
			return result;
		}

		private IEnumerable<Type> GetServiceInterfaces(Type type)
		{
			var result =
				type
					.GetInterfaces()
					.Where(
						x =>
							x != typeof(IService)
							&& !typeof(IAbstractService).IsAssignableFrom(x)
					);

			return result;
		}

		private static bool HasDefaultConstructor(Type type)
			=> type.GetConstructor(Type.EmptyTypes) != null;

		private bool IsRegistered(Type type)
			=> _Services.ContainsKey(type);

		private void Register(object service)
		{
			if (!(service is IService serviceInterface))
				throw new ArgumentException($"[{service.GetType().Name}] must implement {nameof(IService)}.");

			Register(service as IService);
		}

		#endregion

		#region IContainer

		public void Register(string assemblyContainsIgnoreCase)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				if (!assembly.FullName.ContainsIgnoreCase(assemblyContainsIgnoreCase))
					continue;

				Register(assembly);
			}
		}

		public void Register(IService service)
		{
			var interfaces = GetServiceInterfaces(service.GetType());

			foreach (var interfaceType in interfaces)
				_Services[interfaceType] = service;
		}

		public void Register(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentException($"[{nameof(assembly)}] must not be null.");

			var serviceType = typeof(IService);

			var serviceTypes =
				assembly
					.ExportedTypes
					.Where(
						x =>
						x.IsClass
						&& !x.IsAbstract
						&& serviceType.IsAssignableFrom(x)
					)
					.ToList();

			foreach (var type in serviceTypes)
			{
				if (typeof(IAbstractService).IsAssignableFrom(type))
					continue;

				if (!HasDefaultConstructor(type)) continue;
				if (IsRegistered(type)) continue;

				var service = Create(type) as IService;
				Register(service);
			}
		}

		public T Get<T>()
			where T : IService
			=> (T)Get(typeof(T));

		public object Get(Type type)
		{
			if (!_Services.ContainsKey(type))
				return null;

			var result = _Services[type];

			if (result == null)
				throw new Exception($"Type [{type.Name}] not registered.");

			if (result is IInjectable injectable)
				_IInjector.Inject(injectable);

			return result;
		}

		public void Register<T>()
			where T : class, IService, new()
		{
			var type = typeof(T);
			var interfaces = GetServiceInterfaces(type);

			foreach (var interfaceType in interfaces)
				_Services[interfaceType] = new T();
		}

		public T Create<T>()
			where T : IInjectable, new()
			=> _IInjector.Create<T>();

		#endregion
	}
}
