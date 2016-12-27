using System;
using System.Threading;
using System.Threading.Tasks;

namespace Richter.Utilities
{
    public sealed class Resolver {
        public readonly string ServiceName;
        public readonly long? PartitionKey;
        public readonly string EndpointName;
        private readonly PartitionEndpointResolver m_partitionEndpointResolver;
        internal Resolver(PartitionEndpointResolver partitionEndpointResolver, string serviceName, long? partitionKey, string endpointName) {
            m_partitionEndpointResolver = partitionEndpointResolver;
            ServiceName = serviceName;
            PartitionKey = partitionKey;
            EndpointName = endpointName;
        }
        public Task<TResult> CallAsync<TResult>(CancellationToken cancellationToken,
                Func<string, CancellationToken, Task<TResult>> func)
            => PartitionKey.HasValue
                ? m_partitionEndpointResolver.CallAsync(ServiceName, PartitionKey.Value, cancellationToken,
                    (ep, ct) => func(ep[EndpointName], ct))
                : m_partitionEndpointResolver.CallAsync(ServiceName, cancellationToken,
                    (ep, ct) => func(ep[EndpointName], ct));
    }
}