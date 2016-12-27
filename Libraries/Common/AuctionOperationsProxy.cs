using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SFAuction.Common;
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

   public sealed class InternetAuctionOperationProxy : OperationProxy, IInternetOperations {
      public InternetAuctionOperationProxy(Resolver resolver) 
         : base(resolver, new DataTypeJsonConverter()) {
      }

      public Task<UserInfo> CreateUserAsync(string userEmail, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(userEmail)] = userEmail,
         };
         return SendAsync<UserInfo>(nameof(CreateUserAsync), parameters, cancellationToken);
      }
      public Task<UserInfo> GetUserAsync(string userEmail, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(userEmail)] = userEmail,
         };
         return SendAsync<UserInfo>(nameof(GetUserAsync), parameters, cancellationToken);
      }

      public Task<ItemInfo> CreateItemAsync(string sellerEmail, string itemName, string imageUrl, DateTime expiration, decimal startAmount, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(sellerEmail)] = sellerEmail,
            [nameof(itemName)] = itemName,
            [nameof(imageUrl)] = imageUrl,
            [nameof(expiration)] = expiration,
            [nameof(startAmount)] = startAmount
         };
         return SendAsync<ItemInfo>(nameof(CreateItemAsync), parameters, cancellationToken);
      }

      public Task<Bid[]> PlaceBidAsync(string bidderEmail, string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(bidderEmail)] = bidderEmail,
            [nameof(sellerEmail)] = sellerEmail,
            [nameof(itemName)] = itemName,
            [nameof(bidAmount)] = bidAmount
         };
         return SendAsync<Bid[]>(nameof(PlaceBidAsync), parameters, cancellationToken);
      }

      public Task<Bid> PlaceBid2Async(string bidderEmail, string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(bidderEmail)] = bidderEmail,
            [nameof(sellerEmail)] = sellerEmail,
            [nameof(itemName)] = itemName,
            [nameof(bidAmount)] = bidAmount
         };
         return SendAsync<Bid>(nameof(PlaceBid2Async), parameters, cancellationToken);
      }

      public Task<ItemInfo[]> GetItemsBiddingAsync(string userEmail, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(userEmail)] = userEmail
         };
         return SendAsync<ItemInfo[]>(nameof(GetItemsBiddingAsync), parameters, cancellationToken);
      }

      public Task<ItemInfo[]> GetItemsSellingAsync(string userEmail, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(userEmail)] = userEmail
         };
         return SendAsync<ItemInfo[]>(nameof(GetItemsSellingAsync), parameters, cancellationToken);
      }
      public Task<ItemInfo[]> GetAuctionItemsAsync(CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> { };
         return SendAsync<ItemInfo[]>(nameof(GetAuctionItemsAsync), parameters, cancellationToken);
      }
   }

   public sealed class InternalAuctionOperationProxy : OperationProxy, IInternalOperations {
      public InternalAuctionOperationProxy(Resolver resolver) : base(resolver, new DataTypeJsonConverter()) {
      }

      public Task<Bid[]> PlaceBid2Async(string bidderEmail, string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken) {
         var parameters = new Dictionary<string, object> {
            [nameof(bidderEmail)] = bidderEmail,
            [nameof(sellerEmail)] = sellerEmail,
            [nameof(itemName)] = itemName,
            [nameof(bidAmount)] = bidAmount
         };
         return SendAsync<Bid[]>(nameof(PlaceBid2Async), parameters, cancellationToken);
      }
   }
}