# ----------------------------------------
# STAGE 1: Build the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore OluBackendApp.csproj
RUN dotnet publish OluBackendApp.csproj -c Release -o /app/publish

# ----------------------------------------
# STAGE 2: Run the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Optional: Replace `netcat-traditional` with `netcat-openbsd` if former is unavailable
RUN apt-get update && \
    apt-get install -y netcat-openbsd && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Optional: Add if script isn't executable by default
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
