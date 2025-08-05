# 🔄 Migration Guide: .NET Framework 4.8 → .NET 9

This guide helps you migrate your existing NewApp solution from .NET Framework 4.8 to .NET 9.

## 🎯 Quick Migration Path

If you're starting fresh, simply use the new .NET 9 template. If you have existing customizations, follow this guide.

## 📋 Pre-Migration Checklist

- [ ] ✅ **Backup your current solution**
- [ ] ✅ **Document your customizations** (business logic, scheduled tasks, settings)
- [ ] ✅ **Note any third-party dependencies** not included in the template
- [ ] ✅ **Install .NET 9 SDK**

## 🔧 Step-by-Step Migration

### 1. Project Structure Changes

#### Old Structure (.NET Framework 4.8)
```
NewApp - CommandLine and Service.sln
├── AppCore/
│   ├── NewApp.Core.csproj (old format)
│   └── packages.config
├── AppCmdLine/
│   ├── NewApp.CommandLine.csproj (old format)
│   ├── packages.config
│   └── NewAppCommandLine.log4net.cfg.xml
├── AppService/
│   ├── NewApp.Service.csproj (old format)
│   ├── packages.config
│   └── NewAppService.log4Net.cfg.xml
└── AppSettingsGUI/
    ├── NewApp.Settings.GUI.csproj (old format)
    ├── packages.config
    └── NewAppUI.log4net.cfg.xml
```

#### New Structure (.NET 9)
```
NewApp - NET9.sln
├── AppCore/
│   ├── NewApp.Core.csproj (SDK-style)
│   └── appsettings.json
├── AppCmdLine/
│   ├── NewApp.CommandLine.csproj (SDK-style)
│   └── appsettings.json
├── AppService/
│   ├── NewApp.Service.csproj (Worker Service)
│   ├── appsettings.json
│   └── NewAppWorkerService.cs
└── AppSettingsGUI/
    ├── NewApp.Settings.GUI.csproj (SDK-style)
    └── appsettings.json
```

### 2. Code Changes Required

#### A. Logging Migration (log4net → Serilog)

**Old Code (log4net):**
```csharp
using log4net;

private static readonly ILog _logger = LogManager.GetLogger(typeof(MyClass));

_logger.Info("Information message");
_logger.Error("Error message", exception);
_logger.Debug("Debug message");
```

**New Code (Serilog with DI):**
```csharp
using Microsoft.Extensions.Logging;

private readonly ILogger<MyClass> _logger;

public MyClass(ILogger<MyClass> logger)
{
    _logger = logger;
}

_logger.LogInformation("Information message");
_logger.LogError(exception, "Error message");
_logger.LogDebug("Debug message");

// Structured logging (recommended):
_logger.LogInformation("Processing {ItemCount} items for user {UserId}", count, userId);
```

#### B. Service Migration (ServiceBase → Worker Service)

**Old Code (ServiceBase):**
```csharp
public partial class Service : ServiceBase
{
    protected override void OnStart(string[] args)
    {
        // Start logic
    }

    protected override void OnStop()
    {
        // Stop logic
    }
}
```

**New Code (Worker Service):**
```csharp
public class NewAppWorkerService : BackgroundService
{
    private readonly ILogger<NewAppWorkerService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Your service logic here
            await Task.Delay(1000, stoppingToken);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service starting");
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service stopping");
        await base.StopAsync(cancellationToken);
    }
}
```

#### C. Configuration Migration (XML → JSON)

**Old Configuration (app.config/web.config):**
```xml
<configuration>
  <appSettings>
    <add key="ServiceName" value="NewAppService" />
    <add key="LogLevel" value="Info" />
  </appSettings>
</configuration>
```

**New Configuration (appsettings.json):**
```json
{
  "AppSettings": {
    "ServiceName": "NewAppService"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

#### D. Dependency Injection Setup

**Old Code (Manual instantiation):**
```csharp
var scheduler = new CronScheduler();
var logger = LogManager.GetLogger(typeof(Program));
```

**New Code (DI Container):**
```csharp
// In Program.cs
builder.Services.AddSingleton<CronScheduler>();
builder.Services.AddSingleton<IMyService, MyService>();

// In your class
public MyClass(CronScheduler scheduler, ILogger<MyClass> logger)
{
    _scheduler = scheduler;
    _logger = logger;
}
```

#### E. Async/Await Patterns

**Old Code (Synchronous):**
```csharp
public void ProcessData()
{
    var data = LoadData();
    SaveData(data);
}
```

**New Code (Async):**
```csharp
public async Task ProcessDataAsync(CancellationToken cancellationToken = default)
{
    var data = await LoadDataAsync(cancellationToken);
    await SaveDataAsync(data, cancellationToken);
}
```

### 3. Package Dependencies Migration

#### Old packages.config
```xml
<packages>
  <package id="log4net" version="2.0.12" targetFramework="net48" />
  <package id="CommandLineParser" version="2.8.0" targetFramework="net48" />
  <package id="Quartz" version="3.3.2" targetFramework="net48" />
