<#!
.SYNOPSIS
  Clean, restore, build and apply EF Core migrations for eShopOnWeb.
.DESCRIPTION
  Removes all bin/obj folders, restores tools and packages, builds, then applies
  CatalogContext and AppIdentityDbContext migrations against the configured SQL Server.
.PARAMETER SkipMigrations
  If supplied, skips applying EF Core migrations.
.EXAMPLE
  ./build.ps1
.EXAMPLE
  ./build.ps1 -SkipMigrations
#>
param(
  [switch]$SkipMigrations
)

$ErrorActionPreference = 'Stop'
$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$srcRoot    = Join-Path $scriptRoot 'src'
$webDir     = Join-Path $srcRoot 'Web'
$infraProj  = Join-Path (Join-Path $srcRoot 'Infrastructure') 'Infrastructure.csproj'
$webProj    = Join-Path $webDir 'Web.csproj'

function Write-Step($msg){ Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok($msg){ Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warn($msg){ Write-Host "[WARN] $msg" -ForegroundColor Yellow }

Write-Step 'Cleaning bin/obj folders'
Get-ChildItem -Path $srcRoot -Recurse -Directory -Include bin,obj | ForEach-Object {
  try { Remove-Item $_.FullName -Recurse -Force -ErrorAction Stop } catch { Write-Warn "Failed to remove $($_.FullName): $($_.Exception.Message)" }
}
Write-Ok 'Clean complete'

Write-Step 'Restoring packages & tools'
Push-Location $webDir
try {
  dotnet restore | Out-Null
  if (Test-Path '.config/dotnet-tools.json') { dotnet tool restore | Out-Null }
  Write-Ok 'Restore complete'
}
finally { Pop-Location }

Write-Step 'Building solution (Web project triggers others)'
dotnet build $webProj -c Debug --nologo
Write-Ok 'Build complete'

if(-not $SkipMigrations){
  Write-Step 'Applying EF Core migrations (CatalogContext)'
  dotnet ef database update -c CatalogContext -p $infraProj -s $webProj
  Write-Step 'Applying EF Core migrations (AppIdentityDbContext)'
  dotnet ef database update -c AppIdentityDbContext -p $infraProj -s $webProj
  Write-Ok 'Migrations applied'
} else {
  Write-Warn 'Migrations skipped by user request'
}

Write-Ok 'All done.'
