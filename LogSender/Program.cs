using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Session;
using Serilog;

namespace LogSender
{
    class Program
    {
        static int Main(string[] args)
        {
            // create a real time user mode session
            try
            {
                using (var session = new TraceEventSession("ObserveProcs"))
                {
                    if (!(TraceEventSession.IsElevated() ?? false))
                    {
                        Console.WriteLine(
                            "To turn on ETW events you need to be Administrator, please run from an Admin process.");
                        Debugger.Break();
                        return -1;
                    }

                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.ColoredConsole()
                        .WriteTo.Seq("http://localhost:5341")
                        .CreateLogger();


                    // Set up Ctrl-C to stop the session
                    Console.CancelKeyPress +=
                        (object s, ConsoleCancelEventArgs cc) => session.Stop();

                    // Create a stream of process start events.  
                    session.Source.Dynamic.AddCallbackForProviderEvent(
                        SFAuction.Svc.ApiGateway.GlobalName.ProviderName
                        , "Message", delegate(TraceEvent data)
                        {
                            // ETW buffers events and only delivers them after buffering up for some amount of time.  Thus 
                            // there is a small delay of about 2-4 seconds between the timestamp on the event (which is very 
                            // accurate), and the time we actually get the event.  We measure that delay here.     
                            var delay = (DateTime.Now - data.TimeStamp).TotalSeconds;
                            Log.Information($"{data.TimeStamp},{data.PayloadByName("message")} delay:{delay}");
                        });

                    session.Source.Dynamic.AddCallbackForProviderEvent(
                        SFAuction.Svc.Auction.GlobalName.ProviderName
                        , "Message", delegate(TraceEvent data)
                        {
                            // ETW buffers events and only delivers them after buffering up for some amount of time.  Thus 
                            // there is a small delay of about 2-4 seconds between the timestamp on the event (which is very 
                            // accurate), and the time we actually get the event.  We measure that delay here.     
                            var delay = (DateTime.Now - data.TimeStamp).TotalSeconds;
                            Log.Information($"{data.TimeStamp},{data.PayloadByName("message")} delay:{delay}");
                        });
                    // Turn on the process events (includes starts and stops).  
                    session.EnableProvider(SFAuction.Svc.ApiGateway.GlobalName.ProviderName);
                    session.EnableProvider(SFAuction.Svc.Auction.GlobalName.ProviderName);

                    session.Source.Process(); // Listen (forever) for events
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
            return 0;
        }
    }
}
