# CS-Solution-Template (.NET 9 Modernized)

This is a modernized **full C# Solution Template** upgraded to **.NET 9** with **Worker Service**, **Command Line**, and **Settings Editor**. The solution has been completely refactored to use modern .NET practices including **Serilog**, **dependency injection**, **async/await patterns**, and the **Generic Host**.

## 🚀 What's New in .NET 9 Version

### Major Upgrades
- ✅ **Upgraded from .NET Framework 4.8 to .NET 9**
- ✅ **Replaced log4net with Serilog** (structured logging)
- ✅ **Modern SDK-style project files** with PackageReference
- ✅ **Dependency Injection** throughout the solution
- ✅ **Async/Await patterns** for all I/O operations
- ✅ **Generic Host** for service and command line applications
- ✅ **Worker Service pattern** instead of legacy Windows Service
- ✅ **JSON configuration** instead of XML app.config
- ✅ **Modern C# features**: nullable reference types, file-scoped namespaces, implicit usings
- ✅ **Updated Quartz.NET** scheduler with modern async patterns
- ✅ **System.Text.Json** instead of BinaryFormatter

### Architecture Improvements
- **Structured Logging**: Serilog with file rolling, console, and event log sinks
- **Configuration**: JSON-based configuration with strongly-typed options
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection
- **Hosting**: Uses Microsoft.Extensions.Hosting for both service and command line
- **Async Patterns**: All I/O operations are async for better performance
- **Modern Serialization**: JSON-based serialization for security and cross-platform compatibility

## 🏗️ Project Structure

```
NewApp - NET9.sln                    # Modern .NET 9 solution file
├── AppCore/                         # Core business logic library
│   ├── NewApp.Core.csproj          # .NET 9 SDK-style project
│   ├── appsettings.json            # Serilog configuration
│   ├── Helpers/
│   │   ├── ExtensionMethods.cs     # String/DateTime extensions
│   │   ├── General.cs              # General utilities
│   │   └── Serializer.cs           # Modern JSON serialization
│   └── SettingsMgmt/               # Settings management
├── AppCmdLine/                      # Command line application
│   ├── NewApp.CommandLine.csproj   # .NET 9 console app
│   ├── appsettings.json            # App-specific configuration
│   ├── Program.cs                  # Modern async Main with hosting
│   └── CommandLineOptions.cs       # CommandLineParser options
├── AppService/                      # Windows Service (Worker Service)
│   ├── NewApp.Service.csproj       # .NET 9 Worker Service
│   ├── appsettings.json            # Service configuration
│   ├── Program.cs                  # Modern service host
│   ├── NewAppWorkerService.cs      # Background service implementation
│   └── Scheduler/
│       ├── CronScheduler.cs        # Modern async scheduler
│       └── CronJob.cs              # Quartz job implementation
├── AppSettings/                     # Settings library
│   ├── NewApp.Settings.csproj      # Configuration models
│   └── (Settings classes)
└── AppSettingsGUI/                  # Windows Forms GUI
    ├── NewApp.Settings.GUI.csproj  # .NET 9 Windows Forms
    ├── appsettings.json            # GUI configuration
    ├── Program.cs                  # Modern Forms host with DI
    └── ServiceManager.cs           # Main form with Serilog
```

## 🛠️ Getting Started

### Prerequisites
- **.NET 9 SDK** or later
- **Visual Studio 2022** (17.8+) or **VS Code** with C# extension
- **Windows** (for Windows Service and Forms GUI)

### Quick Setup
1. **Download/Clone** the repository
2. **Customize** the `RenameProject.ps1` script:
   ```powershell
   # Edit these variables in RenameProject.ps1
   $ASM_COMPANY="YourCompany"
   $ASM_PRODUCT="Your Product Name"
   $ASM_SERVICENAME="YourService"
   $SHORT_NAME="YourApp"
   $YEAR="2025"
   ```
3. **Run** the rename script:
   ```powershell
   # Right-click and "Run with PowerShell"
   ./RenameProject.ps1
   ```
