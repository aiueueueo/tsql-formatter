# T-SQL Formatter SSMS 22 Uninstall Script
# Run as Administrator

$ErrorActionPreference = "Stop"

# Check if running as Administrator
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Error: Please run this script as Administrator" -ForegroundColor Red
    exit 1
}

# Define possible paths
$SSMSPaths = @(
    "C:\Program Files\Microsoft SQL Server Management Studio 22\Common7\IDE\Extensions\TSqlFormatter",
    "C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions\TSqlFormatter"
)

$Removed = $false

foreach ($TargetDir in $SSMSPaths) {
    if (Test-Path $TargetDir) {
        Write-Host "Removing T-SQL Formatter from: $TargetDir" -ForegroundColor Yellow
        Remove-Item -Path $TargetDir -Recurse -Force
        Write-Host "Removed successfully!" -ForegroundColor Green
        $Removed = $true
    }
}

if (-not $Removed) {
    Write-Host "T-SQL Formatter is not installed" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Please restart SSMS to complete the uninstallation" -ForegroundColor Cyan
