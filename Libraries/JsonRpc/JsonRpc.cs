namespace SFAuction.JsonRpc {
   public struct JsonRpcMessageId {
      public readonly bool IsString; // false
      public readonly long Number;  // 0
      public readonly string String; // null
      public JsonRpcMessageId(long id) {
         IsString = false;
         Number = id;
         String = null;
      }
      public JsonRpcMessageId(string id) {
         IsString = true;
         String = id;
         Number = 0;
      }

      public static implicit operator JsonRpcMessageId(long id) => new JsonRpcMessageId(id);
      public static implicit operator JsonRpcMessageId(string id) => new JsonRpcMessageId(id);
   }
}