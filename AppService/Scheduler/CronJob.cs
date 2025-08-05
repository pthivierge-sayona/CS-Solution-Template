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
using System.Collections.Concurrent;

namespace NewApp.Service.Scheduler;

/// <summary>
/// Modern Quartz.NET job implementation using async patterns
/// </summary>
public class CronJob : IJob
{
    private static readonly ConcurrentDictionary<string, Func<Task>> _taskActions = new();
    private readonly ILogger<CronJob> _logger;

    public CronJob(ILogger<CronJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a task action with a name
    /// </summary>
    /// <param name="taskName">Name of the task</param>
    /// <param name="taskAction">Action to execute</param>
    public static void RegisterTaskAction(string taskName, Func<Task> taskAction)
    {
        _taskActions[taskName] = taskAction;
    }

    /// <summary>
    /// Unregister a task action
    /// </summary>
    /// <param name="taskName">Name of the task to unregister</param>
    public static void UnregisterTaskAction(string taskName)
    {
        _taskActions.TryRemove(taskName, out _);
    }

    /// <summary>
    /// Execute the scheduled job
    /// </summary>
    /// <param name="context">Job execution context</param>
    public async Task Execute(IJobExecutionContext context)
    {
        var taskName = context.JobDetail.JobDataMap.GetString("taskName");
        
        if (string.IsNullOrEmpty(taskName))
        {
            _logger.LogError("Task name is missing from job data map");
            return;
        }

        if (!_taskActions.TryGetValue(taskName, out var taskAction))
        {
            _logger.LogError("No task action registered for task: {TaskName}", taskName);
            return;
        }

        try
        {
            _logger.LogInformation("Executing scheduled task: {TaskName}", taskName);
            
            var startTime = DateTime.UtcNow;
            await taskAction();
            var duration = DateTime.UtcNow - startTime;
            
            _logger.LogInformation("Completed scheduled task: {TaskName} in {Duration}ms", 
                taskName, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scheduled task: {TaskName}", taskName);
            
            // Optionally rethrow to let Quartz handle the failure
            throw;
        }
    }
}
