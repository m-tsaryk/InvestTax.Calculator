# Local Development Scripts

This folder contains PowerShell scripts to manage the local development environment with LocalStack.

## Prerequisites

- Docker Desktop (running)
- AWS CLI v2 (for resource initialization)
- PowerShell 5.1 or later

## Scripts

### start-local.ps1

Starts the LocalStack environment and initializes AWS resources.

**Usage:**
```powershell
.\scripts\start-local.ps1
```

**Options:**
- `-SkipInit` - Skip AWS resource initialization

**Example:**
```powershell
# Start with full initialization
.\scripts\start-local.ps1

# Start without initializing resources (faster restart)
.\scripts\start-local.ps1 -SkipInit
```

### stop-local.ps1

Stops the LocalStack environment.

**Usage:**
```powershell
.\scripts\stop-local.ps1
```

**Options:**
- `-RemoveVolumes` - Remove all data volumes (complete cleanup)

**Example:**
```powershell
# Stop but preserve data
.\scripts\stop-local.ps1

# Stop and remove all data
.\scripts\stop-local.ps1 -RemoveVolumes
```

### check-local.ps1

Checks the status of the local development environment.

**Usage:**
```powershell
.\scripts\check-local.ps1
```

This script displays:
- Running Docker containers
- LocalStack service health
- Available S3 buckets
- DynamoDB tables
- Service URLs

## Typical Workflow

### First Time Setup

1. Start the environment:
   ```powershell
   .\scripts\start-local.ps1
   ```

2. Check status:
   ```powershell
   .\scripts\check-local.ps1
   ```

3. Access services:
   - LocalStack Gateway: http://localhost:4566
   - DynamoDB Admin: http://localhost:8001

### Daily Development

**Starting your work day:**
```powershell
.\scripts\start-local.ps1 -SkipInit
```

**Checking what's running:**
```powershell
.\scripts\check-local.ps1
```

**Ending your work day:**
```powershell
.\scripts\stop-local.ps1
```

### Complete Reset

If you need to start fresh:
```powershell
.\scripts\stop-local.ps1 -RemoveVolumes
.\scripts\start-local.ps1
```

## Manual AWS CLI Commands

If you need to interact with LocalStack directly:

**List S3 buckets:**
```powershell
aws --endpoint-url=http://localhost:4566 s3 ls
```

**List DynamoDB tables:**
```powershell
aws --endpoint-url=http://localhost:4566 dynamodb list-tables --region eu-central-1
```

**Scan DynamoDB table:**
```powershell
aws --endpoint-url=http://localhost:4566 dynamodb scan --table-name InvestTax-Jobs-Local --region eu-central-1
```

**Upload file to S3:**
```powershell
aws --endpoint-url=http://localhost:4566 s3 cp test.csv s3://investtax-upload-local/
```

## Troubleshooting

### Docker not running
```
ERROR: Docker is not running!
```
**Solution:** Start Docker Desktop and wait for it to fully start.

### Port conflicts
```
Error: port is already in use
```
**Solution:** Stop other services using ports 4566 or 8001, or modify `docker-compose.yml`.

### AWS CLI not found
```
WARNING: AWS CLI is not installed!
```
**Solution:** Install AWS CLI from https://awscli.amazonaws.com/AWSCLIV2.msi

### LocalStack not responding
```
WARNING: LocalStack may not be fully ready yet
```
**Solution:** Wait a bit longer, then run `.\scripts\check-local.ps1` to verify status.

## Windows-Specific Notes

- These scripts are designed for PowerShell on Windows
- Bash versions (`.sh`) are available in the `infrastructure/` folder for Git Bash users
- Use PowerShell (not Command Prompt) to run these scripts
- You may need to allow script execution:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

## Additional Resources

- LocalStack Documentation: https://docs.localstack.cloud/
- AWS CLI Documentation: https://docs.aws.amazon.com/cli/
- Docker Compose Documentation: https://docs.docker.com/compose/
