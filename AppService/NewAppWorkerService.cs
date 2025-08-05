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

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewApp.Service.Scheduler;

namespace NewApp.Service;

/// <summary>
/// Modern .NET Worker Service implementation
/// </summary>
public class NewAppWorkerService : BackgroundService
{
    private readonly ILogger<NewAppWorkerService> _logger;
    private readonly CronScheduler _scheduler;

    public NewAppWorkerService(ILogger<NewAppWorkerService> logger, CronScheduler scheduler)
    {
        _logger = logger;
        _scheduler = scheduler;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NewApp Service is starting");

        try
        {
            // Initialize and start the scheduler
            await _scheduler.StartAsync();
            
            // Add your scheduled tasks here
            await _scheduler.AddTaskAsync("SampleTask", "0 */5 * * * ?", SampleTaskAction);
            
            _logger.LogInformation("Service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting the service");
            throw;
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NewApp Service is running");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Main service loop - you can add your business logic here
                _logger.LogDebug("Service heartbeat at: {Time}", DateTimeOffset.Now);
                
                // Wait for 30 seconds or until cancellation
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // This is expected when the service is stopping
            _logger.LogInformation("Service execution was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in service execution");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NewApp Service is stopping");

        try
        {
            await _scheduler.StopAsync();
            _logger.LogInformation("Service stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping the service");
        }

        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Sample task action for demonstration
    /// </summary>
    private async Task SampleTaskAction()
    {
        _logger.LogInformation("Executing sample scheduled task at {Time}", DateTimeOffset.Now);
        
        try
        {
            // Add your business logic here
            await Task.Delay(1000); // Simulate work
            
            _logger.LogInformation("Sample task completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing sample task");
        }
    }
}
