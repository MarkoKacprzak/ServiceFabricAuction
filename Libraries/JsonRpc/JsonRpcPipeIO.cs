using System;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFAuction.JsonRpc
{
    public sealed class JsonRpcPipeIO /*, IDisposable */
    {
        public static async Task StartServerAsync(string pipeName, int maxRequestSizeInBytes,
            Func<JsonRpcRequest, Task<JsonRpcResponse>> serviceClientRequestAsync,
            CancellationToken ct = default(CancellationToken))
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                using (var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, -1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous | PipeOptions.WriteThrough))
                {
                    await Task.Factory.FromAsync(pipe.BeginWaitForConnection, pipe.EndWaitForConnection, null);
                    //await pipe.WaitForConnectionAsync();

                    using (pipe)
                    {
                        // Read request from client:
                        var data = new byte[maxRequestSizeInBytes];
                        var bytesRead = await pipe.ReadAsync(data, 0, data.Length, ct);

                        // Process request to get response:
                        var request = JsonRpcRequest.Parse(Encoding.UTF8.GetString(data, 0, bytesRead));
                        var response = await serviceClientRequestAsync(request);

                        // Write response to client:
                        data = Encoding.UTF8.GetBytes(response.ToString());
                        await pipe.WriteAsync(data, 0, data.Length, ct);
                    }
                }
            }
        }

        public readonly string ServerName;
        public readonly string PipeName;
        public readonly int DefaultMaxResponseSizeInBytes;

        public JsonRpcPipeIO(string serverName, string pipeName, int defaultMaxResponseSizeInBytes)
        {
            ServerName = serverName;
            PipeName = pipeName;
            DefaultMaxResponseSizeInBytes = defaultMaxResponseSizeInBytes;
        }

#if false
         private NamedPipeClientStream m_pipe;
         //public void Dispose() { m_pipe.Dispose(); }
         public async Task ConnectAsync(String serverName) {
            m_pipe = new NamedPipeClientStream(serverName, PipeName,
               PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
//            await m_pipe.ConnectAsync(); // Must Connect before setting ReadMode
  //          m_pipe.ReadMode = PipeTransmissionMode.Message;
         }
#endif

        public async Task<JsonRpcResponse> SendAsync(JsonRpcRequest request, int maxResponseSizeInBytes = 0)
        {
            using (var pipe = new NamedPipeClientStream(ServerName, PipeName,
                PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough))
            {
                /*await */
                pipe.Connect /*Async*/(); // Must Connect before setting ReadMode
                pipe.ReadMode = PipeTransmissionMode.Message;

                // Send request to server:
                var data = Encoding.UTF8.GetBytes(request.ToString());
                await pipe.WriteAsync(data, 0, data.Length);

                // Get response from server:
                data = new byte[maxResponseSizeInBytes == 0 ? DefaultMaxResponseSizeInBytes : maxResponseSizeInBytes];
                var bytesRead = await pipe.ReadAsync(data, 0, data.Length);
                return JsonRpcResponse.Parse(Encoding.UTF8.GetString(data, 0, bytesRead));
            }
        }
    }
}