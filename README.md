# Stock Data Dashboard

A comprehensive stock data analysis platform with separate frontend and backend applications.

## Architecture

This project consists of two separate applications:

1. **Stock Data API** (`src/StockDataApi/`) - .NET 9 Web API backend
2. **Stock Data Frontend** (`stock-data-frontend/`) - React TypeScript frontend

## Quick Start

### 1. Start the API Backend

```bash
# Build and run the API
dotnet run --project src/StockDataApi

# API will be available at http://localhost:5000
# Swagger documentation at http://localhost:5000/swagger
```

### 2. Start the Frontend

```bash
# Navigate to frontend directory
cd stock-data-frontend

# Install dependencies (first time only)
npm install

# Start development server
npm start

# Frontend will be available at http://localhost:3000
```

## Features

### Backend API
- **RESTful API**: Complete stock data API with Swagger documentation
- **Data Sources**: Integration with Chart Exchange, Alpha Vantage, NASDAQ
- **Database**: Entity Framework Core with SQL Server
- **Background Services**: Automated data fetching and ticker updates
- **CORS**: Configured for frontend access

### Frontend Dashboard
- **Interactive Dashboard**: Modern React-based interface
- **Custom Date Picker**: MovableDateRangePicker with timeline slider
- **Data Visualization**: Chart.js integration for charts
- **Real-time Updates**: Live data fetching from API
- **Responsive Design**: Works on all devices

## Project Structure

```
stock-data-api/
├── src/
│   ├── StockDataApi/          # API Backend (.NET 9)
│   ├── StockDataLib/          # Shared Library
│   ├── StockDataWorker/       # Background Worker Service
│   └── TestProjects/          # Test Applications
├── stock-data-frontend/        # React Frontend
├── sql/                        # Database Docker setup
└── docker-compose.yml          # Container orchestration
```

## API Endpoints

- `GET /api/Tickers` - Get all stock tickers
- `GET /api/Tickers/{symbol}` - Get specific ticker details
- `POST /api/Tickers/refresh-all` - Refresh all tickers from exchanges
- `POST /api/Tickers/fetch/{symbol}` - Fetch latest data for ticker
- `GET /api/ShortInterest/{symbol}` - Get short interest data
- `GET /api/ShortVolume/{symbol}` - Get short volume data
- `GET /api/BorrowFee/{symbol}` - Get borrow fee data
- `GET /api/Price/{symbol}` - Get price data

## Development Benefits

### Separated Architecture Advantages:
- **Faster Development**: Frontend changes don't require API rebuilds
- **Independent Scaling**: Scale frontend and backend separately
- **Technology Flexibility**: Use different technologies for each layer
- **Team Collaboration**: Frontend and backend teams can work independently
- **Hot Reloading**: Frontend supports instant updates during development

### Development Workflow:
1. **API Development**: Make changes to API, restart API server
2. **Frontend Development**: Make changes to React components, see instant updates
3. **Testing**: Test both applications independently
4. **Deployment**: Deploy frontend and backend separately

## Configuration

### API Configuration
- Database connection strings in `appsettings.json`
- Alpha Vantage API key configuration
- CORS settings for frontend access

### Frontend Configuration
- API proxy configuration in `package.json`
- Environment variables for API endpoints

## Docker Support

The project includes Docker configuration for:
- SQL Server database
- API application
- Worker service

```bash
# Start all services with Docker Compose
docker-compose up -d
```

## Technologies Used

### Backend
- .NET 9
- Entity Framework Core
- SQL Server
- Swagger/OpenAPI
- Background Services
- HttpClient

### Frontend
- React 18
- TypeScript
- Chart.js
- Moment.js
- CSS Modules

## Contributing

1. Make changes to the appropriate application (API or Frontend)
2. Test changes independently
3. Ensure both applications work together
4. Update documentation as needed

## License

This project is for educational and development purposes.