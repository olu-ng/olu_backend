# ----------------------------------------
# STAGE 1: Build + EF CLI + Migrations
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install dotnet-ef globally for migration support
RUN dotnet tool install --global dotnet-ef

# Add dotnet tools to PATH
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy only .csproj first (layer caching)
COPY ["OluBackendApp.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "OluBackendApp.csproj"

# Copy all source files
COPY . .

# Copy appsettings for EF to work inside container
RUN cp appsettings.Production.json appsettings.json

# OPTIONAL: Uncomment to test migrations in build stage
# RUN dotnet ef migrations add Init --context ApplicationDbContext
# RUN dotnet ef database update --context ApplicationDbContext

# Publish for production
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app

# ----------------------------------------
# STAGE 2: Lightweight Runtime Image
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat (for wait-for-it)
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy app from build stage
COPY --from=build /app .

# Copy wait-for-it script for DB readiness
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Optional: copy runtime config if needed
# COPY appsettings.Production.json ./appsettings.json

# Set environment
ENV ASPNETCORE_URLS=http://+:80

EXPOSE 80

# Final entry point with SQL Server wait
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
