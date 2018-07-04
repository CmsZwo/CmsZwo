namespace CmsZwo.Container.Tests
{
	public interface ICircleBService : IService
	{
		ICircleAService ICircleAService { get; }
		string Value { get; }
	}

	public class CircleBService : Injectable, ICircleBService
	{
		[Inject]
		public ICircleAService ICircleAService { get; set; }

		public string Value
			=> "B";
	}
}
