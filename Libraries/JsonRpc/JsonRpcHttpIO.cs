using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonRpc.Richter.Utilities;

namespace SFAuction.JsonRpc
{

#if false
    public sealed class JsonRpcHttpIO /*, IDisposable */{
        public static async Task StartServerAsync(string port, int maxRequestSizeInBytes, Func<JsonRpcRequest, Task<JsonRpcResponse>> serviceClientRequestAsync, CancellationToken ct = default(CancellationToken)) {
            using (var server = new HttpListener()) {
                try {
                    // From Admin CMD:
                    // netsh http add urlacl url=http://*:8080/ user=MicrosoftAccount\JeffRichter@live.com listen=yes
                    server.Prefixes.Add($"http://*:{port}/");
                    server.Start();
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                }

                while (true) {
                    ct.ThrowIfCancellationRequested();
                    HttpListenerContext context = await server.GetContextAsync();

                    // Read request from client:
                    string jsonRpc = context.Request.QueryString["jsonrpc"];

                    // Process request to get response:
                    JsonRpcRequest request = JsonRpcRequest.Parse(jsonRpc);
                    JsonRpcResponse response = await serviceClientRequestAsync(request);

                    // Write response to client:
                    var data = Encoding.UTF8.GetBytes(response.ToString());
                    using (context.Response) {
                        await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
                    }
                }
            }
        }

        public readonly string Url;
        public JsonRpcHttpIO(string url) { Url = url; }
        public async Task<JsonRpcResponse> SendAsync(JsonRpcRequest request, CancellationToken cancellationToken) {
            using (var client = new HttpClient()) {
                // Send request to server:
                var response = await client.GetStringAsync(Url + $"?jsonrpc={request.ToString()}").WithCancellation(cancellationToken).ConfigureAwait(false);

                // Get response from server:
                return JsonRpcResponse.Parse(response);
            }
        }
    }
    
#endif
}