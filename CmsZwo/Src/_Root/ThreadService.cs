using System;
using System.Threading;

namespace CmsZwo
{
	public interface IThreadService : IService
	{
		void Sleep(TimeSpan ts);
		void Sleep(int milliseconds);

		bool QueueUserWorkItem(WaitCallback callBack);
		bool QueueUserWorkItem(WaitCallback callBack, object state);
	}

	public class ThreadService : IThreadService
	{
		public void Sleep(TimeSpan ts)
			=> Thread.Sleep(ts);

		public void Sleep(int milliseconds)
			=> Thread.Sleep(milliseconds);

		public bool QueueUserWorkItem(WaitCallback callBack)
			=> ThreadPool.QueueUserWorkItem(callBack);

		public bool QueueUserWorkItem(WaitCallback callBack, object state)
			=> ThreadPool.QueueUserWorkItem(callBack, state);
	}
}
