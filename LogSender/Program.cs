using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
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

                    /*
                     var toCheckAssembly = new List<Assembly>();
                     args.Select(
                         param => Directory.EnumerateFiles(
                             path, 
                             $"*{param}", 
                             SearchOption.AllDirectories)
                             .ToList()
                             .FirstOrDefault())
                             .Where(found => found != null)
                             .ToList()
                             .ForEach(name=> toCheckAssembly.Add(
                                 Assembly.LoadFile(name)));

                    var path =
                      Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var a = new List<Assembly>
                    {
                        Assembly.LoadFile(Path.Combine(path, @"..\..\..\Svc.ApiGateway\bin\debug\SFAuction.Svc.ApiGateway.exe")),
                        Assembly.LoadFile(Path.Combine(path, @"..\..\..\Svc.Auction\bin\debug\SFAuction.Svc.Auction.exe"))
                    };
                    
                    a
                        .ToList()
                        .ForEach(asm =>
                         asm.GetTypes()
                        .Where(t => t.BaseType == typeof(EventSource))
                        .Where(t => Attribute.IsDefined(t, typeof(EventSourceAttribute)))
                        .ToList()
                        .ForEach(t =>
                        {
                            typeof(EtlListenerLog).GetMethod("Register").MakeGenericMethod(t).Invoke(null, new object[] { session });
                        }));
                    */
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
