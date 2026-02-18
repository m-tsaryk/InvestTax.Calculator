# Validate State Machine Definition
# This script validates the ASL JSON syntax and checks for common issues

param(
    [string]$DefinitionFile = "infrastructure\cdk\lib\state-machine-definition.json"
)

$ErrorActionPreference = "Stop"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "State Machine Definition Validator" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$DefinitionPath = Join-Path $RootDir $DefinitionFile

if (-not (Test-Path $DefinitionPath)) {
    Write-Host "Error: Definition file not found: $DefinitionPath" -ForegroundColor Red
    exit 1
}

Write-Host "Reading definition from: $DefinitionPath" -ForegroundColor Gray
Write-Host ""

# Load and parse JSON
try {
    $definition = Get-Content $DefinitionPath -Raw | ConvertFrom-Json
    Write-Host "[OK] JSON syntax is valid" -ForegroundColor Green
}
catch {
    Write-Host "[ERROR] JSON syntax error:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Validation checks
$issues = @()
$warnings = @()

# Check required top-level fields
Write-Host ""
Write-Host "Checking required fields..." -ForegroundColor Yellow

if (-not $definition.Comment) {
    $warnings += "Missing 'Comment' field (optional but recommended)"
}

if (-not $definition.StartAt) {
    $issues += "Missing required 'StartAt' field"
}
else {
    Write-Host "  [OK] StartAt: $($definition.StartAt)" -ForegroundColor Green
}

if (-not $definition.States) {
    $issues += "Missing required 'States' field"
}
else {
    $stateCount = ($definition.States | Get-Member -MemberType NoteProperty).Count
    Write-Host "  [OK] States: $stateCount defined" -ForegroundColor Green
}

# Check that StartAt state exists
if ($definition.StartAt -and $definition.States) {
    $startState = $definition.States.PSObject.Properties[$definition.StartAt]
    if (-not $startState) {
        $issues += "StartAt state '$($definition.StartAt)' not found in States"
    }
    else {
        Write-Host "  [OK] StartAt state exists" -ForegroundColor Green
    }
}

# Check for terminal states
Write-Host ""
Write-Host "Checking for terminal states..." -ForegroundColor Yellow
$hasSucceed = $false
$hasFail = $false

foreach ($stateName in $definition.States.PSObject.Properties.Name) {
    $state = $definition.States.$stateName
    if ($state.Type -eq "Succeed") {
        $hasSucceed = $true
        Write-Host "  [OK] Found Succeed state: $stateName" -ForegroundColor Green
    }
    if ($state.Type -eq "Fail") {
        $hasFail = $true
        Write-Host "  [OK] Found Fail state: $stateName" -ForegroundColor Green
    }
}

if (-not $hasSucceed) {
    $warnings += "No Succeed state found (may be intentional)"
}

if (-not $hasFail) {
    $warnings += "No Fail state found (error handling may be incomplete)"
}

# Check for unreachable states
Write-Host ""
Write-Host "Checking for unreachable states..." -ForegroundColor Yellow
$reachableStates = @($definition.StartAt)
$checkedStates = @()

while ($reachableStates.Count -gt $checkedStates.Count) {
    foreach ($stateName in $reachableStates) {
        if ($stateName -in $checkedStates) {
            continue
        }
        
        $checkedStates += $stateName
        $state = $definition.States.$stateName
        
        if (-not $state) {
            continue
        }
        
        # Add next states
        if ($state.Next -and $state.Next -notin $reachableStates) {
            $reachableStates += $state.Next
        }
        
        # Add catch next states
        if ($state.Catch) {
            foreach ($catcher in $state.Catch) {
                if ($catcher.Next -and $catcher.Next -notin $reachableStates) {
                    $reachableStates += $catcher.Next
                }
            }
        }
        
        # Add choice branches
        if ($state.Type -eq "Choice" -and $state.Choices) {
            foreach ($choice in $state.Choices) {
                if ($choice.Next -and $choice.Next -notin $reachableStates) {
                    $reachableStates += $choice.Next
                }
            }
        }
        
        # Add default choice
        if ($state.Default -and $state.Default -notin $reachableStates) {
            $reachableStates += $state.Default
        }
    }
}

$allStates = $definition.States.PSObject.Properties.Name
$unreachableStates = $allStates | Where-Object { $_ -notin $reachableStates }

if ($unreachableStates.Count -gt 0) {
    foreach ($stateName in $unreachableStates) {
        $warnings += "Unreachable state: $stateName"
    }
}
else {
    Write-Host "  [OK] All states are reachable" -ForegroundColor Green
}

# Check for placeholders
Write-Host ""
Write-Host "Checking for unresolved placeholders..." -ForegroundColor Yellow
$definitionText = Get-Content $DefinitionPath -Raw
$placeholders = [regex]::Matches($definitionText, '\$\{([^}]+)\}')

if ($placeholders.Count -gt 0) {
    Write-Host "  Found $($placeholders.Count) placeholders:" -ForegroundColor Yellow
    foreach ($placeholder in $placeholders) {
        Write-Host "    - $($placeholder.Value)" -ForegroundColor Gray
    }
    Write-Host "  Note: Placeholders will be replaced during CDK deployment" -ForegroundColor Gray
}
else {
    Write-Host "  [OK] No placeholders found (already substituted)" -ForegroundColor Green
}

# Summary
Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Validation Summary" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -eq 0) {
    Write-Host "[OK] No critical issues found" -ForegroundColor Green
}
else {
    Write-Host "[ERROR] Found $($issues.Count) critical issue(s):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  - $issue" -ForegroundColor Red
    }
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "[WARNING] Found $($warnings.Count) warning(s):" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "  - $warning" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "State Machine Statistics:" -ForegroundColor White
Write-Host "  Total States: $($allStates.Count)" -ForegroundColor Gray
Write-Host "  Reachable States: $($reachableStates.Count)" -ForegroundColor Gray

$lambdaCount = ($definition.States.PSObject.Properties.Value | Where-Object { $_.Resource -like '*lambda:invoke*' }).Count
$dynamoCount = ($definition.States.PSObject.Properties.Value | Where-Object { $_.Resource -like '*dynamodb:*' }).Count
$choiceCount = ($definition.States.PSObject.Properties.Value | Where-Object { $_.Type -eq 'Choice' }).Count

Write-Host "  Lambda Invocations: $lambdaCount" -ForegroundColor Gray
Write-Host "  DynamoDB Operations: $dynamoCount" -ForegroundColor Gray
Write-Host "  Choice States: $choiceCount" -ForegroundColor Gray
Write-Host ""

if ($issues.Count -gt 0) {
    exit 1
}
