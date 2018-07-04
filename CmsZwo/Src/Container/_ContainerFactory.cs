using CmsZwo.Container;

namespace CmsZwo
{
	public class ContainerFactory
	{
		#region Construct

		private static IContainer _IContainer;

		public static IContainer Shared
		{
			get
			{
				if (_IContainer == null)
					_IContainer = new Container.Container();

				return _IContainer;
			}
		}

		#endregion
	}
}
