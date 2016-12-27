using System;
using System.Fabric;
using System.Fabric.Description;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace SFAuction.Svc {
   public sealed class HttpCommunicationListener : ICommunicationListener {
      public readonly string PublishedUri;
      private readonly HttpListener _mHttpListener = new HttpListener();
      private readonly Func<HttpListenerContext, CancellationToken, Task> _mProcessRequest;
      private readonly CancellationTokenSource _mProcessRequestsCancellation = new CancellationTokenSource();

      // Url Prefix Strings: https://msdn.microsoft.com/en-us/library/aa364698(v=vs.85).aspx
      public HttpCommunicationListener(string uriPrefix, Func<HttpListenerContext, CancellationToken, Task> processRequest) {
         _mProcessRequest = processRequest;
         PublishedUri = uriPrefix.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
         _mHttpListener.Prefixes.Add(uriPrefix);
      }

      public void Abort() {
         _mProcessRequestsCancellation.Cancel(); _mHttpListener.Abort();
      }

      public Task CloseAsync(CancellationToken cancellationToken) {
         _mProcessRequestsCancellation.Cancel();
         _mHttpListener.Close(); return Task.FromResult(true);//Task.CompletedTask;
      }
      public Task<string> OpenAsync(CancellationToken cancellationToken) {
         _mHttpListener.Start();
         var noWarning = ProcessRequestsAsync(_mProcessRequestsCancellation.Token);
         return Task.FromResult(PublishedUri);
      }
      private async Task ProcessRequestsAsync(CancellationToken processRequests) {
         while (!processRequests.IsCancellationRequested) {
            var request = await _mHttpListener.GetContextAsync();

            // The ContinueWith forces rethrowing the exception if the task fails.
            var noWarning = _mProcessRequest(request, _mProcessRequestsCancellation.Token)
               .ContinueWith(async t => await t /* Rethrow unhandled exception */, TaskContinuationOptions.OnlyOnFaulted);
         }
      }
   }
}
