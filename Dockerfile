# -------------------------------
# STAGE 1: Build + EF CLI Support
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Install dotnet-ef CLI tool
RUN dotnet tool install -g dotnet-ef && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.bashrc
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy csproj and restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy the full app source
COPY . .

# Handle optional production config
RUN if [ -f appsettings.Production.json ]; then \
      cp -f appsettings.Production.json appsettings.json; \
    else \
      echo "✅ No production config found. Default appsettings.json will be used."; \
    fi

# Publish to /app
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app

# Label this build stage for reuse (for migrate service)
FROM build AS sdk-image

# -------------------------------
# STAGE 2: Runtime Container
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for wait-for-it
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy published output
COPY --from=build /app .

# Add wait-for-it script
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Expose app port
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
