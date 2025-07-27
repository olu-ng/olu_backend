# ----------------------------------------
# STAGE 1: Build from source code in /source
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything inside container /src
COPY . .

# Restore and publish
RUN dotnet restore OluBackendApp.csproj
RUN dotnet publish OluBackendApp.csproj -c Release -o /app

# ----------------------------------------
# STAGE 2: Runtime (use only compiled app from /app)
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for wait-for-it health checks
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy published files from build stage
COPY --from=build /app ./

# Add optional wait-for-it script
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Bind to port 80 in Docker
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Start app after DB is available
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
