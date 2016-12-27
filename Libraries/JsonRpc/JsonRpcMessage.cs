using System.Web.Script.Serialization;

namespace SFAuction.JsonRpc
{
    public abstract class JsonRpcMessage {
        // http://www.jsonrpc.org/specification
        public const string Version = "2.0";
        internal static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer();

        public readonly JsonRpcMessageId Id;
        protected JsonRpcMessage(JsonRpcMessageId id) { Id = id; }
    }
}