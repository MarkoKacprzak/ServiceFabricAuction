using System.Text;

namespace SFAuction.JsonRpc
{
    public sealed class JsonRpcErrorResponse : JsonRpcResponse {
        public JsonRpcErrorResponse(JsonRpcMessageId id, JsonRpcError error, string message, string data = null) : base(id) {
            Error = error;
            Message = message;
            Data = data;
        }
        public JsonRpcErrorResponse(JsonRpcMessageId id, int error, string message, string data = null) : this(id, (JsonRpcError)error, message, data) { }

        public readonly JsonRpcError Error = 0;
        public readonly string Message;
        public readonly string Data;
        public override string ToString() {
            StringBuilder json = new StringBuilder($"{{\"jsonrpc\": \"{Version}\"")
                .AppendId(Id)
                .Append($", \"error\": {(int)Error}"); // 'error'?
            if (Message != null) json.Append($", \"message\": \"{Message}\"");
            if (Data != null) json.Append($", \"data\": \"{Data}\"");
            json.Append("}");
            return json.ToString();
        }
        public override string JsonResult { get { throw new JsonRpcResponseErrorException(this); } }
    }
}