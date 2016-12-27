using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SFAuction.JsonRpc;
using System.Web.Script.Serialization;
using Richter.Utilities;

namespace SFAuction.OperationsProxy {
   public abstract class OperationProxy {
      private static readonly JavaScriptSerializer m_serializer = new JavaScriptSerializer();
      private static readonly HttpClient s_httpClient =
         new HttpClient(new CircuitBreakerHttpMessageHandler(3, TimeSpan.FromSeconds(30)));
      public readonly Resolver MResolver;
      protected OperationProxy(Resolver resolver, params JavaScriptConverter[] converters) {
         m_serializer.RegisterConverters(converters);
         MResolver = resolver;
      }
      private async Task<JsonRpcResponse> SendJsonRpcAsync(JsonRpcRequest request, CancellationToken cancellationToken) {
         // Send request to server:
         var response = await MResolver.CallAsync(cancellationToken, 
            async (ep, ct) => await s_httpClient.GetStringAsync(ep + $"?jsonrpc={request.ToString()}").WithCancellation(cancellationToken).ConfigureAwait(false));

         // Get response from server:
         return JsonRpcResponse.Parse(response);
      }

      protected async Task SendAsync(string method, IDictionary<string, object> parameters, CancellationToken cancellationToken) {
         var jsonRpcRequest = new JsonRpcRequest(Guid.NewGuid().ToString(), method, parameters);
         var jsonRpcResponse = await SendJsonRpcAsync(jsonRpcRequest, cancellationToken);
         var result = jsonRpcResponse.JsonResult;
      }
      protected async Task SendAsync(string method, IList<object> parameters, CancellationToken cancellationToken) {
         var jsonRpcRequest = new JsonRpcRequest(Guid.NewGuid().ToString(), method, parameters);
         var jsonRpcResponse = await SendJsonRpcAsync(jsonRpcRequest, cancellationToken);
         var result = jsonRpcResponse.JsonResult;
      }
      protected async Task<TResult> SendAsync<TResult>(string method, IDictionary<string, object> parameters, CancellationToken cancellationToken) {
         var jsonRpcRequest = new JsonRpcRequest(Guid.NewGuid().ToString(), method, parameters);
         var jsonRpcResponse = await SendJsonRpcAsync(jsonRpcRequest, cancellationToken);
         return m_serializer.Deserialize<TResult>(jsonRpcResponse.JsonResult);
      }
      protected async Task<TResult> SendAsync<TResult>(string method, IList<object> parameters, CancellationToken cancellationToken) {
         var jsonRpcRequest = new JsonRpcRequest(Guid.NewGuid().ToString(), method, parameters);
         var jsonRpcResponse = await SendJsonRpcAsync(jsonRpcRequest, cancellationToken);
         return m_serializer.Deserialize<TResult>(jsonRpcResponse.JsonResult);
      }
   }
}