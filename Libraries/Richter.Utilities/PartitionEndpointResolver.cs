﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;                      // System.dll
using System.Net.Http;                 // System.Net.Http.dll
using System.Runtime.Caching;          // System.Runtime.Caching.dll
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization; // System.Web.Extensions.dll

namespace Richter.Utilities {
   public sealed class PartitionEndpointResolver : IDisposable {
      private static readonly JavaScriptSerializer JavaScriptSerializer = new JavaScriptSerializer();

      private readonly HttpClient _httpClient;
      private readonly string _clusterEndpoint;
      private readonly Cache<PartitionInfo> _clusterPartitionEndpointCache;

      private sealed class PartitionInfo {
         public PartitionInfo(string previousRspVersion, IDictionary<string, string> endpoints) {
            PreviousRspVersion = previousRspVersion;
            Endpoints = new ReadOnlyDictionary<string, string>(endpoints);
         }
         public readonly string PreviousRspVersion;
         public readonly IReadOnlyDictionary<string, string> Endpoints;
      }

      public PartitionEndpointResolver(string clusterEndpoint = "localhost", TimeSpan endpointTtl = default(TimeSpan), HttpClient httpClient = null) {
         _httpClient = httpClient ?? new HttpClient();
         _clusterEndpoint = clusterEndpoint;
         endpointTtl = (endpointTtl == default(TimeSpan)) ? TimeSpan.FromMinutes(5) : endpointTtl;
         _clusterPartitionEndpointCache = new Cache<PartitionInfo>(_clusterEndpoint, endpointTtl);
      }
      public void Dispose() => _clusterPartitionEndpointCache.Dispose();

      private async Task<PartitionInfo> ResolvePartitionEndpointsAsync(string serviceName, long? partitionKey, string previousRspVersion, CancellationToken cancellationToken = default(CancellationToken)) {
         serviceName = serviceName.Replace("fabric:/", string.Empty);

            string uri = $"http://{_clusterEndpoint}:19080/Services/{serviceName}/$/ResolvePartition?api-version=1.0";
         if (partitionKey != null)
            uri += $"&PartitionKeyType=2&PartitionKeyValue={partitionKey.Value}";
         if (previousRspVersion != null)
            uri += $"&PreviousRspVersion={previousRspVersion}";

         var partitionJson = await _httpClient.GetStringAsync(new Uri(uri)); // Fix to take CancellationToken
         var partitionObject = (IDictionary<string, object>)JavaScriptSerializer.DeserializeObject(partitionJson);
         previousRspVersion = (string)partitionObject["Version"];

         var partitionAddress = (string)
            ((IDictionary<string, object>)((object[])partitionObject["Endpoints"])[0])["Address"];

         IDictionary<string, string> partitionEndpoints =
            JavaScriptSerializer.Deserialize<EndpointsCollection>(partitionAddress).Endpoints;
         return new PartitionInfo(previousRspVersion, partitionEndpoints);
      }
      private sealed class EndpointsCollection {
         public readonly Dictionary<string, string> Endpoints = null;
      }

      public Resolver CreateSpecific(string serviceName, long partitionKey, string endpointName)
         => new Resolver(this, serviceName, partitionKey, endpointName);
      public Resolver CreateSpecific(string serviceName, string endpointName)
         => new Resolver(this, serviceName, null, endpointName);

      public Task<TResult> CallAsync<TResult>(string serviceName, CancellationToken cancellationToken,
         Func<IReadOnlyDictionary<string, string>, CancellationToken, Task<TResult>> func)
         => CallAsync(serviceName, null, cancellationToken, func);

      public Task<TResult> CallAsync<TResult>(string serviceName, long partitionKey, CancellationToken cancellationToken,
         Func<IReadOnlyDictionary<string, string>, CancellationToken, Task<TResult>> func)
         => CallAsync(serviceName, (long?) partitionKey, cancellationToken, func);

      private async Task<TResult> CallAsync<TResult>(string serviceName, long? partitionKey, CancellationToken cancellationToken,
         Func<IReadOnlyDictionary<string, string>, CancellationToken, Task<TResult>> func) {
         var serviceNameAndPartition = serviceName + ";" + partitionKey?.ToString();
         for (;;) {
            var servicePartitionInfo = _clusterPartitionEndpointCache.Get(serviceNameAndPartition);
            try {
               // We do not have endpoints, get them using https://msdn.microsoft.com/en-us/library/azure/dn707638.aspx
               if (servicePartitionInfo == null) {
                  servicePartitionInfo = await ResolvePartitionEndpointsAsync(serviceName, partitionKey, servicePartitionInfo?.PreviousRspVersion, cancellationToken);
                  _clusterPartitionEndpointCache.Add(serviceNameAndPartition, servicePartitionInfo);
               }
               return await func(servicePartitionInfo.Endpoints, cancellationToken);
            }
            catch (HttpRequestException ex) when ((ex.InnerException as WebException)?.Status == WebExceptionStatus.ConnectFailure) {
               // Force update of latest endpoints from naming service
               servicePartitionInfo = await ResolvePartitionEndpointsAsync(serviceName, partitionKey, servicePartitionInfo?.PreviousRspVersion, cancellationToken);
               _clusterPartitionEndpointCache.Set(serviceNameAndPartition, servicePartitionInfo);
            }
         }
      }


      private sealed class Cache<TValue> : IDisposable where TValue : class {
         private readonly MemoryCache m_cache;
         private readonly CacheItemPolicy m_cacheItemPolicy;
         public Cache(string name, TimeSpan slidingExpiration) {
            System.Collections.Specialized.NameValueCollection config = null;
            /*new System.Collections.Specialized.NameValueCollection {
               { "CacheMemoryLimitMegabytes", "50942361600" },
               { "PhysicalMemoryLimitPercentage", "99" },
               { "PollingInterval", "00:02:00" } };*/
            m_cache = new MemoryCache(name, config);
            m_cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = slidingExpiration };
         }
         public void Dispose() => m_cache.Dispose();
         public TValue Get(string key) => (TValue)m_cache.Get(key);
         public TValue AddOrGetExisting(string key, TValue value)
            => (TValue)m_cache.AddOrGetExisting(key, value, m_cacheItemPolicy);
         public bool Add(string key, TValue value)
            => m_cache.Add(key, value, m_cacheItemPolicy);
         public void Set(string key, TValue value)
            => m_cache.Set(key, value, m_cacheItemPolicy);
         public TValue Remove(string key) => (TValue)m_cache.Remove(key);
      }
   }
}