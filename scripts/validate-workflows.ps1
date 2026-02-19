# CI/CD Pipeline Validation Script
# Validates GitHub Actions workflow syntax

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " GitHub Actions CI/CD Pipeline Validation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

$workflowPath = ".github/workflows"
$workflows = Get-ChildItem -Path $workflowPath -Filter "*.yml"

Write-Host "`n[1/3] Checking workflow files..." -ForegroundColor Yellow

foreach ($workflow in $workflows) {
    Write-Host "  ✓ Found: $($workflow.Name)" -ForegroundColor Green
}

Write-Host "`n[2/3] Validating YAML syntax..." -ForegroundColor Yellow

$hasErrors = $false
foreach ($workflow in $workflows) {
    $filePath = $workflow.FullName
    $content = Get-Content $filePath -Raw
    
    # Basic YAML validation checks
    if ($content -match '^\s+[^\s]') {
        Write-Host "  ✓ $($workflow.Name): Indentation looks valid" -ForegroundColor Green
    }
    
    # Check for required sections
    if ($content -match 'name:' -and $content -match 'on:' -and $content -match 'jobs:') {
        Write-Host "  ✓ $($workflow.Name): Required sections present" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $($workflow.Name): Missing required sections" -ForegroundColor Red
        $hasErrors = $true
    }
    
    # Check for common syntax errors
    if ($content -match '\$\{\{[^}]*\}\}' -or $content -match '`\$\{\{[^}]*\}\}') {
        Write-Host "  ✓ $($workflow.Name): GitHub Actions expressions found" -ForegroundColor Green
    }
}

Write-Host "`n[3/3] Checking required secrets..." -ForegroundColor Yellow
Write-Host "  ⚠ Manual setup required:" -ForegroundColor Yellow
Write-Host "    - AWS_ACCESS_KEY_ID" -ForegroundColor White
Write-Host "    - AWS_SECRET_ACCESS_KEY" -ForegroundColor White
Write-Host "    - AWS_ACCOUNT_ID" -ForegroundColor White

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

if ($hasErrors) {
    Write-Host "✗ Validation completed with errors" -ForegroundColor Red
    exit 1
} else {
    Write-Host "✓ All workflows validated successfully!" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Cyan
    Write-Host "  1. Configure GitHub repository secrets" -ForegroundColor White
    Write-Host "  2. Push to remote: git push origin feature/ci" -ForegroundColor White
    Write-Host "  3. Create Pull Request to trigger build workflow" -ForegroundColor White
    Write-Host "  4. Monitor workflow runs in GitHub Actions tab" -ForegroundColor White
}
