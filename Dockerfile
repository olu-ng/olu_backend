# ----------------------------------------
# STAGE 1: Build the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything into container
COPY . .

# Ensure appsettings.json exists for EF
RUN cp appsettings.Production.json appsettings.json

# Restore & publish
RUN dotnet restore "OluBackendApp.csproj"
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app

# ----------------------------------------
# STAGE 2: Runtime - optimized container
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for wait script
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy built app from previous stage
COPY --from=build /app . 

# Copy wait script
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Copy appsettings.Production.json as appsettings.json
COPY appsettings.Production.json ./appsettings.json

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
