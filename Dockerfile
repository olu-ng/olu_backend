# -------------------------------
# STAGE 1: Build + EF Support
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Install dotnet-ef inside the container
RUN dotnet tool install -g dotnet-ef && \
    echo 'export PATH="$PATH:/root/.dotnet/tools"' >> /root/.bashrc
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy only the project file first and restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy the rest of the app
COPY . .

# Set up correct config casing
RUN cp -f appsettings.Production.json appsettings.json

# Publish to /app (this will be copied into runtime container)
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app

# -------------------------------
# STAGE 2: Runtime Image
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for wait-for-it
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy published output
COPY --from=build /app .

# Copy wait-for-it script from source (optional if already in source)
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

# Expose app port
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Run with DB wait
ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
