using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Richter.Utilities;
using SFAuction.Common;

namespace SFAuction.OperationsProxy
{
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
}