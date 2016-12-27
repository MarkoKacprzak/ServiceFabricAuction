using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFAuction.Common {
   /// <summary>
   /// This interface ensures that SFAuction.Svc.Auction.PartitionOperations and 
   /// SFAuction.OperationsProxy.AuctionOperationsProxy expose the same operations
   /// with the same signatures.
   /// </summary>
   public interface IInternetOperations {
      Task<UserInfo> CreateUserAsync(string userEmail, CancellationToken cancellationToken);
      Task<UserInfo> GetUserAsync(string userEmail, CancellationToken cancellationToken);
      Task<ItemInfo> CreateItemAsync(string sellerEmail, string itemName, string imageUrl, DateTime expiration, decimal startAmount, CancellationToken cancellationToken);
      Task<Bid[]> PlaceBidAsync(string bidderEmail, string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken);
      Task<ItemInfo[]> GetItemsBiddingAsync(string userEmail, CancellationToken cancellationToken);
      Task<ItemInfo[]> GetItemsSellingAsync(string _userEmail, CancellationToken cancellationToken);
      Task<ItemInfo[]> GetAuctionItemsAsync(CancellationToken cancellationToken);
   }
   public interface IInternalOperations {
      Task<Bid[]> PlaceBid2Async(string bidderEmail, string sellerEmail, string itemName, decimal bidAmount, CancellationToken cancellationToken);
   }
}