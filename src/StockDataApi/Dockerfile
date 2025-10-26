FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/StockDataApi/StockDataApi.csproj", "src/StockDataApi/"]
COPY ["src/StockDataLib/StockDataLib.csproj", "src/StockDataLib/"]
RUN dotnet restore "src/StockDataApi/StockDataApi.csproj"
COPY . .
WORKDIR "/src/src/StockDataApi"
RUN dotnet build "StockDataApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StockDataApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockDataApi.dll"]