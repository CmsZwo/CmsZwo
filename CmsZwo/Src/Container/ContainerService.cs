namespace CmsZwo
{
	public interface IContainerService : IService
	{
		T Get<T>()
			where T : IService;
	}

	public class ContainerService : Injectable, IContainerService
	{
		public T Get<T>()
			where T : IService
			=> (this as IInjectable).IContainer.Get<T>();
	}
}
