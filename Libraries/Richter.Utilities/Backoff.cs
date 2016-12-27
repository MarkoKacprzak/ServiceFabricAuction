using System;
using System.Threading;
using System.Threading.Tasks;

namespace Richter.Utilities {
   public struct ExponentialBackoff {
      private readonly int _maxRetries, _delayMilliseconds, _maxDelayMilliseconds;
      private int _retries;
      public ExponentialBackoff(int maxRetries, int delayMilliseconds, int maxDelayMilliseconds) {
         _maxRetries = maxRetries;
         _delayMilliseconds = delayMilliseconds;
         _maxDelayMilliseconds = maxDelayMilliseconds;
         _retries = 0;
      }
      public Task Delay(CancellationToken cancellationToken) {
         if (_retries == _maxRetries)
            throw new TimeoutException("Max retry attempts exceeded.");
            var delay = Math.Min(_delayMilliseconds * (Pow(2, ++_retries) - 1) / 2, _maxDelayMilliseconds);
         return Task.Delay(delay, cancellationToken);
      }
      private static int Pow(int number, int exponent) {
            var result = 1;
         for (var n = 0; n < exponent; n++) result *= number;
         return result;
      }
#if Usage
         ExponentialBackoff backoff = new ExponentialBackoff(3, 10, 100);
         retry:
         try {
            // ...
         }
         catch (TimeoutException) {
            await backoff.Delay(cancellationToken);
            goto retry;
         }
#endif
   }
}
