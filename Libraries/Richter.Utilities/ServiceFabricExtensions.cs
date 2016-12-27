using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization; // System.Web.Extensions.dll
using Microsoft.ServiceFabric.Services.Client;

namespace Richter.Utilities {
    public static class ServiceFabricExtensions {
      public static EndpointResourceDescription GetEndpointResourceDescription(this ServiceContext context, string endpointName)
            => context.CodePackageActivationContext.GetEndpoint(endpointName);

      public static string CalcUriSuffix(this StatelessServiceContext context)
         => context.CalcUriSuffix(context.InstanceId);

      public static string CalcUriSuffix(this StatefulServiceContext context)
         => context.CalcUriSuffix(context.ReplicaId);

      private static string CalcUriSuffix(this ServiceContext context, long instanceOrReplicaId)
         => $"{context.PartitionId}/{instanceOrReplicaId}" +
            $"/{Guid.NewGuid().ToByteArray().ToBase32String()}/";   // Uniqueness

      private static readonly JavaScriptSerializer s_javaScriptSerializer = new JavaScriptSerializer();
      private sealed class EndpointsCollection {
         public Dictionary<string, string> Endpoints = null;
      }

      public static async Task<string> ResolveEndpointAsync(this ServicePartitionResolver resolver, Uri namedService, ServicePartitionKey partitionKey, string endpointName, CancellationToken cancellationToken) {
         var partition = await resolver.ResolveAsync(namedService, partitionKey, cancellationToken);
         return DeserializeEndpoints(partition.GetEndpoint())[endpointName];
      }


      public static async Task<IReadOnlyDictionary<string, string>> ResolveEndpointsAsync(this ServicePartitionResolver resolver, Uri namedService, ServicePartitionKey partitionKey, CancellationToken cancellationToken) {
         var partition = await resolver.ResolveAsync(namedService, partitionKey, cancellationToken);
         return DeserializeEndpoints(partition.GetEndpoint());
      }

      private static IReadOnlyDictionary<string, string> DeserializeEndpoints(ResolvedServiceEndpoint partitionEndpoint) {
         return s_javaScriptSerializer.Deserialize<EndpointsCollection>(partitionEndpoint.Address).Endpoints;
      }
   }
}