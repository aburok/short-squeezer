FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/StockDataWorker/StockDataWorker.csproj", "src/StockDataWorker/"]
COPY ["src/StockDataLib/StockDataLib.csproj", "src/StockDataLib/"]
RUN dotnet restore "src/StockDataWorker/StockDataWorker.csproj"
COPY . .
WORKDIR "/src/src/StockDataWorker"
RUN dotnet build "StockDataWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StockDataWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockDataWorker.dll"]