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
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using NewApp.Service.Scheduler;
using Quartz;

namespace NewApp.Service;

internal static class Program
{
    /// <summary>
    ///     Service Main Entry Point - Modern .NET Worker Service
    /// </summary>
    private static async Task Main(string[] args)
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
            Log.Information("NewApp Service starting up");

            var builder = Host.CreateApplicationBuilder(args);
            
            // Configure services
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = configuration["ServiceSettings:ServiceName"] ?? "NewAppService";
            });

            // Add Serilog
            builder.Services.AddSerilog();

            // Add Quartz services
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                // Configure from appsettings.json
                q.UseInMemoryStore();
            });
            builder.Services.AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });

            // Add the worker service
            builder.Services.AddHostedService<NewAppWorkerService>();
            
            // Add your custom services
            builder.Services.AddSingleton<CronScheduler>();

            var host = builder.Build();

            // Handle service install/uninstall for development
            if (Environment.UserInteractive && args.Length > 0)
            {
                string parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        Log.Information("Installing service...");
                        // For .NET 9, use sc.exe or PowerShell commands
                        await InstallServiceAsync(configuration);
                        return;
                    case "--uninstall":
                        Log.Information("Uninstalling service...");
                        await UninstallServiceAsync(configuration);
                        return;
                }
            }

            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Service terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task InstallServiceAsync(IConfiguration configuration)
    {
        var serviceName = configuration["ServiceSettings:ServiceName"] ?? "NewAppService";
        var displayName = configuration["ServiceSettings:DisplayName"] ?? "NewApp Service";
        var description = configuration["ServiceSettings:Description"] ?? "NewApp Service";
        var executablePath = Environment.ProcessPath;

        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sc.exe";
            process.StartInfo.Arguments = $"create \"{serviceName}\" binPath=\"{executablePath}\" DisplayName=\"{displayName}\" start=auto";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                // Set description
                process.StartInfo.Arguments = $"description \"{serviceName}\" \"{description}\"";
                process.Start();
                await process.WaitForExitAsync();
                
                Log.Information("Service installed successfully");
            }
            else
            {
                Log.Error("Failed to install service. Exit code: {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error installing service");
        }
    }

    private static async Task UninstallServiceAsync(IConfiguration configuration)
    {
        var serviceName = configuration["ServiceSettings:ServiceName"] ?? "NewAppService";

        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sc.exe";
            process.StartInfo.Arguments = $"delete \"{serviceName}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Log.Information("Service uninstalled successfully");
            }
            else
            {
                Log.Error("Failed to uninstall service. Exit code: {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error uninstalling service");
        }
    }
}