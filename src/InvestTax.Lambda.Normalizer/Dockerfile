# Base image for .NET Lambda functions
FROM public.ecr.aws/lambda/dotnet:10 AS base

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Directory.Build.props", "./"]
COPY ["InvestTax.Core/InvestTax.Core.csproj", "InvestTax.Core/"]
COPY ["InvestTax.Infrastructure/InvestTax.Infrastructure.csproj", "InvestTax.Infrastructure/"]
COPY ["InvestTax.Lambda.Normalizer/InvestTax.Lambda.Normalizer.csproj", "InvestTax.Lambda.Normalizer/"]

# Restore dependencies
RUN dotnet restore "InvestTax.Lambda.Normalizer/InvestTax.Lambda.Normalizer.csproj"

# Copy source code
COPY ["InvestTax.Core/", "InvestTax.Core/"]
COPY ["InvestTax.Infrastructure/", "InvestTax.Infrastructure/"]
COPY ["InvestTax.Lambda.Normalizer/", "InvestTax.Lambda.Normalizer/"]

# Build and publish
WORKDIR "/src/InvestTax.Lambda.Normalizer"
RUN dotnet publish "InvestTax.Lambda.Normalizer.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish

# Final stage
FROM base AS final
WORKDIR /var/task
COPY --from=build /app/publish .

# Set the CMD to your handler
CMD ["InvestTax.Lambda.Normalizer::InvestTax.Lambda.Normalizer.Function::FunctionHandler"]
