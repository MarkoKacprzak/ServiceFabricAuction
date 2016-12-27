using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Richter.Utilities {
   public sealed class CircuitBreakerHttpMessageHandler : DelegatingHandler {
      private readonly int _failuresToOpen;
      private readonly TimeSpan _timeToStayOpen;

      private const int EvictionScanFrequency = 100;
      private readonly TimeSpan _evictionStaleTime = TimeSpan.FromMinutes(1);
      private int _evictionScan = 0;    // 0 to _evictionScanFrequency-1
      private readonly SortedList<Uri, UriCircuitBreaker> _pool =
         new SortedList<Uri, UriCircuitBreaker>(new UriComparer());

      private sealed class UriCircuitBreaker {
         private int _failureCount = 0;
         public DateTime LastAttempt = DateTime.UtcNow;

         public UriCircuitBreaker() { }
         public void ThrowIfOpen(int failuresToOpen, TimeSpan timetoStayOpen) {
            lock (this) {
               if (_failureCount < failuresToOpen) return;
               if (LastAttempt.Add(timetoStayOpen) < DateTime.UtcNow) return;
               throw new InvalidOperationException();
            }
         }
         public void ReportAttempt(bool succeeded, int failuresToOpen) {
            lock (this) {
               LastAttempt = DateTime.UtcNow;
               if (succeeded) _failureCount = 0; // Successful call, reset count
               else {
                  if (_failureCount < failuresToOpen) _failureCount++;   // Threshold reached, open breaker
               }
            }
         }
         public override string ToString() {
            return $"Failures={_failureCount}, Last Attempt={LastAttempt}";
         }
      }

      public CircuitBreakerHttpMessageHandler(int failuresToOpen, TimeSpan timeToStayOpen, HttpMessageHandler innerHandler = null) : base(innerHandler ?? new HttpClientHandler()) {
         _failuresToOpen = failuresToOpen;
         _timeToStayOpen = timeToStayOpen;
      }

      protected override void Dispose(bool disposing) => base.Dispose(disposing);

      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
         var ucb = GetCircuitBreaker(request.RequestUri);
         ucb.ThrowIfOpen(_failuresToOpen, _timeToStayOpen);
         // If we get here, we're closed
         try {
            var t = await base.SendAsync(request, cancellationToken);
            ucb.ReportAttempt(t.IsSuccessStatusCode, _failuresToOpen);
            return t;
         }
         catch (Exception /*ex*/) { ucb.ReportAttempt(false, _failuresToOpen); throw; }
      }

      private UriCircuitBreaker GetCircuitBreaker(Uri uri) {
         uri = new Uri(uri.GetLeftPart(UriPartial.Path));
         Monitor.Enter(_pool);
         if ((_evictionScan = (_evictionScan + 1) % EvictionScanFrequency) == 0) {
            var staleUris = from kvp in _pool
                            where kvp.Value.LastAttempt.Add(_evictionStaleTime) < DateTime.UtcNow
                            select kvp.Key;
            foreach (var staleUri in staleUris) _pool.Remove(staleUri);
         }
         UriCircuitBreaker ucb;
         if (!_pool.TryGetValue(uri, out ucb)) _pool.Add(uri, ucb = new UriCircuitBreaker());
         Monitor.Exit(_pool);
         return ucb;
      }

      private sealed class UriComparer : IComparer<Uri> {
         public int Compare(Uri uri1, Uri uri2) {
            return Uri.Compare(uri1, uri2, UriComponents.SchemeAndServer, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
         }
      }
   }
}