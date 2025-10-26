FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/StockDataWorker/StockDataWorker.csproj", "src/StockDataWorker/"]
COPY ["src/StockDataLib/StockDataLib.csproj", "src/StockDataLib/"]
RUN dotnet restore "src/StockDataWorker/StockDataWorker.csproj"

# Copy all files and build
COPY . .
RUN dotnet build "src/StockDataWorker/StockDataWorker.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "src/StockDataWorker/StockDataWorker.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockDataWorker.dll"]