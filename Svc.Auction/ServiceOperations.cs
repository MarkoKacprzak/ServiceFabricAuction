﻿using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Richter.Utilities;
using SFAuction.Common;
using SFAuction.OperationsProxy;

namespace SFAuction.Svc.Auction
{
    internal sealed class ServiceOperations : IInternetOperations, IInternalOperations
    {
        #region Infrastructure

        private const string EndpointName = "ReplicaEndpoint";
        private readonly Uri _serviceNameUri;
        private readonly PartitionEndpointResolver _partitionEndpointResolver;

        internal ServiceOperations(PartitionEndpointResolver partitionEndpointResolver, Uri serviceNameUri)
        {
            _partitionEndpointResolver = partitionEndpointResolver;
            _serviceNameUri = serviceNameUri;
        }

        private InternetAuctionOperationProxy GetProxy(string email)
        {
            // Maps email to a partition key
            var partitionKey = Email.Parse(email).PartitionKey();
            var resolver = _partitionEndpointResolver.CreateSpecific(_serviceNameUri.ToString(), partitionKey,
                EndpointName);
            return new InternetAuctionOperationProxy(resolver);
        }

        #endregion

        /// <summary>
        /// This method executes on the bidder's partition.
        /// Called by web: priority 0
        /// </summary>
        public Task<UserInfo> CreateUserAsync(string userEmail, CancellationToken cancellationToken) =>
            GetProxy(userEmail).CreateUserAsync(userEmail, cancellationToken);

        public Task<UserInfo> GetUserAsync(string userEmail, CancellationToken cancellationToken) =>
            GetProxy(userEmail).GetUserAsync(userEmail, cancellationToken);

        /// <summary>
        /// This method executes on the seller's partition.
        /// Called by web: priority 0
        /// </summary>
        public Task<ItemInfo> CreateItemAsync(string sellerEmail, string itemName, string imageUrl,
                DateTime expiration, decimal startAmount, CancellationToken cancellationToken) =>
            GetProxy(sellerEmail)
                .CreateItemAsync(sellerEmail, itemName, imageUrl, expiration, startAmount, cancellationToken);

        /// <summary>
        /// This method executes on the bidder's partition.
        /// Called by web: priority 0
        /// </summary>
        public Task<Bid[]> PlaceBidAsync(string bidderEmail,
                string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken) =>
            GetProxy(bidderEmail).PlaceBidAsync(bidderEmail, sellerEmail, itemName, bidAmount, cancellationToken);

        /// <summary>
        /// Called by web but priority 1
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ItemInfo[]> GetItemsBiddingAsync(string userEmail, CancellationToken cancellationToken) =>
            GetProxy(userEmail).GetItemsBiddingAsync(userEmail, cancellationToken);

        //public async Task GetMyBidsAsync(Email bidderEmail, ItemId itemId, CancellationToken cancellationToken) {      }
        /// <summary>
        /// Called by web but priority 1
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ItemInfo[]> GetItemsSellingAsync(string userEmail, CancellationToken cancellationToken) =>
            GetProxy(userEmail).GetItemsSellingAsync(userEmail, cancellationToken);

        /// <summary>
        /// Call by web home page a lot: priority 0
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ItemInfo[]> GetAuctionItemsAsync(CancellationToken cancellationToken)
        {
            var qm = new FabricClient().QueryManager;
            // Get this service's partitions
            var partitions =
                await qm.GetPartitionListAsync(_serviceNameUri, null, TimeSpan.FromSeconds(4), cancellationToken);

            // Get each partition's low key
            var partitionKeys = from partition in partitions
                select ((Int64RangePartitionInformation) partition.PartitionInformation).LowKey;

            // Simultaneously ask each partition for the auction items is has to sell
            var tasks = new List<Task<ItemInfo[]>>();
            foreach (var p in partitionKeys)
            {
                var resolver = _partitionEndpointResolver.CreateSpecific(_serviceNameUri.ToString(), p, EndpointName);
                var proxy = new InternetAuctionOperationProxy(resolver);
                tasks.Add(proxy.GetAuctionItemsAsync(cancellationToken));
            }

            // Continue processing after every partition's auction items have come in
            var partitionsItemInfos = await Task.WhenAll(tasks);

            // Combine them all together sorting them by expiration
            var results = (from partitionItemInfos in partitionsItemInfos
                from itemInfo in partitionItemInfos
                // where itemInfo.Expiration >= DateTime.UtcNow   // Change this for Version 2
                orderby itemInfo.Expiration
                select itemInfo).ToArray();
            return results; // Return the set of auction items
        }

        Task<Bid[]> IInternalOperations.PlaceBid2Async(string bidderEmail, string sellerEmail, string itemName,
            decimal bidAmount, CancellationToken cancellationToken)
        {
            var partitionKey = Email.Parse(sellerEmail).PartitionKey();
            var resolver = _partitionEndpointResolver.CreateSpecific(_serviceNameUri.ToString(), partitionKey,
                EndpointName);
            return new InternalAuctionOperationProxy(resolver).PlaceBid2Async(bidderEmail, sellerEmail, itemName,
                bidAmount, cancellationToken);
        }
    }
}