using System.Collections.Generic;
using Microsoft.ServiceFabric.Data;

namespace SFAuction.Svc.Auction
{
    internal sealed class ReliableListEnumerable<TKey> : IAsyncEnumerable<TKey>
    {
        private readonly IAsyncEnumerable<KeyValuePair<TKey, object>> _inner;

        public ReliableListEnumerable(IAsyncEnumerable<KeyValuePair<TKey, object>> inner)
        {
            _inner = inner;
        }

        public IAsyncEnumerator<TKey> GetAsyncEnumerator() =>
            new ReliableListAsyncEnumerator<TKey>(_inner.GetAsyncEnumerator());
    }
}