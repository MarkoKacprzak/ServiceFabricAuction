using System;
using System.Collections.Generic;

namespace SFAuction.JsonRpc
{
    public abstract class JsonRpcResponse : JsonRpcMessage
    {
        protected JsonRpcResponse(JsonRpcMessageId id) : base(id)
        {
        }

        public static JsonRpcResponse Parse(string json)
        {
            var jsonObject = (IDictionary<string, object>) Serializer.DeserializeObject(json);
            if ((string) jsonObject["jsonrpc"] != Version)
                throw new ArgumentException($"JsonRpc must be \"{Version}\"");

            if (jsonObject.ContainsKey("result"))
            {
                var jsonResult = Serializer.Serialize(jsonObject["result"]); // Turn .NET objects back into JSON
                var jsonId = jsonObject["id"] as string;
                return (jsonId != null)
                    ? new JsonRpcResultResponse(jsonId, jsonResult)
                    : new JsonRpcResultResponse(Convert.ToInt64(jsonObject["id"]), jsonResult);
            }

            if (jsonObject.ContainsKey("error"))
            {
                var error = (int) jsonObject["error"];
                var message = (string) jsonObject["message"];
                var data = !jsonObject.ContainsKey("data") ? null : (string) jsonObject["data"];
                var jsonId = jsonObject["id"] as string;
                return (jsonId != null)
                    ? new JsonRpcErrorResponse((string) jsonObject["id"], error, message, data)
                    : new JsonRpcErrorResponse(Convert.ToInt64(jsonObject["id"]), error, message, data);
            }
            throw new ArgumentException("Response must contain 'result' or 'error'");
        }

        public abstract string JsonResult { get; }
    }
}