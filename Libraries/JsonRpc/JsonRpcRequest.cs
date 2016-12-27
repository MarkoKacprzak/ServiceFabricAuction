using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Microsoft.CSharp.RuntimeBinder;

namespace SFAuction.JsonRpc
{
    public sealed class JsonRpcRequest : JsonRpcMessage
    {
        public JsonRpcRequest(JsonRpcMessageId id, string method, IDictionary<string, object> parameters = null)
            : base(id)
        {
            Method = method;
            ParametersAreNamed = true;
            NamedParameters = parameters;
        }

        public JsonRpcRequest(JsonRpcMessageId id, string method, IList<object> parameters = null) : base(id)
        {
            Method = method;
            ParametersAreNamed = false;
            PositionalParameters = parameters;
        }

        public readonly string Method = null;
        public readonly bool ParametersAreNamed;
        public readonly IDictionary<string, object> NamedParameters = null;
        public readonly IList<object> PositionalParameters = null;

        public override string ToString()
        {
            // JMR: throw if method is null
            var json = new StringBuilder("{ \"jsonrpc\": \"" + Version + "\"")
                .AppendId(Id)
                .Append($", \"method\": \"{Method}\"");
            if (ParametersAreNamed) json.AppendParameters(NamedParameters);
            else json.AppendParameters(PositionalParameters);
            json.Append("}");
            return json.ToString();
        }

        public static JsonRpcRequest Parse(string json)
        {
            var jsonObject = (IDictionary<string, object>) Serializer.DeserializeObject(json);
            jsonObject = new Dictionary<string, object>(jsonObject, StringComparer.OrdinalIgnoreCase);

            if ((string) jsonObject["jsonrpc"] != Version)
                throw new ArgumentException($"JsonRpc must be \"{Version}\"");

            var method = (string) jsonObject["method"];
            var paramsObject = !jsonObject.ContainsKey("params") ? null : jsonObject["params"];
            var positionalParameters = paramsObject as object[];
            if (positionalParameters != null)
            {
                if (!jsonObject.ContainsKey("id"))
                    return new JsonRpcRequest(null, method, positionalParameters);

                var jsonId = (string) jsonObject["id"];
                return (jsonId != null)
                    ? new JsonRpcRequest(jsonId, method, positionalParameters)
                    : new JsonRpcRequest(Convert.ToInt64(jsonObject["id"]), method, positionalParameters);
            }

            var namedParameters = paramsObject as IDictionary<string, object>;
            if (!jsonObject.ContainsKey("id"))
                return new JsonRpcRequest(null, method, namedParameters);

            var id = (string) jsonObject["id"];
            return (id != null)
                ? new JsonRpcRequest((string) jsonObject["id"], method, namedParameters)
                : new JsonRpcRequest(Convert.ToInt64(jsonObject["id"]), method, namedParameters);
        }

        public Task<JsonRpcResponse> InvokeAsync(JavaScriptSerializer jsSerializer, Type type,
                CancellationToken token = default(CancellationToken))
            => InvokeAndWrapAsync(jsSerializer, type, null, token);

        public Task<JsonRpcResponse> InvokeAsync(JavaScriptSerializer jsSerializer, object @object,
                CancellationToken token = default(CancellationToken))
            => InvokeAndWrapAsync(jsSerializer, @object.GetType(), @object, token);

        private async Task<JsonRpcResponse> InvokeAndWrapAsync(JavaScriptSerializer jsSerializer,
            Type type, object instance, CancellationToken token = default(CancellationToken))
        {
            JsonRpcResponse response = null;
            try
            {
                var jsonResult = await InvokeAsync(jsSerializer, type, instance, token);
                response = new JsonRpcResultResponse(Id, jsonResult);
            }
            catch (Exception e)
            {
                var parameters = string.Join(", ", ParametersAreNamed
                    ? NamedParameters.Select(kvp => kvp.Key + "=" + kvp.Value)
                    : PositionalParameters.Select(p => p.ToString()));

                response = new JsonRpcErrorResponse(Id,
                    (e is ArgumentException) ? JsonRpcError.InvalidParameters : JsonRpcError.ReservedLow,
                    e.Message,
                    $"Type={e.TargetSite.DeclaringType}, Method={e.TargetSite}, Params={parameters}");
            }
            return response;
        }

        private async Task<string> InvokeAsync(JavaScriptSerializer jsSerializer, Type type, object instance,
            CancellationToken token)
        {
            // Throw if method not found, parameter counts don't match(?), method requires arg not specified. Arg specified but not used?
            var methodInfo = type.GetTypeInfo().GetDeclaredMethod(Method);
            if (methodInfo == null) // Method not found
                throw new JsonRpcResponseErrorException(new JsonRpcErrorResponse(Id, JsonRpcError.MethodNotFound, Method));

            var methodParams = methodInfo.GetParameters();
            var lastArgIsCancellationToken = (methodParams.Length != 0) &&
                                             methodParams[methodParams.Length - 1].ParameterType ==
                                             typeof(CancellationToken);

            var arguments = new object[methodParams.Length]; // + (lastArgIsCancellationToken ? 1 : 0)];
            var passedArgs = methodParams.Length - (lastArgIsCancellationToken ? 1 : 0);
            for (var arg = 0; arg < passedArgs; arg++)
            {
                object argValue;
                if (!ParametersAreNamed)
                {
                    argValue = PositionalParameters[arg];
                }
                else
                {
                    if (!NamedParameters.TryGetValue(methodParams[arg].Name, out argValue))
                    {
                        // Required argument not found; throw
                        throw new ArgumentException($"Missing required argument: {methodParams[arg].Name}");
                    }
                }
                arguments[arg] = jsSerializer.ConvertToType(argValue, methodParams[arg].ParameterType);
            }
            if (lastArgIsCancellationToken)
                arguments[arguments.Length - 1] = token; // Add the CancellationToken as the last parameter
            // JMR: What if passed arg is not required by method? throw? flag to ignore?
            var result = methodInfo.Invoke(instance, arguments);
            var task = result as Task;
            if (task == null)
                return jsSerializer.Serialize(result);

            await task;
            try
            {
                result = ((dynamic) task).GetAwaiter().GetResult();
            }
            catch (RuntimeBinderException /* void-returning Task */)
            {
                result = null;
            }
            return jsSerializer.Serialize(result);
        }
    }
}