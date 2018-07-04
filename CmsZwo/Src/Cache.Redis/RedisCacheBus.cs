using System;
using System.Linq;
using System.Threading.Tasks;

using ServiceStack.Redis;

namespace CmsZwo.Cache.Redis
{
	public interface IRedisCacheBus : ICacheBus { }

	public class RedisCacheBus : CacheBus, IRedisCacheBus
	{
		#region Inject

		[Inject]
		public IThreadService IThreadService { get; set; }

		[Inject]
		public IRedisGatewayService IRedisGatewayService { get; set; }

		[Inject]
		public IRedisClientFactory IRedisClientFactory { get; set; }

		#endregion

		#region Tools

		private CacheServiceSettings.RedisSettings _Settings
			=> ISettingsService.Get<CacheServiceSettings>()?.Redis;

		private const string _MessagesSet = nameof(RedisCacheBus) + ":messages";

		protected override void PullMissingMessages(long startingSequence)
		{
			using (var client = IRedisClientFactory.GetClient())
			{
				var items = client.GetAllWithScoresFromSortedSet(_MessagesSet);

				var missingMessages =
					items
						.Where(x => x.Value > startingSequence)
						.Select(x => x.Key.ToObjectByJson<CacheBusCommand>());

				foreach (var command in missingMessages.Safe())
					OnReciveCommand(command);
			}
		}

		protected override Task TransferCommand(CacheBusCommand command)
		{
			return
				Task.Run(() =>
				{
					using (var client = IRedisClientFactory.GetClient())
					{
						var sequence = client.Increment($"{nameof(RedisCacheBus)}:sequence", 1);

						command.Sequence = sequence;

						client.AddItemToSortedSet(_MessagesSet, command.ToJson(), sequence);
						client.PublishMessage(_Target, command.ToJson());
					}
				});
		}

		protected override Task Purge(long nextPurgeSequence)
		{
			return
				Task.Run(() =>
				{
					using (var client = IRedisClientFactory.GetClient())
					{
						client.RemoveRangeFromSortedSetByScore(_MessagesSet, 0, nextPurgeSequence);
					}
				});
		}

		#endregion

		#region ICacheBusDelegate

		private void OnMessage(string channel, string raw)
			=> OnReceivedMessage(raw);

		public override void Start()
		{
			base.Start();

			_Target = _Settings.SyncChannel;

			IThreadService.QueueUserWorkItem(x =>
			{
				Start:

				try
				{
					using (var client = IRedisClientFactory.GetClient())
					using (var subscription = client.CreateSubscription())
					{
						subscription.OnMessage = OnMessage;
						subscription.SubscribeToChannels(new[] { _Target });
					}
				}
				catch (RedisException)
				{
					IThreadService.Sleep(TimeSpan.FromSeconds(30));
					goto Start;
				}
			});
		}

		#endregion
	}
}
