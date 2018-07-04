namespace CmsZwo.Cache.Memory
{
	public interface IMemoryCacheDelegate : ICacheDelegate { }

	public class MemoryCacheDelegate : CacheDelegate, IMemoryCacheDelegate
	{
	}
}
