FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set up NuGet package cache
ENV NUGET_PACKAGES=/root/.nuget/packages
RUN mkdir -p /root/.nuget/packages

# Copy project files first for better layer caching
COPY ["src/StockDataApi/StockDataApi.csproj", "src/StockDataApi/"]
COPY ["src/StockDataLib/StockDataLib.csproj", "src/StockDataLib/"]
COPY ["Directory.Packages.props", "."]

# Restore dependencies (this layer will be cached if project files don't change)
RUN dotnet restore "src/StockDataApi/StockDataApi.csproj" --verbosity quiet

# Copy the rest of the code
COPY . .

WORKDIR "/src/src/StockDataApi"
RUN dotnet build "StockDataApi.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "StockDataApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockDataApi.dll"]