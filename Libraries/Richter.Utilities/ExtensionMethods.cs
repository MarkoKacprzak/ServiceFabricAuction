//using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
//using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Web.Script.Serialization;

namespace Richter.Utilities {
   public static class Extensions {
      [DebuggerStepThrough]
      public static bool IsNullOrWhiteSpace(this string s) { return string.IsNullOrWhiteSpace(s); }

      public static TimeSpan DefaultToInfinite(this TimeSpan timespan)
         => (timespan == default(TimeSpan)) ? Timeout.InfiniteTimeSpan : timespan;

      public static string RemoveWhiteSpace(this string s) {
         var sb = new StringBuilder(s);
         // We do this backwards for performance & to simplify indexing
         for (var n = sb.Length - 1; n >= 0; n--)
            if (char.IsWhiteSpace(sb[n])) sb.Remove(n, 1);
         return sb.ToString();
      }

      [DebuggerStepThrough]
      public static string Lookup(this NameValueCollection nvc, string key) {
            var value = nvc.Get(key);
         if (value == null) throw new ArgumentException($"Missing URL query parameter '{key}'.");
         return value;
      }
   }
}