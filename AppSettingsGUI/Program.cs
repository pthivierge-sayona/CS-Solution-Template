#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace NewApp.Settings.GUI;

internal static class Program
{
    /// <summary>
    ///     Main entry point for the Windows Forms application
    /// </summary>
    [STAThread]
    private static void Main()
    {
        // Create configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting Settings GUI application");

            // Enable modern visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create a host for dependency injection
            var host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Register configuration
                    services.AddSingleton(configuration);
                    
                    // Register forms
                    services.AddTransient<ServiceManager>();
                    
                    // Add other services as needed
                })
                .Build();

            // Get the main form from DI container
            using (host)
            {
                var serviceManager = host.Services.GetRequiredService<ServiceManager>();
                Application.Run(serviceManager);
            }

            Log.Information("Settings GUI application ended normally");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Settings GUI application terminated unexpectedly");
            MessageBox.Show($"A fatal error occurred: {ex.Message}", "Application Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}