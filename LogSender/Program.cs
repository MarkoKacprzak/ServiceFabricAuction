using System;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing;
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
                using (var session = new TraceEventSession("Auction"))
                {
                    Log.Information("Startig ETW collector with sending to Seq server as a log");
                    
                    EtlListenerLog.Register<SFAuction.Svc.ApiGateway.ServiceEventSource>(session);
                    EtlListenerLog.Register<SFAuction.Svc.Auction.ServiceEventSource>(session);

                    /* Uncoment if want to log all ETL
                    session.Source.Dynamic.All+=delegate (TraceEvent data)
                      {
                            // ETW buffers events and only delivers them after buffering up for some amount of time.  Thus 
                            // there is a small delay of about 2-4 seconds between the timestamp on the event (which is very 
                            // accurate), and the time we actually get the event.  We measure that delay here.     
                            var delay = (DateTime.Now - data.TimeStamp).TotalSeconds;
                          Log.Error($"{data.ToString( )} delay:{delay}");
                      };
                      */
                    session.Source.UnhandledEvents += delegate (TraceEvent data)
                    {
                        if ((int) data.ID != 0xFFFE)
                            // The EventSource manifest events show up as unhanded, filter them out.
                        {
                            var delay = (DateTime.Now - data.TimeStamp).TotalSeconds;
                            Log.Error($"{data.Dump()} delay:{delay}");
                        }
                    };

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
                Log.Information("Closing ETW collector");
                Serilog.Log.CloseAndFlush();
            }
            return 0;
        }
        

        /// <summary>
        /// Configures the serilog -> log into ColoredConsole and Seq service and into trace log
        /// </summary>
        static void ConfigureSerilog()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty(nameof(GlobalVariable.Environment), GlobalVariable.Environment)
               // .Enrich.FromLogContext()
                .WriteTo.ColoredConsole()
                        .WriteTo.Seq(GlobalVariable.SeqAddress)
                        .WriteTo.Trace()
                        .CreateLogger();
            Log.Information("ConfigureSerilog Finish");
        }
    }
}
