using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Richter.Utilities;
using SFAuction.Common;

namespace SFAuction.OperationsProxy
{
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