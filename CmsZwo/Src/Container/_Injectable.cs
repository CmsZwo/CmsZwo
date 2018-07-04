using CmsZwo.Container;

namespace CmsZwo
{
	public interface IInjectable
	{
		IContainer IContainer { get; set; }
	};

	public abstract class Injectable : IInjectable
	{
		private IContainer IContainer { get; set; }

		IContainer IInjectable.IContainer
		{
			get => IContainer;
			set => IContainer = value;
		}
	}
}
