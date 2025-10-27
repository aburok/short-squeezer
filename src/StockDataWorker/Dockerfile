FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set up NuGet package cache
ENV NUGET_PACKAGES=/root/.nuget/packages
RUN mkdir -p /root/.nuget/packages

# Copy project files first for better layer caching
COPY ["src/StockDataWorker/StockDataWorker.csproj", "src/StockDataWorker/"]
COPY ["src/StockDataLib/StockDataLib.csproj", "src/StockDataLib/"]
COPY ["Directory.Packages.props", "."]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "src/StockDataWorker/StockDataWorker.csproj" --verbosity quiet

# Copy the rest of the code
COPY . .

WORKDIR "/src/src/StockDataWorker"
RUN dotnet build "StockDataWorker.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "StockDataWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockDataWorker.dll"]