</packages>
```

#### New PackageReference (in .csproj)
```xml
<ItemGroup>
  <PackageReference Include="Serilog" Version="4.1.0" />
  <PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
  <PackageReference Include="CommandLineParser" Version="2.9.1" />
  <PackageReference Include="Quartz" Version="3.13.1" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.0" />
</ItemGroup>
```

### 4. Scheduler Migration (Quartz.NET)

**Old Code:**
```csharp
public void AddTask(string taskName, string cronConfig, Action action)
{
    var scheduler = factory.GetScheduler().Result;
    var jobDetail = new JobDetailImpl(taskName, typeof(CronTask));
    var trigger = new CronTriggerImpl(taskName, "Group1", cronConfig);
    scheduler.ScheduleJob(jobDetail, trigger);
}
```

**New Code:**
```csharp
public async Task AddTaskAsync(string taskName, string cronExpression, Func<Task> taskAction)
{
    var jobDetail = JobBuilder.Create<CronJob>()
        .WithIdentity(taskName, "default")
        .Build();

    var trigger = TriggerBuilder.Create()
        .WithIdentity($"{taskName}_trigger", "default")
        .WithCronSchedule(cronExpression)
        .Build();

    await _scheduler.ScheduleJob(jobDetail, trigger);
}
```

## 🚨 Breaking Changes to Address

### 1. BinaryFormatter Removal
**Problem:** `BinaryFormatter` is obsolete and removed.
**Solution:** Use `System.Text.Json` for serialization.

### 2. Nullable Reference Types
**Problem:** Compiler warnings about null references.
**Solution:** Add null checks or nullable annotations:
```csharp
public void ProcessItem(string? item)
{
    if (item != null)
    {
        // Process item
    }
}
```

### 3. Implicit Usings
**Effect:** Many `using` statements are now automatic.
**Action:** Remove redundant using statements:
```csharp
// These are now implicit in .NET 9:
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
```

### 4. File-scoped Namespaces
**Old:**
```csharp
namespace MyApp.Core
{
    public class MyClass
    {
    }
}
```

**New:**
```csharp
namespace MyApp.Core;

public class MyClass
{
}
```

## 📝 Migration Checklist

### Code Migration
- [ ] ✅ **Replace log4net with Serilog ILogger**
- [ ] ✅ **Convert synchronous methods to async**
- [ ] ✅ **Update service to use Worker Service pattern**
- [ ] ✅ **Replace BinaryFormatter with System.Text.Json**
- [ ] ✅ **Add dependency injection constructors**
- [ ] ✅ **Update Quartz.NET scheduler code**
- [ ] ✅ **Handle nullable reference types**

### Configuration Migration
- [ ] ✅ **Convert XML config to JSON**
- [ ] ✅ **Update log4net config to Serilog**
- [ ] ✅ **Move connection strings to appsettings.json**
- [ ] ✅ **Update service installation scripts**

### Project Migration
- [ ] ✅ **Convert to SDK-style project files**
- [ ] ✅ **Update package references**
- [ ] ✅ **Remove packages.config files**
- [ ] ✅ **Update solution file**
- [ ] ✅ **Test build and run**

### Testing
- [ ] ✅ **Test command line application**
- [ ] ✅ **Test Windows Service installation**
- [ ] ✅ **Test GUI application**
- [ ] ✅ **Verify logging output**
- [ ] ✅ **Test scheduled tasks**

## 🛠️ Automated Migration

You can use the new `RenameProject-NET9.ps1` script to automatically handle most of the migration:

1. **Backup your project**
2. **Copy the new .NET 9 template files**
3. **Run the migration script:**
   ```powershell
   .\RenameProject-NET9.ps1
   ```
4. **Manually migrate your custom business logic**
5. **Test thoroughly**

## 🔧 Common Issues and Solutions

### Issue: Build Errors with Nullable References
**Solution:** Add nullable annotations or disable nullable context:
```xml
<PropertyGroup>
  <Nullable>disable</Nullable>
</PropertyGroup>
```

### Issue: Service Won't Start
**Solution:** Check the new service installation method:
```bash
# Use sc.exe or PowerShell instead of InstallUtil
sc create "YourService" binPath="C:\path\to\YourService.exe"
```

### Issue: Logging Not Working
**Solution:** Ensure Serilog is properly configured in Program.cs:
```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

### Issue: Scheduled Tasks Not Running
**Solution:** Verify Quartz is registered in DI:
```csharp
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService();
```

## 🎯 Benefits After Migration

- ✅ **Better Performance**: Native .NET 9 performance improvements
- ✅ **Modern Logging**: Structured logging with Serilog
- ✅ **Better Debugging**: Improved diagnostics and error handling
- ✅ **Simplified Deployment**: Single-file deployment options
- ✅ **Future-Proof**: Latest .NET features and security updates
- ✅ **Cross-Platform**: Potential for Linux/macOS support
- ✅ **Better Tooling**: Enhanced Visual Studio/VS Code experience

## 📞 Need Help?

- **Documentation**: Check README-NET9.md for complete setup guide
- **Samples**: Look at the modernized template code
- **Issues**: Create GitHub issues for specific problems

---

**Good luck with your migration to .NET 9! 🚀**
