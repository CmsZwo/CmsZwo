using System;

namespace CmsZwo
{
	public interface ITimeService : IService
	{
		DateTime Now { get; }
	}

	public class TimeService : ITimeService
	{
		public DateTime Now
			=> DateTime.Now;
	}
}
