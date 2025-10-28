import { useState } from 'react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './App.css';
import Dashboard from './components/Dashboard';
import FinraDashboard from './components/FinraDashboard';
import TopNavigation from './components/TopNavigation';

function App() {
  const [selectedTicker, setSelectedTicker] = useState('');
  const [isRefreshingAll, setIsRefreshingAll] = useState(false);
  const [isFetchingBlocks, setIsFetchingBlocks] = useState(false);
  const [isFetchingPolygon, setIsFetchingPolygon] = useState(false);
  const [isFetchingAllPolygon, setIsFetchingAllPolygon] = useState(false);

  const handleTickerSelect = (ticker: string) => {
    setSelectedTicker(ticker);
  };

  const handleRefreshAllTickers = async () => {
    setIsRefreshingAll(true);
    try {
      const response = await fetch('/api/Tickers/refresh-all', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const result = await response.json();

      if (response.ok && result.success) {
        console.log('Successfully refreshed all tickers');
      } else {
        console.error('Failed to refresh tickers:', result.message);
      }
    } catch (error) {
      console.error('Error refreshing tickers:', error);
    } finally {
      setIsRefreshingAll(false);
    }
  };

  const handleFetchBlocksSummary = async () => {
    setIsFetchingBlocks(true);
    try {
      const response = await fetch('/api/Finra/blocks-summary', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      const result = await response.json();

      if (response.ok && result.success) {
        console.log('Successfully fetched FINRA blocks summary');
      } else {
        console.error('Failed to fetch blocks summary:', result.message);
      }
    } catch (error) {
      console.error('Error fetching blocks summary:', error);
    } finally {
      setIsFetchingBlocks(false);
    }
  };

  const handleFetchPolygonData = async () => {
    if (!selectedTicker) {
      console.error('Please select a ticker first');
      return;
    }

    setIsFetchingPolygon(true);
    try {
      const response = await fetch(
        `/api/Polygon/${selectedTicker}/fetch?years=2`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        console.log(`Successfully fetched ${result.count} new Polygon data points (2 years) for ${selectedTicker}!`);
      } else {
        console.error(result.message || 'Failed to fetch Polygon data');
      }
    } catch (err) {
      console.error('Error fetching Polygon data: ' + (err as Error).message);
    } finally {
      setIsFetchingPolygon(false);
    }
  };

  const handleFetchAllPolygonData = async () => {
    if (!selectedTicker) {
      console.error('Please select a ticker first');
      return;
    }

    setIsFetchingAllPolygon(true);
    try {
      const response = await fetch(
        `/api/StockData/${selectedTicker}/fetch-polygon`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        let message = `Polygon data for ${selectedTicker}: `;
        const parts = [];

        if (result.prices.skipped) {
          parts.push(`Prices (skipped - data exists)`);
        } else {
          parts.push(`Prices (${result.prices.fetched} records)`);
        }

        if (result.shortInterest.skipped) {
          parts.push(`Short Interest (skipped - data exists)`);
        } else {
          parts.push(`Short Interest (${result.shortInterest.fetched} records)`);
        }

        if (result.shortVolume.skipped) {
          parts.push(`Short Volume (skipped - data exists)`);
        } else {
          parts.push(`Short Volume (${result.shortVolume.fetched} records)`);
        }

        message += parts.join(', ');
        console.log(message);
      } else {
        console.error(result.error || 'Failed to fetch Polygon data');
      }
    } catch (err) {
      console.error('Error fetching Polygon data: ' + (err as Error).message);
    } finally {
      setIsFetchingAllPolygon(false);
    }
  };

  return (
    <Router>
      <div className="App">
        <TopNavigation
          selectedTicker={selectedTicker}
          onTickerSelect={handleTickerSelect}
          onFetchPolygonData={handleFetchPolygonData}
          onFetchAllPolygonData={handleFetchAllPolygonData}
          onRefreshAllTickers={handleRefreshAllTickers}
          onFetchBlocksSummary={handleFetchBlocksSummary}
          isFetchingPolygon={isFetchingPolygon}
          isFetchingAllPolygon={isFetchingAllPolygon}
          isRefreshingAll={isRefreshingAll}
          isFetchingBlocks={isFetchingBlocks}
        />

        <Routes>
          <Route path="/" element={<Dashboard selectedTicker={selectedTicker} onTickerSelect={handleTickerSelect} />} />
          <Route path="/finra" element={<FinraDashboard selectedTicker={selectedTicker} onTickerSelect={handleTickerSelect} />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;