using System;
using System.Threading;
using System.Threading.Tasks;

namespace Richter.Utilities {
   public struct ExponentialBackoff {
      private readonly int m_maxRetries, m_delayMilliseconds, m_maxDelayMilliseconds;
      private int m_retries;
      public ExponentialBackoff(int maxRetries, int delayMilliseconds, int maxDelayMilliseconds) {
         m_maxRetries = maxRetries;
         m_delayMilliseconds = delayMilliseconds;
         m_maxDelayMilliseconds = maxDelayMilliseconds;
         m_retries = 0;
      }
      public Task Delay(CancellationToken cancellationToken) {
         if (m_retries == m_maxRetries)
            throw new TimeoutException("Max retry attempts exceeded.");
            var delay = Math.Min(m_delayMilliseconds * (Pow(2, ++m_retries) - 1) / 2, m_maxDelayMilliseconds);
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
