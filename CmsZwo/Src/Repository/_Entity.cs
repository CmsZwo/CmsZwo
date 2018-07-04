using System;

namespace CmsZwo
{
	public interface IEntity
	{
		DateTime? Created { get; set; }
		ulong AutoId { get; set; }
		string Id { get; set; }
		string Name { get; set; }
	}

	public abstract class Entity : IEntity
	{
		#region IEntity

		public DateTime? Created { get; set; }
		public string Id { get; set; }
		public ulong AutoId { get; set; }

		public string Name { get;  set; }

		#endregion
	}
}
