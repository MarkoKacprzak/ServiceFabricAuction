using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.ServiceFabric.Services.Runtime;

namespace SFAuction.Svc.ApiGateway
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("ApiGatewaySvcType",
                    context => new ApiGatewaySvc(context)).GetAwaiter().GetResult();
                using (var session = new TraceEventSession("MySession1", GlobalName.FileName))
                {
                    session.StopOnDispose = true;
                    session.EnableProvider(GlobalName.ProviderName);
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id,
                        typeof(ApiGatewaySvc).Name);
                    // Today you have to be Admin to turn on ETW events (anyone can write ETW events).   
                    if (!(TraceEventSession.IsElevated() ?? false))
                    {
                        ServiceEventSource.Current.Message("TraceEventSession.IsElevated() error");
                        Console.WriteLine(
                            "To turn on ETW events you need to be Administrator, please run from an Admin process.");
                    }
                }
                ServiceEventSource.Current.Message("Hello from etw");
                // Prevents this host process from terminating so services keep running.
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
