namespace CmsZwo.Container.Tests
{
	public interface ICircleAService : IService
	{
		ICircleBService ICircleBService { get; }
		string Value { get; }
	}

	public class CircleAService : Injectable, ICircleAService
	{
		[Inject]
		public ICircleBService ICircleBService { get; set; }

		public string Value
			=> "A";
	}
}
