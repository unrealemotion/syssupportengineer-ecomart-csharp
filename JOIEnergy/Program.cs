using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JOIEnergy
{
    // The main entry point for the ASP.NET Core application.
    public class Program
    {
        public static void Main(string[] args)
        {
            // Builds and runs the web host.
            BuildWebHost(args).Run();
        }

        // Builds the web host for the application.
        // Parameters:
        //    args: command line arguments
        // Returns: The web host.
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>() // Specifies the Startup class to use.
                .Build();
    }
}
