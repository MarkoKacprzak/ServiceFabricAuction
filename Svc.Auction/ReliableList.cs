using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Richter.Utilities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFAuction.Svc.Auction
{

    public sealed class ReliableList<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly IReliableDictionary<TKey, object> _dictionary;

        public static async Task<ReliableList<TKey>> CreateAsync(IReliableStateManager stateManager, string name)
        {
            return new ReliableList<TKey>(await stateManager.GetOrAddAsync<IReliableDictionary<TKey, object>>(name));
        }

        private ReliableList(IReliableDictionary<TKey, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public Task ClearAsync(TimeSpan timeout = default(TimeSpan),
                CancellationToken cancellationToken = default(CancellationToken))
            => _dictionary.ClearAsync(timeout.DefaultToInfinite(), cancellationToken);

        public Task AddAsync(ITransaction tx, TKey key, TimeSpan timeout = default(TimeSpan),
                CancellationToken cancellationToken = default(CancellationToken))
            => _dictionary.AddAsync(tx, key, null, timeout.DefaultToInfinite(), cancellationToken);

        public Task<bool> ContainsAsync(ITransaction tx, TKey key, LockMode lockMode = LockMode.Default,
                TimeSpan timeout = default(TimeSpan), CancellationToken cancellationToken = default(CancellationToken))
            => _dictionary.ContainsKeyAsync(tx, key, lockMode, timeout.DefaultToInfinite(), cancellationToken);

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TimeSpan timeout = default(TimeSpan),
                CancellationToken cancellationToken = default(CancellationToken))
            => _dictionary.TryAddAsync(tx, key, null, timeout.DefaultToInfinite(), cancellationToken);

        public Task TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout = default(TimeSpan),
                CancellationToken cancellationToken = default(CancellationToken))
            => _dictionary.TryRemoveAsync(tx, key, timeout.DefaultToInfinite(), cancellationToken);

        public async Task<IAsyncEnumerable<TKey>> CreateEnumerableAsync(ITransaction tx,
            EnumerationMode enumerationMode = EnumerationMode.Unordered, Func<TKey, bool> filter = null)
        {
            var enumerable = await _dictionary.CreateEnumerableAsync(tx, filter ?? (k => true), enumerationMode);
            return new ReliableListEnumerable<TKey>(enumerable);
        }
    }
}