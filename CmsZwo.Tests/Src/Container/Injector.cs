using System;
using Xunit;
using Moq;

namespace CmsZwo.Container.Tests
{
	public class InjectorTests
	{
		[Fact]
		public void Inject()
		{
			var container = new Mock<IContainer>();
			container
				.Setup(x => x.Get(It.Is<Type>(s => s.Name == nameof(IPromptService))))
				.Returns(new PromptService());

			var injector = new Injector(container.Object);

			var injectedProvider = new InjectedProvider();
			injector.Inject(injectedProvider);

			Assert .Equal("Hello, World!", injectedProvider.IPromptService.Text);
		}

		[Fact]
		public void Create()
		{
			var registry = new Mock<IContainer>();
			registry
				.Setup(x => x.Get(It.Is<Type>(s => s.Name == nameof(IPromptService))))
				.Returns(new PromptService());

			var injector = new Injector(registry.Object);
			var injectedProvider = injector.Create<InjectedProvider>();

			Assert.Equal("Hello, World!", injectedProvider.IPromptService.Text);
		}
	}
}
