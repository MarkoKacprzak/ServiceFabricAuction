using System;
using System.Runtime.Serialization;
using Richter.Utilities;

namespace SFAuction.Common {
   /// <summary>ItemId is case preserved but case insensitive.</summary>
   [DataContract]
   public struct ItemId : IEquatable<ItemId>, IComparable<ItemId> {
      #region Static members
      private const int c_MaxCharacters = 100;
      private const string c_delimiter = "~";
      public static readonly ItemId Empty = new ItemId(Email.Empty, string.Empty);

      private static bool IsValidFormat(string itemName) => true;

      public static ItemId Parse(Email seller, string itemName, bool trustInput = false) {
         if (!trustInput) {
            if (itemName != null) itemName = itemName.Trim();
            if (itemName.IsNullOrWhiteSpace() || itemName.Length > c_MaxCharacters || !IsValidFormat(itemName))
               throw new InvalidItemIdFormatException(itemName);
         }
         return new ItemId(seller, itemName);
      }

      public static bool operator ==(ItemId itemId1, ItemId itemId2) => itemId1.Equals(itemId2);
      public static bool operator !=(ItemId ItemId1, ItemId itemId2) => !ItemId1.Equals(itemId2);
      #endregion

      [DataMember] public readonly Email Seller;
      [DataMember] public readonly string ItemName;
      public ItemId(Email seller, string itemName) {
         if (itemName == null) throw new ArgumentNullException(nameof(itemName));
         Seller = seller;
         ItemName = itemName;
      }
      public bool IsEmpty => ItemName.IsNullOrWhiteSpace();
      public override string ToString() => $"Seller={Seller}, Name={ItemName}";
      public string Key => (Seller + c_delimiter + ItemName).ToLowerInvariant();
      public override bool Equals(object obj) => (obj is ItemId) && Equals((ItemId)obj);
      public bool Equals(ItemId other) => CompareTo(other) == 0;
      public override int GetHashCode() => Key.GetHashCode();
      public int CompareTo(ItemId other) {
         if (other == null) return 1;
         var n = Seller.CompareTo(other.Seller);
         return (n != 0) ? n : string.Compare(ItemName, other.ItemName, StringComparison.OrdinalIgnoreCase);
      }
   }

   public sealed class InvalidItemIdFormatException : Exception {
      public InvalidItemIdFormatException(string attemptedItemId) { AttemptedItemId = attemptedItemId; }
      public string AttemptedItemId { get; private set; }
   }
}