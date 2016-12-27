using System;

namespace SFAuction.JsonRpc
{
    public sealed class JsonRpcResponseErrorException : Exception {
        public readonly JsonRpcErrorResponse Response;
        public JsonRpcResponseErrorException(JsonRpcErrorResponse response)
            : base(response.Message) { Response = response; }

    }
}