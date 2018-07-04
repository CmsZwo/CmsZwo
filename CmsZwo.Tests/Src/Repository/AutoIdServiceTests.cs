using System.Threading.Tasks;

using Moq;
using Xunit;

namespace CmsZwo.Repository.Tests
{
	public class AutoIdServiceTests
	{
		public class DummyEntity : Entity { }

		[Fact]
		public async void GetNext_Should_Invoke_Repository()
		{
			var service = MoqHelper.CreateWithMocks<AutoIdService>();

			var autoId = new AutoId
			{
				Value = 10ul
			};

			var IAutoIdRepository = Mock.Get(service.IAutoIdRepository);
			IAutoIdRepository
				.Setup(x => x.GetByNameAsync(It.IsAny<string>()))
				.Returns(Task.FromResult(autoId));

			Assert.Equal(11ul, await service.GetNext<DummyEntity>());
			Assert.Equal(12ul, await service.GetNext<DummyEntity>());

			IAutoIdRepository.Verify(x => x.SaveAsync(It.Is<AutoId>(y => y.Value != 0ul)), Times.Exactly(2));
		}

		[Fact]
		public async void GetNext_Should_Create_AutoId()
		{
			var service = MoqHelper.CreateWithMocks<AutoIdService>();
			var IAutoIdRepository = Mock.Get(service.IAutoIdRepository);

			await service.GetNext<DummyEntity>();

			IAutoIdRepository.Verify(x => x.SaveAsync(It.IsAny<AutoId>()), Times.Exactly(2));
		}
	}
}
