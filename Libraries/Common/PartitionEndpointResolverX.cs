using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;

namespace Richter.Utilities
{
    public sealed class PartitionEndpointResolverX {
        public readonly Uri ServiceName;
        public readonly string EndpointName;
        private static readonly ServicePartitionResolver s_servicePartitionResolver
            = ServicePartitionResolver.GetDefault();

        public PartitionEndpointResolverX(Uri serviceName, string endpointName) {
            ServiceName = serviceName;
            EndpointName = endpointName;
        }
        public Task<string> ResolveEndpointAsync(ServicePartitionKey partitionKey, CancellationToken cancellationToken) {
            return s_servicePartitionResolver.ResolveEndpointAsync(ServiceName, partitionKey, EndpointName, cancellationToken);
        }
    }
}