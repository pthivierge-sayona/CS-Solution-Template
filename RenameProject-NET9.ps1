$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
#
# SETTINGS TO CHANGE
# Current values are examples.
# You must change ALL of them
# Company Official Name
$ASM_COMPANY="ACME"

#Long product name - give a good description here
$ASM_PRODUCT="ACME super product"

# Service name (short name)
$ASM_SERVICENAME="ACMEservice"
# Service display name
$ASM_SERVICE_DISPLAY_NAME="ACME Super Service"
$ASM_SERVICE_DESCRIPTION="ACME Service that computes XYZ"

# Current year
$YEAR="2025"

# New short name of the application (Short one word name) - used in Namespace and files naming 
# In one word, no space, no special character
$SHORT_NAME="ACME"

#
# END SETTINGS
#

Write-Host "Starting .NET 9 Solution Rename Process..."
Write-Host "Company: $ASM_COMPANY"
Write-Host "Product: $ASM_PRODUCT"
Write-Host "Short Name: $SHORT_NAME"
Write-Host "Service Name: $ASM_SERVICENAME"
Write-Host "Year: $YEAR"
Write-Host ""

# Files excluded from the global search and replace
$excluded = @('.git', '.gitignore', '*.ico', '', '.nuget', '.vs', 'RenameProject.ps1', 'RenameProject-NET9.ps1', '*.nupkg', '*.nuspec', '*.exe', '*.dll', '*.pdb', '*.resx', 'bin', 'obj', 'packages', '.vscode')

# Get all files for text replacement, excluding binaries and temp files
$configFiles = Get-ChildItem $scriptPath -Recurse -File -Exclude $excluded | Where-Object { 
    $_.FullName -notmatch 'packages' -and 
    $_.FullName -notmatch '\\bin\\' -and 
    $_.FullName -notmatch '\\obj\\' -and
    $_.FullName -notmatch '\\.git\\' -and
    $_.FullName -notmatch '\\.vs\\' -and
    $_.Extension -notmatch '\.(exe|dll|pdb|cache|tmp)$'
}

Write-Host "Processing $($configFiles.Count) files for text replacement..."

# Rename text inside files
foreach ($file in $configFiles)
{
    try {
        Write-Host "Processing: $($file.Name)"
        
        # Read file content
        $content = Get-Content $file.PSPath -Raw -ErrorAction Stop
        
        if ($content) {
            # Perform replacements
            $newContent = $content -replace "NewApp", $SHORT_NAME `
                                  -replace "%ASM_COMPANY%", $ASM_COMPANY `
                                  -replace "%ASM_PRODUCT%", $ASM_PRODUCT `
                                  -replace "%YEAR%", $YEAR `
                                  -replace "%ASM_SERVICE_DISPLAY_NAME%", $ASM_SERVICE_DISPLAY_NAME `
                                  -replace "%ASM_SERVICE_DESC%", $ASM_SERVICE_DESCRIPTION `
                                  -replace "NewAppService", $ASM_SERVICENAME
            
            # Write back only if content changed
            if ($newContent -ne $content) {
                Set-Content $file.PSPath -Value $newContent -NoNewline -ErrorAction Stop
                Write-Host "  ✓ Content updated"
            }
        }
    }
    catch {
        Write-Warning "Failed to process file: $($file.FullName) - $($_.Exception.Message)"
    }
}

Write-Host ""
Write-Host "Renaming files and directories..."

# Rename files that contain "NewApp" in their name
$filesToRename = Get-ChildItem $scriptPath -Recurse -File | Where-Object { 
    $_.Name -like "*NewApp*" -and 
    $_.FullName -notmatch '\\bin\\' -and 
    $_.FullName -notmatch '\\obj\\' -and
    $_.FullName -notmatch '\\.git\\' -and
    $_.FullName -notmatch '\\.vs\\'
}

foreach ($file in $filesToRename) {
    try {
        $newName = $file.Name -replace "NewApp", $SHORT_NAME
        $newPath = Join-Path $file.Directory.FullName $newName
        
        if ($file.FullName -ne $newPath) {
            Write-Host "Renaming: $($file.Name) → $newName"
            Rename-Item $file.FullName -NewName $newName -ErrorAction Stop
        }
    }
    catch {
        Write-Warning "Failed to rename file: $($file.FullName) - $($_.Exception.Message)"
    }
}

# Rename directories that contain "NewApp" in their name
$dirsToRename = Get-ChildItem $scriptPath -Recurse -Directory | Where-Object { 
    $_.Name -like "*NewApp*" -and 
    $_.FullName -notmatch '\\.git\\' -and
    $_.FullName -notmatch '\\.vs\\'
} | Sort-Object { $_.FullName.Split('\').Count } -Descending  # Rename deepest directories first

foreach ($dir in $dirsToRename) {
    try {
        $newName = $dir.Name -replace "NewApp", $SHORT_NAME
        $newPath = Join-Path $dir.Parent.FullName $newName
        
        if ($dir.FullName -ne $newPath) {
            Write-Host "Renaming directory: $($dir.Name) → $newName"
            Rename-Item $dir.FullName -NewName $newName -ErrorAction Stop
        }
    }
    catch {
        Write-Warning "Failed to rename directory: $($dir.FullName) - $($_.Exception.Message)"
    }
}

Write-Host ""
Write-Host "Cleaning up temporary files..."

# Remove .new files if they exist
Get-ChildItem $scriptPath -Recurse -File -Filter "*.new" | ForEach-Object {
    Write-Host "Removing temporary file: $($_.Name)"
    Remove-Item $_.FullName -Force
}

# Remove old packages.config files for .NET 9 projects
Get-ChildItem $scriptPath -Recurse -File -Filter "packages.config" | ForEach-Object {
    Write-Host "Removing legacy packages.config: $($_.FullName)"
    Remove-Item $_.FullName -Force
}

# Remove log4net configuration files
Get-ChildItem $scriptPath -Recurse -File -Filter "*.log4net.cfg.xml" | ForEach-Object {
    Write-Host "Removing legacy log4net config: $($_.Name)"
    Remove-Item $_.FullName -Force
}

Write-Host ""
Write-Host "✅ .NET 9 Solution Rename Process Completed Successfully!"
Write-Host ""
Write-Host "📋 Next Steps:"
Write-Host "1. Delete this script: RenameProject.ps1"
Write-Host "2. Delete the old solution file: '$SHORT_NAME - CommandLine and Service.sln'"
Write-Host "3. Use the new solution file: '$SHORT_NAME - NET9.sln'"
Write-Host "4. Delete the .git folder if you want to start fresh"
Write-Host "5. Build the solution: dotnet build '$SHORT_NAME - NET9.sln'"
Write-Host ""
Write-Host "🎉 Your .NET 9 solution is ready to use!"

# Keep the console open to show results
Read-Host "Press Enter to continue..."
