namespace CmsZwo.Repository
{
	public interface IAutoIdRepository : IRepository<AutoId> { }

	public class AutoIdRepository : Repository<AutoId>, IAutoIdRepository { }
}
