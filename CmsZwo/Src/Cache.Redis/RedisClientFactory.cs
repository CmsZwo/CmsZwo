using ServiceStack.Redis;

namespace CmsZwo.Cache
{
	public interface IRedisClientFactory : IService
	{
		IRedisClient GetClient();
	}

	public class RedisClientFactory : Injectable, IRedisClientFactory
	{
		#region Inject

		[Inject]
		public ISettingsService ISettingsService { get; set; }

		#endregion

		#region Tools

		private CacheServiceSettings.RedisSettings _Settings
			=> ISettingsService.Get<CacheServiceSettings>()?.Redis;

		private IRedisClientsManager _IRedisClientsManager = null;

		private IRedisClientsManager IRedisClientsManager
		{
			get
			{
				if (_IRedisClientsManager == null)
					_IRedisClientsManager = new RedisManagerPool(_Settings.ConnectionString);

				return _IRedisClientsManager;
			}
		}

		#endregion

		#region IRedisClientFactory

		public IRedisClient GetClient()
			=> IRedisClientsManager.GetClient();

		#endregion
	}
}
