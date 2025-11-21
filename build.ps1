#!/usr/bin/env pwsh
##########################################################################
# This is the Cake bootstrapper script for PowerShell.
##########################################################################

<#
.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER ShowDescription
Shows description about tasks.
.PARAMETER DryRun
Performs a dry run.
.PARAMETER SkipToolPackageRestore
Skips restoring of packages.
.PARAMETER ScriptArgs
Remaining arguments are added here.
#>

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$SkipToolPackageRestore,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

Write-Host "Preparing to run build script..."

# Determine the .NET SDK version
$DotNetVersion = Get-Content -Path "global.json" -Raw | ConvertFrom-Json | Select-Object -ExpandProperty sdk | Select-Object -ExpandProperty version
Write-Host "Using .NET SDK version: $DotNetVersion"

# Install Cake as a global tool
Write-Host "Installing Cake.Tool..."
dotnet tool restore
if ($LASTEXITCODE -ne 0) {
    dotnet tool install --global Cake.Tool --version 4.0.0
}

# Build arguments
$cakeArguments = @($Script);
if ($Target) { $cakeArguments += "--target=$Target" }
if ($Configuration) { $cakeArguments += "--configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "--verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "--showdescription" }
if ($DryRun) { $cakeArguments += "--dryrun" }
$cakeArguments += $ScriptArgs

# Start Cake
Write-Host "Running build script..."
dotnet cake @cakeArguments
exit $LASTEXITCODE
