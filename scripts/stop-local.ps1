# Stop Local Development Environment
# This script stops LocalStack and removes containers

param(
    [switch]$RemoveVolumes = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Stopping Local Development Environment" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Stopping LocalStack containers..." -ForegroundColor Yellow

if ($RemoveVolumes) {
    Write-Host "Removing volumes as well..." -ForegroundColor Yellow
    docker-compose down -v
}
else {
    docker-compose down
}

if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Local environment stopped" -ForegroundColor Green
}
else {
    Write-Host "ERROR: Failed to stop containers" -ForegroundColor Red
    exit 1
}

if ($RemoveVolumes) {
    Write-Host "`nNote: Volumes removed. All LocalStack data has been deleted." -ForegroundColor Yellow
}
else {
    Write-Host "`nNote: Volumes preserved. Data will persist on next start." -ForegroundColor Gray
    Write-Host "To remove volumes, run: .\scripts\stop-local.ps1 -RemoveVolumes" -ForegroundColor Gray
}

Write-Host "`n[OK] Environment stopped successfully`n" -ForegroundColor Green
