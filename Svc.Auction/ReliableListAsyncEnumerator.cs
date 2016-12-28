using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;

namespace SFAuction.Svc.Auction
{
    internal sealed class ReliableListAsyncEnumerator<TKey> : IAsyncEnumerator<TKey>
    {
        private readonly IAsyncEnumerator<KeyValuePair<TKey, object>> _inner;

        public ReliableListAsyncEnumerator(IAsyncEnumerator<KeyValuePair<TKey, object>> inner)
        {
            _inner = inner;
        }

        public TKey Current => _inner.Current.Key;

        public void Dispose() => _inner.Dispose();

        public Task<bool> MoveNextAsync(CancellationToken cancellationToken) 
            => _inner.MoveNextAsync(cancellationToken);

        public void Reset() => _inner.Reset();
    }
}