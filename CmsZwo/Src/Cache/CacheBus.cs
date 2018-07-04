using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Cache.Memory;

namespace CmsZwo.Cache
{
	public class CacheBusCommand
	{
		public long? Sequence { get; set; }

		public DateTime? Created { get; set; }
		public string Target { get; set; }
		public string Sender { get; set; }
		public string Name { get; set; }
		public string Key { get; set; }
		public IEnumerable<string> Parameter { get; set; }
	}

	public interface ICacheBus : IService
	{
		void Start();

		Task RemoveAsync(string key);
		Task RemoveManyAsync(IEnumerable<string> keys);

		Task AddToSetAsync(string key, IEnumerable<string> items);
		Task RemoveFromSetAsync(string key, string item);

		Task ClearAsync();
	}

	public abstract class CacheBus : Injectable, ICacheBus
	{
		#region Inject

		[Inject]
		public ITimeService ITimeService { get; set; }

		[Inject]
		public ISettingsService ISettingsService { get; set; }

		[Inject]
		public IRandomService IRandomService { get; set; }

		[Inject]
		public IMemoryCacheDelegate IMemoryCacheDelegate { get; set; }

		#endregion

		#region Tools

		private string _InstanceId;
		private bool _Started;

		protected string _Target;

		private long _LastReceivedSequence;

		private DateTime? _NextPurge;
		private long _NextPurgeSequence;

		protected abstract void PullMissingMessages(long startingSequence);

		protected void OnReceivedMessage(string raw)
		{
			var command = raw.ToObjectByJson<CacheBusCommand>();
			OnReciveCommand(command);
		}

		protected void OnReciveCommand(CacheBusCommand command)
		{
			if (command.Sequence < _LastReceivedSequence)
				return;

			if (_LastReceivedSequence > 0)
				if (_LastReceivedSequence + 1 < command.Sequence)
					PullMissingMessages(_LastReceivedSequence);

			_LastReceivedSequence = command.Sequence.GetValueOrDefault();

			if (command.Target != _Target)
				return;

			if (command.Sender == _InstanceId)
				return;

			if (command.Name == nameof(RemoveAsync))
				IMemoryCacheDelegate.RemoveAsync(command.Key);

			else if (command.Name == nameof(RemoveManyAsync))
				IMemoryCacheDelegate.RemoveAsync(command.Key.ToObjectByJson<List<string>>());

			else if (command.Name == nameof(ClearAsync))
				IMemoryCacheDelegate.ClearAsync();

			else if (command.Name == nameof(AddToSetAsync))
				IMemoryCacheDelegate.AddToSetAsync(command.Key, command.Parameter);

			else if (command.Name == nameof(RemoveFromSetAsync))
				IMemoryCacheDelegate.RemoveFromSetAsync(command.Key, command.Parameter);
		}

		private async Task SendCommand(string name, string key = null, IEnumerable<string> parameter = null)
		{
			if (!_Started)
				throw new Exception("Start first.");

			var command = new CacheBusCommand
			{
				Created = ITimeService.Now,
				Target = _Target,
				Sender = _InstanceId,
				Name = name,
				Key = key,
				Parameter = parameter
			};

			await TransferCommand(command);
			await HandlePurge(command);
		}

		private async Task HandlePurge(CacheBusCommand command)
		{
			if (_NextPurge == null)
			{
				_NextPurge = ITimeService.Now.AddMinutes(20);
				_NextPurgeSequence = command.Sequence.Value;
			}

			if (ITimeService.Now > _NextPurge)
			{
				await Purge(_NextPurgeSequence);

				_NextPurge = ITimeService.Now.AddMinutes(20);
				_NextPurgeSequence = command.Sequence.Value;
			}
		}

		protected abstract Task TransferCommand(CacheBusCommand command);
		protected abstract Task Purge(long nextPurgeSequence);

		#endregion

		#region ICacheBusDelegate

		public virtual void Start()
		{
			_InstanceId = IRandomService.RandomBase24(8);
			_Started = true;
		}

		public Task RemoveAsync(string key)
			=> SendCommand(nameof(RemoveAsync), key);

		public Task RemoveManyAsync(IEnumerable<string> keys)
		{
			if (!keys.HasContent())
				return Task.CompletedTask;

			return SendCommand(nameof(RemoveManyAsync), keys.ToList().ToJson());
		}

		public Task ClearAsync()
			=> SendCommand(nameof(ClearAsync));

		public Task AddToSetAsync(string key, IEnumerable<string> items)
			=> SendCommand(nameof(AddToSetAsync), key, items);

		public Task RemoveFromSetAsync(string key, string item)
			=> SendCommand(nameof(RemoveFromSetAsync), key, new[] { item });

		#endregion
	}
}
