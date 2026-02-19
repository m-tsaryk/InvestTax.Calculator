# Base image for .NET Lambda functions
FROM public.ecr.aws/lambda/dotnet:10 AS base

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Directory.Build.props", "./"]
COPY ["InvestTax.Core/InvestTax.Core.csproj", "InvestTax.Core/"]
COPY ["InvestTax.Infrastructure/InvestTax.Infrastructure.csproj", "InvestTax.Infrastructure/"]
COPY ["InvestTax.Lambda.EmailSender/InvestTax.Lambda.EmailSender.csproj", "InvestTax.Lambda.EmailSender/"]

# Restore dependencies
RUN dotnet restore "InvestTax.Lambda.EmailSender/InvestTax.Lambda.EmailSender.csproj"

# Copy source code
COPY ["InvestTax.Core/", "InvestTax.Core/"]
COPY ["InvestTax.Infrastructure/", "InvestTax.Infrastructure/"]
COPY ["InvestTax.Lambda.EmailSender/", "InvestTax.Lambda.EmailSender/"]

# Build and publish
WORKDIR "/src/InvestTax.Lambda.EmailSender"
RUN dotnet publish "InvestTax.Lambda.EmailSender.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish

# Final stage
FROM base AS final
WORKDIR /var/task
COPY --from=build /app/publish .

# Set the CMD to your handler
CMD ["InvestTax.Lambda.EmailSender::InvestTax.Lambda.EmailSender.Function::FunctionHandler"]
