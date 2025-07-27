# -------------------------------
# STAGE 1: Build + EF Support
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["OluBackendApp.csproj", "./"]
RUN dotnet restore "OluBackendApp.csproj"

# Copy all project files
COPY . .

# EF needs config at root of build context
COPY appsettings.Production.json ./appsettings.json

# Publish
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app

# -------------------------------
# STAGE 2: Runtime Image
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat (for wait script)
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# Copy published output from build
COPY --from=build /app .

# Copy wait script
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
