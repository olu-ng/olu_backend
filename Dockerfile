# ----------------------------------------
# STAGE 1: Build the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "OluBackendApp.csproj"
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# ----------------------------------------
# STAGE 2: Runtime - use prebuilt artifacts from ../app
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for healthchecks or wait scripts
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# COPY build output from ../app/ folder (outside Docker context)
# Note: You must run Docker from the root (olu_backend/) so ../app is accessible

COPY ../app/ .      # <- This is the key change: pulling prebuilt DLLs into container

# Copy wait-for-it script from source dir
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
