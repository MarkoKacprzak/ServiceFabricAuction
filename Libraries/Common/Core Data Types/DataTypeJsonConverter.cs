﻿using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace SFAuction.Common {

   public sealed class DataTypeJsonConverter : JavaScriptConverter {
      public override IEnumerable<Type> SupportedTypes => new[] {
         typeof(UserInfo), typeof(ItemId), typeof(ItemInfo), typeof(Bid)
      };
      public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer) {
         if (obj.GetType() == typeof(UserInfo)) {
            var userInfo = (UserInfo)obj;
            return new Dictionary<string, object> {
                  { "Email", userInfo.Email.ToString() },
               };

         }
         if (obj.GetType() == typeof(ItemId)) {
            var itemId = (ItemId)obj;
            return new Dictionary<string, object> {
                  { "Seller", itemId.Seller.ToString() },
                  { "ItemName", itemId.ItemName }
               };
         }

         if (obj.GetType() == typeof(ItemInfo)) {
            var itemInfo = (ItemInfo)obj;
            return new Dictionary<string, object> {
                  { "ItemId", itemInfo.ItemId },
                  { "ImageUrl", itemInfo.ImageUrl },
                  { "Expiration", itemInfo.Expiration },
                  { "Bids", itemInfo.Bids}
               };
         }

         var bid = obj as Bid;
         if (bid != null) {
            return new Dictionary<string, object> {
                  { "Bidder", bid.Bidder.ToString() },
                  { "Amount", bid.Amount},
                  { "Time", bid.Time}
               };
         }
         return null;
      }
      public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
         if (type == typeof(UserInfo))
            return new UserInfo(Email.Parse(serializer.ConvertToType<string>(dictionary["Email"])));

         if (type == typeof(ItemId))
            return new ItemId(Email.Parse(serializer.ConvertToType<string>(dictionary["Seller"])),
               serializer.ConvertToType<string>(dictionary["ItemName"]));

         if (type == typeof(ItemInfo))
            return new ItemInfo(
               serializer.ConvertToType<ItemId>(dictionary["ItemId"]),
               serializer.ConvertToType<string>(dictionary["ImageUrl"]),
               serializer.ConvertToType<DateTime>(dictionary["Expiration"]),
               serializer.ConvertToType<Bid[]>(dictionary["Bids"]));

         if (type == typeof(Bid))
            return new Bid(Email.Parse(serializer.ConvertToType<string>(dictionary["Bidder"])),
               serializer.ConvertToType<decimal>(dictionary["Amount"]),
               serializer.ConvertToType<DateTime>(dictionary["Time"]));

         return null;
      }
   }
}