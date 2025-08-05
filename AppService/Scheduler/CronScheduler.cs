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

using Microsoft.Extensions.Logging;
using Quartz;

namespace NewApp.Service.Scheduler;

/// <summary>
/// Modern async scheduler class using Quartz.NET
/// for cron configuration
/// <see cref="https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/crontrigger.html"/>
/// </summary>
public class CronScheduler
{
    private readonly ILogger<CronScheduler> _logger;
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler? _scheduler;

    public CronScheduler(ILogger<CronScheduler> logger, ISchedulerFactory schedulerFactory)
    {
        _logger = logger;
        _schedulerFactory = schedulerFactory;
    }

    /// <summary>
    /// Add a scheduled task with cron expression
    /// </summary>
    /// <param name="taskName">Unique name for the task</param>
    /// <param name="cronExpression">Cron expression (e.g., "0 */5 * * * ?" for every 5 minutes)</param>
    /// <param name="taskAction">The action to execute</param>
    public async Task AddTaskAsync(string taskName, string cronExpression, Func<Task> taskAction)
    {
        try
        {
            _scheduler ??= await _schedulerFactory.GetScheduler();

            // Create job detail
            var jobDetail = JobBuilder.Create<CronJob>()
                .WithIdentity(taskName, "default")
                .UsingJobData("taskName", taskName)
                .Build();

            // Store the task action in a static dictionary for retrieval
            CronJob.RegisterTaskAction(taskName, taskAction);

            // Create trigger with cron schedule
            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{taskName}_trigger", "default")
                .WithCronSchedule(cronExpression)
                .StartNow()
                .Build();

            // Schedule the job
            await _scheduler.ScheduleJob(jobDetail, trigger);
            
            _logger.LogInformation("Scheduled task '{TaskName}' with cron expression '{CronExpression}'", 
                taskName, cronExpression);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add task '{TaskName}' with cron expression '{CronExpression}'", 
                taskName, cronExpression);
            throw;
        }
    }

    /// <summary>
    /// Add a scheduled task with cron expression (synchronous action)
    /// </summary>
    /// <param name="taskName">Unique name for the task</param>
    /// <param name="cronExpression">Cron expression</param>
    /// <param name="taskAction">The synchronous action to execute</param>
    public async Task AddTaskAsync(string taskName, string cronExpression, Action taskAction)
    {
        await AddTaskAsync(taskName, cronExpression, () =>
        {
            taskAction();
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Check if the scheduler is started
    /// </summary>
    public bool IsStarted => _scheduler?.IsStarted ?? false;

    /// <summary>
    /// Start the scheduler
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _scheduler ??= await _schedulerFactory.GetScheduler();
            await _scheduler.Start();
            _logger.LogInformation("Cron Scheduler started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Cron Scheduler");
            throw;
        }
    }

    /// <summary>
    /// Stop the scheduler
    /// </summary>
    public async Task StopAsync()
    {
        try
        {
            if (_scheduler != null)
            {
                _logger.LogInformation("Stopping Cron Scheduler...");
                await _scheduler.Shutdown(waitForJobsToComplete: true);
                _logger.LogInformation("Cron Scheduler stopped successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping Cron Scheduler");
            throw;
        }
    }

    /// <summary>
    /// Remove a scheduled task
    /// </summary>
    /// <param name="taskName">Name of the task to remove</param>
    public async Task RemoveTaskAsync(string taskName)
    {
        try
        {
            if (_scheduler != null)
            {
                var jobKey = new JobKey(taskName, "default");
                var removed = await _scheduler.DeleteJob(jobKey);
                
                if (removed)
                {
                    CronJob.UnregisterTaskAction(taskName);
                    _logger.LogInformation("Removed task '{TaskName}'", taskName);
                }
                else
                {
                    _logger.LogWarning("Task '{TaskName}' was not found", taskName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove task '{TaskName}'", taskName);
            throw;
        }
    }
}