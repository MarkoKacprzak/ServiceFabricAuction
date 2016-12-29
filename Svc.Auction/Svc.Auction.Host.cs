using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceFabric.Services.Runtime;

namespace SFAuction.Svc.Auction
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("AuctionSvcType",
                    context => new AuctionSvc(context)).GetAwaiter().GetResult();
                /*
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.ColoredConsole()
                    .WriteTo.Seq("http://localhost:5341")
                    .CreateLogger();
                Log.Information("Hello, {Name}!", Environment.UserName);
                */
                /*
                using (var session = new TraceEventSession("MySession2", GlobalName.FileName))
                {
                    session.EnableProvider(GlobalName.ProviderName);
                    session.Source.Kernel.ProcessStart += delegate(ProcessTraceData data)
                    {
                        Log.Information($"{data.ProcessName},{data.CommandLine}");
                    };
                    session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process);
                    session.Source.Process();
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id,
                        typeof(AuctionSvc).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
                */
                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id,
                           typeof(AuctionSvc).Name);
                ServiceEventSource.Current.Message("Hello from AuctionSvc");
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
