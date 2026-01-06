# T-SQL Formatter SSMS 22 Installation Script
# Run as Administrator

$ErrorActionPreference = "Stop"

# Define paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
$SourceDir = Join-Path $ProjectRoot "src\Plugin\bin\Release\net48"
$PluginSrcDir = Join-Path $ProjectRoot "src\Plugin"
$SSMSExtensionsDir = "C:\Program Files\Microsoft SQL Server Management Studio 22\Common7\IDE\Extensions"
$TargetDir = Join-Path $SSMSExtensionsDir "TSqlFormatter"

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Error: Please run this script as Administrator" -ForegroundColor Red
    exit 1
}

# Check if SSMS extensions directory exists
if (-not (Test-Path $SSMSExtensionsDir)) {
    # Try alternate path
    $SSMSExtensionsDir = "C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions"
    if (-not (Test-Path $SSMSExtensionsDir)) {
        Write-Host "Error: SSMS 22 installation not found" -ForegroundColor Red
        Write-Host "Please check if SSMS 22 is installed and the path is correct" -ForegroundColor Yellow
        exit 1
    }
    $TargetDir = Join-Path $SSMSExtensionsDir "TSqlFormatter"
}

# Check if source directory exists
if (-not (Test-Path $SourceDir)) {
    Write-Host "Error: Build output not found at $SourceDir" -ForegroundColor Red
    Write-Host "Please build the project first: dotnet build src/Plugin/TSqlFormatter.Extension.csproj -c Release" -ForegroundColor Yellow
    exit 1
}

Write-Host "Installing T-SQL Formatter to SSMS 22..." -ForegroundColor Green
Write-Host "Source: $SourceDir" -ForegroundColor Cyan
Write-Host "Target: $TargetDir" -ForegroundColor Cyan

# Remove existing installation if present
if (Test-Path $TargetDir) {
    Write-Host "Removing existing installation..." -ForegroundColor Yellow
    Remove-Item -Path $TargetDir -Recurse -Force
}

# Create target directory
New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null

# Copy required files from bin output
$RequiredFiles = @(
    "TSqlFormatter.Extension.dll",
    "TSqlFormatter.Core.dll",
    "Microsoft.SqlServer.TransactSql.ScriptDom.dll",
    "Newtonsoft.Json.dll"
)

foreach ($file in $RequiredFiles) {
    $sourcePath = Join-Path $SourceDir $file
    if (Test-Path $sourcePath) {
        Write-Host "  Copying $file..." -ForegroundColor Gray
        Copy-Item -Path $sourcePath -Destination $TargetDir -Force
    } else {
        Write-Host "  Warning: $file not found" -ForegroundColor Yellow
    }
}

# Copy pkgdef from source directory
$pkgdefSource = Join-Path $PluginSrcDir "TSqlFormatter.Extension.pkgdef"
if (Test-Path $pkgdefSource) {
    Write-Host "  Copying TSqlFormatter.Extension.pkgdef..." -ForegroundColor Gray
    Copy-Item -Path $pkgdefSource -Destination $TargetDir -Force
} else {
    Write-Host "  Warning: TSqlFormatter.Extension.pkgdef not found" -ForegroundColor Yellow
}

# Create extension.vsixmanifest in target directory
$manifestContent = @"
<?xml version="1.0" encoding="utf-8"?>
<PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011">
  <Metadata>
    <Identity Id="TSqlFormatter.Extension.a1b2c3d4-e5f6-7890-abcd-ef1234567890" Version="1.0.0" Language="en-US" Publisher="T-SQL Formatter" />
    <DisplayName>T-SQL Formatter</DisplayName>
    <Description>A SQL Server Management Studio extension for formatting T-SQL queries.</Description>
  </Metadata>
  <Installation AllUsers="true">
    <InstallationTarget Id="Microsoft.VisualStudio.Ssms" Version="[17.0,)" />
  </Installation>
  <Dependencies>
    <Dependency Id="Microsoft.Framework.NDP" DisplayName="Microsoft .NET Framework" Version="[4.8,)" />
  </Dependencies>
  <Assets>
    <Asset Type="Microsoft.VisualStudio.VsPackage" Path="TSqlFormatter.Extension.pkgdef" />
  </Assets>
</PackageManifest>
"@

$manifestPath = Join-Path $TargetDir "extension.vsixmanifest"
$manifestContent | Out-File -FilePath $manifestPath -Encoding UTF8

# Clear SSMS 22 extension cache
Write-Host "Clearing SSMS 22 extension cache..." -ForegroundColor Yellow
$SSMSCacheDirs = Get-ChildItem "$env:LocalAppData\Microsoft\SSMS" -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -like "22.0*" }
foreach ($cacheDir in $SSMSCacheDirs) {
    $componentModelCache = Join-Path $cacheDir.FullName "ComponentModelCache"
    $extensionsCache = Join-Path $cacheDir.FullName "Extensions"

    if (Test-Path $componentModelCache) {
        Write-Host "  Clearing ComponentModelCache..." -ForegroundColor Gray
        Remove-Item -Path "$componentModelCache\*" -Recurse -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path $extensionsCache) {
        Write-Host "  Clearing Extensions cache..." -ForegroundColor Gray
        Remove-Item -Path "$extensionsCache\*" -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""
Write-Host "Installation completed!" -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANT: Please follow these steps:" -ForegroundColor Yellow
Write-Host "1. Close ALL SSMS windows completely" -ForegroundColor White
Write-Host "2. Wait a few seconds" -ForegroundColor White
Write-Host "3. Start SSMS 22" -ForegroundColor White
Write-Host "4. Look for 'Format T-SQL' in the Tools menu" -ForegroundColor White
Write-Host ""
Write-Host "Shortcut: Ctrl+K" -ForegroundColor Cyan
