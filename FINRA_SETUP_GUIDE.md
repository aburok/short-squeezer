# Steps to Properly Connect to FINRA API

## 1. **Get a FINRA API Key**

### Registration Process:
1. Go to [FINRA API Developer Portal](https://developer.finra.org/)
2. Create an account
3. Complete the API Console entitlement process
4. Generate API credentials
5. **Important**: Make note of your API key - you'll need it for the configuration

## 2. **Update Configuration Files**

You need to update **TWO** configuration files with your FINRA Client ID and Client Secret:

### File 1: `src/StockDataApi/appsettings.json`
```json
{
  "Finra": {
    "ClientId": "YOUR-ACTUAL-FINRA-CLIENT-ID",
    "ClientSecret": "YOUR-ACTUAL-FINRA-CLIENT-SECRET",
    "BaseUrl": "https://api.finra.org"
  }
}
```

### File 2: `src/StockDataWorker/appsettings.json`
```json
{
  "Finra": {
    "ClientId": "YOUR-ACTUAL-FINRA-CLIENT-ID",
    "ClientSecret": "YOUR-ACTUAL-FINRA-CLIENT-SECRET",
    "BaseUrl": "https://api.finra.org"
  },
  "DataFetcher": {
    "FetchIntervalMinutes": 60,
    "DelayBetweenRequestsSeconds": 5,
    "EnableFinraDataFetching": true
  }
}
```

**Replace the placeholders with your actual Client ID and Client Secret from step 1.**

## 3. **Run Database Migration**

The FINRA data table needs to be created in your database:

```bash
# Navigate to the API project directory
cd src/StockDataApi

# Run the database migration
dotnet ef database update --project ../StockDataLib --startup-project ../StockDataApi
```

## 4. **Start the Worker Service**

The worker service needs to be running to fetch FINRA data:

```bash
# In a separate terminal, navigate to the worker project
cd src/StockDataWorker

# Run the worker
dotnet run
```

## 5. **Verify the Configuration**

### Check if Credentials are Loaded:
1. Start the API server
2. Check the console logs - you should see:
   ```
   Successfully obtained OAuth token
   ```

### Test the Endpoint:
```http
GET http://localhost:5000/api/finra/short-interest/AAPL
```

## 6. **Current Status Check**

Your current configuration shows:
- ✅ **API Project**: Has Finra configuration section
- ✅ **Worker Project**: Has Finra configuration section
- ⚠️  **Credentials**: Set to placeholders "your-client-id-here" and "your-client-secret-here" - **NEEDS TO BE CHANGED**
- ✅ **Database Migration**: Already created
- ✅ **Service Integration**: Already implemented with OAuth 2.0 authentication

## 7. **Known Issues & Solutions**

### Issue: "No data available"
- **Cause**: Client ID/Secret not configured or invalid
- **Solution**: Ensure the Client ID and Client Secret in both config files match your actual FINRA credentials

### Issue: "401 Unauthorized"
- **Cause**: Invalid Client ID or Client Secret
- **Solution**: Verify your credentials are correct and active in FINRA's portal

### Issue: "Failed to obtain OAuth token"
- **Cause**: Invalid credentials or network issue
- **Solution**: 
  1. Verify Client ID and Client Secret
  2. Check network connectivity to api.finra.org
  3. Check logs for detailed error messages

## 8. **Data Fetching Behavior**

Once configured:
- **Automatic**: Worker fetches FINRA data every 60 minutes (configurable)
- **Manual**: You can trigger a refresh via the API endpoint
- **Cached**: FINRA data is cached for 30 minutes to reduce API calls

## Quick Setup Checklist

- [ ] Get FINRA Client ID and Client Secret from developer portal
- [ ] Update `appsettings.json` in API project with Client ID and Client Secret
- [ ] Update `appsettings.json` in Worker project with Client ID and Client Secret
- [ ] Run database migration
- [ ] Start Worker service
- [ ] Start API service
- [ ] Test with `GET /api/finra/short-interest/AAPL`

## Important Notes

1. **Credentials Security**: Never commit Client ID and Client Secret to version control. Use environment variables or secure secrets management in production.

2. **OAuth 2.0**: The application uses OAuth 2.0 client credentials flow to obtain access tokens. Tokens are obtained automatically when needed.

3. **Rate Limits**: FINRA API has rate limits. The current configuration includes delays between requests.

4. **Data Updates**: FINRA short interest data is updated twice monthly (mid-month and month-end).

5. **Historical Data**: Available for a rolling 5-year period.

6. **Coverage**: All exchange-listed and OTC equity securities.

## Testing

After configuration, test with:
```bash
curl http://localhost:5000/api/finra/short-interest/AAPL
```

Expected response:
```json
[
  {
    "date": "2024-01-15T00:00:00",
    "settlementDate": "2024-01-15T00:00:00",
    "shortInterest": 15000000,
    "shortInterestPercent": 12.5,
    "marketValue": 2500000000.00,
    "sharesOutstanding": 120000000,
    "avgDailyVolume": 50000000,
    "days2Cover": 0.3
  }
]
```

## Need Help?

If you're still having issues:
1. Check the console logs when starting the API and Worker
2. Verify the API key format is correct
3. Ensure the Worker service is running
4. Check network connectivity to api.finra.org
5. Verify database migration completed successfully
