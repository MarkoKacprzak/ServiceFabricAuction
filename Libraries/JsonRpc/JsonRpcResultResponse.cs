using System.Text;

namespace SFAuction.JsonRpc
{
    public sealed class JsonRpcResultResponse : JsonRpcResponse {
        private readonly string _mJsonResult;
        public JsonRpcResultResponse(JsonRpcMessageId id, string jsonResult) : base(id) {
            _mJsonResult = jsonResult;
        }
        public override string JsonResult => _mJsonResult;

        public override string ToString() {
            var json = new StringBuilder($"{{\"jsonrpc\": \"{Version}\"")
                .AppendId(Id)
                .Append($", \"result\": {_mJsonResult}")
                .Append("}");
            return json.ToString();
        }
    }
}