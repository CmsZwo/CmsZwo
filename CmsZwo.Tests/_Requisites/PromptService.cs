namespace CmsZwo.Container.Tests
{
	public interface IPromptService : IService
	{
		INestedService INestedService { get; }
		string Text { get; }
	}

	public interface IAliasService : IService
	{
	}

	public class PromptService : Injectable, IPromptService, IAliasService
	{
		[Inject]
		public INestedService INestedService { get; set; }

		public string Text
			=> "Hello, World!";
	}

	public class InjectedProvider : Injectable
	{
		[Inject]
		public IPromptService IPromptService { get; set; }
	}
}
