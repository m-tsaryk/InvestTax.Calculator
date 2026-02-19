# GitHub Actions CI/CD Pipeline

This directory contains the automated CI/CD pipeline for the InvestTax Calculator project, following the DevOps Infinity Loop principles.

## Overview

The pipeline implements the complete DevOps lifecycle:

**Plan → Code → Build → Test → Release → Deploy → Operate → Monitor → Plan**

## Workflows

### 1. Build and Test (`build.yml`)

**Trigger**: Push to `main`, `develop`, or `feature/*` branches, PRs to `main`/`develop`

**Purpose**: Continuous Integration - validates code quality, runs tests, builds artifacts

**Jobs**:
- **build-and-test**: Compiles .NET solution, runs unit tests with code coverage
- **build-lambdas**: Builds all Lambda function packages in parallel
- **cdk-synth**: Validates CDK infrastructure code
- **security-scan**: Checks for vulnerable dependencies
- **build-summary**: Aggregates results and reports status

**Artifacts**:
- Test results (TRX format)
- Code coverage reports (OpenCover XML)
- Lambda deployment packages
- CDK CloudFormation templates

**Success Criteria**:
- ✅ All tests pass
- ✅ Code coverage reported (target: 80%)
- ✅ CDK synthesizes without errors
- ✅ No critical security vulnerabilities

### 2. Deploy to AWS (`deploy.yml`)

**Trigger**: Manual workflow dispatch with environment selection

**Purpose**: Continuous Deployment - deploys infrastructure and code to AWS

**Environments**:
- `dev`: Development environment for testing
- `staging`: Pre-production environment
- `prod`: Production environment

**Jobs**:
- **pre-deploy-checks**: Runs tests and validates readiness
- **deploy-infrastructure**: Deploys CDK stack to AWS
- **smoke-tests**: Verifies deployment success
- **deployment-summary**: Reports deployment status

**Required Secrets**:
- `AWS_ACCESS_KEY_ID`: AWS credentials for deployment
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `AWS_ACCOUNT_ID`: AWS account ID for CDK bootstrap

**Usage**:
```bash
# Deploy to dev environment
gh workflow run deploy.yml -f environment=dev

# Deploy to prod (tests cannot be skipped)
gh workflow run deploy.yml -f environment=prod
```

### 3. Monitor and Alert (`monitor.yml`)

**Trigger**: 
- Scheduled (every 6 hours)
- Manual workflow dispatch

**Purpose**: Continuous Monitoring - tracks system health and performance

**Checks**:
- Lambda function health
- DynamoDB table availability
- Step Functions execution statistics
- Recent errors (last 6 hours)
- DORA metrics (Deployment frequency, Lead time, MTTR, Change failure rate)

**Alerts**:
- High error rates (>10 failures in last 100 executions)
- Missing infrastructure components
- Performance degradation

## Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────┐
│ Developer Workflow                                          │
├─────────────────────────────────────────────────────────────┤
│ 1. Code → Push/PR                                           │
│ 2. Build & Test (Automatic)                                 │
│ 3. Code Review + Approval                                   │
│ 4. Merge to main/develop                                    │
│ 5. Deploy (Manual trigger)                                  │
│ 6. Smoke Tests (Automatic)                                  │
│ 7. Monitor (Scheduled)                                      │
└─────────────────────────────────────────────────────────────┘
```

## DORA Metrics Tracking

The pipeline tracks key DevOps Research and Assessment (DORA) metrics:

1. **Deployment Frequency**: How often deployments occur (via CloudFormation history)
2. **Lead Time for Changes**: Time from commit to production (tracked via commit timestamps)
3. **Change Failure Rate**: Percentage of deployments causing failures (via Step Functions stats)
4. **Time to Restore Service**: Time from incident to resolution (via CloudWatch logs)

## Code Coverage

Code coverage is automatically calculated and reported on pull requests:

- **Target**: 80% overall coverage
- **Thresholds**: 
  - 🟢 80%+ (Good)
  - 🟡 60-80% (Acceptable)
  - 🔴 <60% (Needs improvement)

Coverage reports are:
- Posted as PR comments
- Uploaded as workflow artifacts
- Available in OpenCover XML format

## Security Scanning

All dependencies are scanned for known vulnerabilities:

```bash
dotnet list package --vulnerable --include-transitive
```

Warnings are generated for packages with:
- High or critical severity vulnerabilities
- No available patches

## Local Testing

You can test workflows locally using [act](https://github.com/nektos/act):

```bash
# Install act (Windows)
choco install act-cli

# Test build workflow
act push -W .github/workflows/build.yml

# Test with specific event
act pull_request -W .github/workflows/build.yml
```

## Rollback Procedure

If a deployment fails:

1. Navigate to Actions → Deploy to AWS
2. Find the last successful deployment
3. Click "Re-run all jobs"
4. Alternatively, use the rollback script:
   ```bash
   ./scripts/rollback.ps1 -Stage prod -PreviousVersion v1.0.0
   ```

## Troubleshooting

### Build Fails with "Restore failed"
**Cause**: NuGet package caching issues  
**Solution**: Clear cache and re-run
```bash
dotnet nuget locals all --clear
```

### CDK Synth Fails
**Cause**: Node modules not installed or outdated  
**Solution**: Clean install dependencies
```bash
cd infrastructure/cdk
rm -rf node_modules package-lock.json
npm install
```

### Deployment Fails with Permission Error
**Cause**: AWS credentials missing or insufficient permissions  
**Solution**: Verify secrets are set and IAM role has required permissions:
- CloudFormation: Full access
- Lambda: Full access
- S3: Full access
- DynamoDB: Full access
- Step Functions: Full access
- IAM: PassRole permission

### Security Scan Shows Vulnerabilities
**Cause**: Outdated dependencies with known CVEs  
**Solution**: Update packages
```bash
dotnet list package --outdated
dotnet add package <PackageName>
```

## Best Practices

1. **Always run tests** before merging to main
2. **Review code coverage** reports on PRs
3. **Deploy to dev first**, then staging, then prod
4. **Monitor after deployment** for at least 1 hour
5. **Keep dependencies updated** to avoid security issues
6. **Document changes** in commit messages and PR descriptions
7. **Use feature branches** for new development
8. **Tag releases** for easy rollback

## Continuous Improvement

The pipeline itself is continuously improved:

- Monthly review of DORA metrics
- Quarterly security audit
- Performance optimization based on execution times
- Addition of new checks based on production incidents

## Support

For issues with the CI/CD pipeline:
1. Check workflow run logs in GitHub Actions
2. Review troubleshooting section above
3. Check infrastructure/cdk/README.md for CDK-specific issues
4. Contact DevOps team

---

**Last Updated**: February 2026  
**Maintained by**: InvestTax Development Team
