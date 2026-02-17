# CDK Helper Scripts for InvestTax Calculator

# Install dependencies
Write-Host "Installing CDK dependencies..." -ForegroundColor Cyan
npm install

# Build TypeScript
Write-Host "`nBuilding TypeScript..." -ForegroundColor Cyan
npm run build

# Synthesize CloudFormation template
Write-Host "`nSynthesizing CloudFormation template..." -ForegroundColor Cyan
npx cdk synth -c stage=dev

Write-Host "`n✓ CDK setup complete!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Build and publish Lambda functions:" -ForegroundColor White
Write-Host "   cd ..\..\src" -ForegroundColor Gray
Write-Host "   dotnet publish -c Release" -ForegroundColor Gray
Write-Host "`n2. Bootstrap CDK (first-time only):" -ForegroundColor White
Write-Host "   npx cdk bootstrap" -ForegroundColor Gray
Write-Host "`n3. Deploy to AWS:" -ForegroundColor White
Write-Host "   npx cdk deploy -c stage=dev" -ForegroundColor Gray
