using System;

namespace CmsZwo
{
	public class CacheEvent
	{
		public string Name { get; private set; }

		public static implicit operator CacheEvent(string instance)
			=> instance.HasContent() ? new CacheEvent { Name = instance } : null;

		public static implicit operator CacheEvent(Type instance)
			=> instance != null ? new CacheEvent { Name = $"__{instance.Name}" } : null;
	}
}
