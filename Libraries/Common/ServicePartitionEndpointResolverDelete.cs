using System;
using System.Collections.Generic;
using System.Fabric;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.ServiceFabric.Services.Client;

namespace Richter.Utilities
{
    public sealed class ServicePartitionEndpointResolverDelete {
        private readonly ServicePartitionResolver _mSpr;
        public readonly Uri ServiceName;
        public readonly string EndpointName;
        public readonly ServicePartitionKey PartitionKey;
        private ResolvedServicePartition _mRsp = null;
        private string _mEndpoint = null;
        public ServicePartitionEndpointResolverDelete(ServicePartitionResolver resolver, Uri serviceName, string endpointName, ServicePartitionKey partitionKey) {
            _mSpr = resolver;
            ServiceName = serviceName;
            EndpointName = endpointName;
            PartitionKey = partitionKey;
        }

        public async Task<TResult> ResolveAsync<TResult>(CancellationToken cancellationToken, Func<string, CancellationToken, Task<TResult>> func) {
            if (_mRsp == null) {
                // Get endpoints from naming service; https://msdn.microsoft.com/en-us/library/azure/dn707638.aspx
                _mRsp = await _mSpr.ResolveAsync(ServiceName, PartitionKey, cancellationToken);
            }
            for (;;) {
                try {
                    if (_mEndpoint == null) _mEndpoint = DeserializeEndpoints(_mRsp.GetEndpoint())[EndpointName];
                    return await func(_mEndpoint, cancellationToken);
                }
                catch (HttpRequestException ex) when ((ex.InnerException as WebException)?.Status == WebExceptionStatus.ConnectFailure) {
                    _mRsp = await _mSpr.ResolveAsync(_mRsp, cancellationToken);
                    _mEndpoint = null; // Retry after getting the latest endpoints from naming service
                }
            }
        }

        private static readonly JavaScriptSerializer s_javaScriptSerializer = new JavaScriptSerializer();
        private sealed class EndpointsCollection {
            public Dictionary<string, string> Endpoints = null;
        }
        private static IReadOnlyDictionary<string, string> DeserializeEndpoints(ResolvedServiceEndpoint partitionEndpoint) {
            return s_javaScriptSerializer.Deserialize<EndpointsCollection>(partitionEndpoint.Address).Endpoints;
        }
    }
}