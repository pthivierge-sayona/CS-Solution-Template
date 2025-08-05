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

using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using NewApp.Core.Helpers;

namespace NewApp.CommandLine;

/// <summary>
///     Command line program "Main"
///     Uses modern .NET hosting and Serilog for logging
/// </summary>
internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Create configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Application starting up");

            // Build and configure command line parser instance
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
            
            return await parserResult.MapResult(
                async (CommandLineOptions opts) => await RunOptionsAsync(opts, configuration),
                async errors => await HandleParseErrorAsync(errors)
            );
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static async Task<int> RunOptionsAsync(CommandLineOptions opts, IConfiguration configuration)
    {
        try
        {
            Log.Information("Command Line Started");
            
            // Create host builder for dependency injection
            var host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(opts);
                    // Add your services here
                })
                .Build();

            using (host)
            {
                // Your code here
                Console.WriteLine("Program running, press any key to stop");
                Console.ReadKey();
                
                Log.Information("Command Line Completed Successfully");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while running the command line application");
            return 1;
        }
    }

    private static async Task<int> HandleParseErrorAsync(IEnumerable<Error> errors)
    {
        Log.Error("Command line parsing failed: {Errors}", string.Join(", ", errors.Select(e => e.ToString())));
        return 1;
    }
}