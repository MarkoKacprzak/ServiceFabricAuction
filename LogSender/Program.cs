using System;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing.Session;
using Serilog;

namespace LogSender
{
    class Program
    {
        /// <summary>
        /// Real time ETL listener that collect and send events into Seq service via Serilog service
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            try
            {
                ConfigureSerilog();
                // create a real time user mode session
                using (var session = new TraceEventSession("ApiGateway-AuctionETWSender"))
                {
                    Log.Information("Startig ETW collector with sending to Seq server as a log");
                    EtlListenerLog.Register<SFAuction.Svc.ApiGateway.ServiceEventSource>(session);
                    EtlListenerLog.Register<SFAuction.Svc.Auction.ServiceEventSource>(session);
                    if (!(TraceEventSession.IsElevated() ?? false))
                    {
                        Log.Error(
                            "To turn on ETW events you need to be Administrator, please run from an Admin process.");
                        Debugger.Break();
                        return -1;
                    }
                     // Set up Ctrl-C to stop the session
                    Console.CancelKeyPress +=
                        (object s, ConsoleCancelEventArgs cc) => session.Stop();
                    Console.WriteLine("Press (Ctrl+C) to stop sending ETW to Seq service.");                                      
                    session.Source.Process(); // Listen (forever) for events
                }
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
            return 0;
        }
        private static ILogger Log => Serilog.Log.Logger;

        /// <summary>
        /// Configures the serilog -> log into ColoredConsole and Seq service and into trace log
        /// </summary>
        static void ConfigureSerilog()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty(nameof(GlobalVariable.Environment), GlobalVariable.Environment)
                        .WriteTo.ColoredConsole()
                        .WriteTo.Seq(GlobalVariable.SeqAddress)
                        .WriteTo.Trace()
                        .CreateLogger();
            Log.Information("ConfigureSerilog Finish");
        }
    }
}