4. **Open** `NewApp - NET9.sln` in Visual Studio or VS Code
5. **Build** the solution:
   ```bash
   dotnet build
   ```

## 📋 How to Use

### Command Line Application
```bash
# Build and run
dotnet run --project AppCmdLine

# With parameters
dotnet run --project AppCmdLine -- --help
```

### Windows Service
```bash
# Build
dotnet publish AppService -c Release -r win-x64

# Install service (run as Administrator)
.\AppService\bin\Release\net9.0\win-x64\publish\NewAppService.exe --install

# Start service
sc start NewAppService

# Uninstall service
.\NewAppService.exe --uninstall
```

### Settings GUI
```bash
# Run the settings manager
dotnet run --project AppSettingsGUI
```

## 🔧 Configuration

### Serilog Configuration (appsettings.json)
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/YourApp-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Scheduled Tasks (Quartz.NET)
```csharp
// In NewAppWorkerService.cs
await _scheduler.AddTaskAsync("MyTask", "0 */5 * * * ?", MyTaskAction);

// Cron expressions:
// "0 */5 * * * ?" - Every 5 minutes
// "0 0 */2 * * ?" - Every 2 hours
// "0 0 0 * * ?"   - Daily at midnight
```

## 🧰 Development Tips

### Adding Business Logic
1. **Core Logic**: Add to `AppCore` project
2. **Scheduled Tasks**: Add to `NewAppWorkerService.cs`
3. **CLI Commands**: Extend `CommandLineOptions.cs`
4. **Configuration**: Update `appsettings.json`

### Logging Best Practices
```csharp
// Use structured logging
_logger.LogInformation("Processing {ItemCount} items for {UserId}", count, userId);

// Log exceptions with context
_logger.LogError(ex, "Failed to process item {ItemId}", itemId);

// Use appropriate log levels
_logger.LogDebug("Debug info");      // Development only
_logger.LogInformation("Info");      // General information
_logger.LogWarning("Warning");       // Unexpected but recoverable
_logger.LogError("Error");           // Errors requiring attention
```

### Modern C# Features Used
- **File-scoped namespaces**: `namespace MyApp.Core;`
- **Nullable reference types**: `string? optionalValue`
- **Implicit usings**: Common namespaces included automatically
- **Record types**: For configuration models
- **Pattern matching**: Enhanced switch expressions
- **Async enumerable**: `IAsyncEnumerable<T>`

## 📦 Dependencies

### Core Packages
- **Serilog** - Structured logging
- **Microsoft.Extensions.Hosting** - Generic Host
- **Microsoft.Extensions.DependencyInjection** - DI container
- **Microsoft.Extensions.Configuration** - Configuration system

### Service-Specific
- **Quartz** - Job scheduling
- **Microsoft.Extensions.Hosting.WindowsServices** - Windows Service hosting

### Command Line
- **CommandLineParser** - CLI argument parsing

### GUI
- **System.Windows.Forms** - Windows Forms (with .NET 9 improvements)

## 🔄 Migration from Legacy Version

### Breaking Changes
1. **Namespace changes**: File-scoped namespaces
2. **Logging**: log4net → Serilog (different API)
3. **Configuration**: XML → JSON
4. **Serialization**: BinaryFormatter → System.Text.Json
5. **Service**: System.ServiceProcess → Worker Service

### Migration Steps
1. **Update project references** to new project files
2. **Replace log4net calls** with Serilog ILogger
3. **Convert XML config** to JSON appsettings
4. **Update using statements** (many are now implicit)
5. **Add nullable annotations** where appropriate

## 📜 License

    Copyright 2025 Patrice Thivierge Fortin
 
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
 
    http://www.apache.org/licenses/LICENSE-2.0
 
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.

## 🤝 Contributing

This is a template project. Feel free to:
- Report issues
- Suggest improvements
- Submit pull requests
- Use as a base for your own projects

---

**Happy coding with .NET 9! 🚀**
