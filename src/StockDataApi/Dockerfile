FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Node.js build stage for React
FROM node:18 AS node-build
WORKDIR /src
COPY ["src/StockDataApi/package.json", "src/StockDataApi/"]
COPY ["src/StockDataApi/webpack.config.js", "src/StockDataApi/"]
WORKDIR "/src/src/StockDataApi"
RUN npm install
COPY ["src/StockDataApi/ClientApp", "src/StockDataApi/ClientApp/"]
RUN mkdir -p wwwroot/js/react
# Force clean build
RUN echo "Building React bundle..."
RUN npm run build
# Verify bundle was created
RUN if [ ! -f "wwwroot/js/react/bundle.js" ]; then echo "Error: bundle.js not created" && exit 1; fi
RUN echo "React bundle built successfully"

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
# Copy React bundle with verification
COPY --from=node-build /src/src/StockDataApi/wwwroot/js/react /app/wwwroot/js/react
RUN echo "Verifying React bundle exists in final image..."
RUN if [ ! -f "/app/wwwroot/js/react/bundle.js" ]; then echo "Error: bundle.js not found in final image" && exit 1; fi
RUN echo "React bundle verified in final image"
ENTRYPOINT ["dotnet", "StockDataApi.dll"]