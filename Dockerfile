# ----------------------------------------
# STAGE 1: Build the application
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the source folder
COPY source/ ./
RUN dotnet restore "OluBackendApp.csproj"
RUN dotnet publish "OluBackendApp.csproj" -c Release -o /app/publish

# ----------------------------------------
# STAGE 2: Runtime - use prebuilt artifacts from app/
# ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install netcat for wait-for-it.sh to work
RUN apt-get update && apt-get install -y netcat-traditional && apt-get clean

# ✅ Copy prebuilt binaries from /app (must run build from root: /olu_backend)
COPY app/ .   # ← pulls from actual folder "app/" beside "source/"

# Copy the wait-for-it.sh script from source
COPY source/wait-for-it.sh .
RUN chmod +x wait-for-it.sh

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["./wait-for-it.sh", "sqlserver:1433", "--timeout=60", "--", "dotnet", "OluBackendApp.dll"]
