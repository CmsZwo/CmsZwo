using Xunit;
using System.Reflection;

namespace CmsZwo.Container.Tests
{
	public class ContainerTests
	{
		[Fact]
		public void Register_Assembly()
		{
			var container = new Container();

			var assembly = Assembly.GetExecutingAssembly();
			container.Register(assembly);

			var IPromptService = container.Get<IPromptService>();
			Assert.True(IPromptService is PromptService);
		}

		[Fact]
		public void Register_Assembly_Pattern()
		{
			var container = new Container();

			var assembly = Assembly.GetExecutingAssembly();
			container.Register("CmsZwo.");

			var IPromptService = container.Get<IPromptService>();
			Assert.True(IPromptService is PromptService);
		}

		[Fact]
		public void Register_Get()
		{
			var container = new Container();
			container.Register<PromptService>();

			var IPromptService = container.Get<IPromptService>();
			Assert.True(IPromptService is PromptService);

			var IAliasService = container.Get<IAliasService>();
			Assert.True(IAliasService is PromptService);

			var IUnregisteredService = container.Get<IUnregisteredService>();
			Assert.Null(IUnregisteredService);
		}

		[Fact]
		public void Register_Get_By_Type()
		{
			var container = new Container();
			container.Register<PromptService>();

			var IPromptService = container.Get(typeof(IPromptService));
			Assert.True(IPromptService is PromptService);
		}

		[Fact]
		public void Register_Nested()
		{
			var container = new Container();
			container.Register<PromptService>();
			container.Register<NestedService>();

			var InjectedProvider = container.Create<InjectedProvider>();
			Assert.True(InjectedProvider.IPromptService.INestedService is INestedService);
		}

		[Fact]
		public void Register_Circle()
		{
			var container = new Container();
			container.Register<CircleAService>();
			container.Register<CircleBService>();

			var serviceA = container.Create<CircleAService>();
			Assert.Equal("A", serviceA.Value);
			Assert.Equal("B", serviceA.ICircleBService.Value);
			Assert.Equal("A", serviceA.ICircleBService.ICircleAService.Value);
			Assert.Equal("B", serviceA.ICircleBService.ICircleAService.ICircleBService.Value);
		}

		[Fact]
		public void Register_Should_Set_Service()
		{
			var container = new Container();

			var service = new ThreadService();
			container.Register(service);

			Assert.NotNull(container.Get<IThreadService>());
		}

		[Fact]
		public void Create()
		{
			var container = new Container();
			container.Register<PromptService>();

			var injectedProvider = container.Create<InjectedProvider>();
			Assert.NotNull((injectedProvider as IInjectable).IContainer);
			Assert.True(injectedProvider.IPromptService is IPromptService);
		}
	}
}
