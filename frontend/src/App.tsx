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
  const [isFetchingChartExchange, setIsFetchingChartExchange] = useState(false);

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
      const response = await fetch('/api/Finra/blocks-summary/fetch', {
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

  const handleFetchChartExchangeData = async () => {
    if (!selectedTicker) {
      console.error('Please select a ticker first');
      return;
    }

    setIsFetchingChartExchange(true);
    try {
      const response = await fetch(
        `/api/StockData/${selectedTicker}/fetch-chartexchange`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        let message = `ChartExchange data for ${selectedTicker}: `;
        const parts = [];

        if (result.prices.skipped) {
          parts.push(`Prices (skipped - data exists)`);
        } else {
          parts.push(`Prices (${result.prices.fetched} records)`);
        }

        if (result.failureToDeliver.skipped) {
          parts.push(`Failure to Deliver (skipped - data exists)`);
        } else {
          parts.push(`Failure to Deliver (${result.failureToDeliver.fetched} records)`);
        }

        if (result.redditMentions.skipped) {
          parts.push(`Reddit Mentions (skipped - data exists)`);
        } else {
          parts.push(`Reddit Mentions (${result.redditMentions.fetched} records)`);
        }

        if (result.optionChain.skipped) {
          parts.push(`Option Chain (skipped - data exists)`);
        } else {
          parts.push(`Option Chain (${result.optionChain.fetched} records)`);
        }

        if (result.stockSplits.skipped) {
          parts.push(`Stock Splits (skipped - data exists)`);
        } else {
          parts.push(`Stock Splits (${result.stockSplits.fetched} records)`);
        }

        message += parts.join(', ');
        console.log(message);
      } else {
        console.error(result.error || 'Failed to fetch ChartExchange data');
      }
    } catch (err) {
      console.error('Error fetching ChartExchange data: ' + (err as Error).message);
    } finally {
      setIsFetchingChartExchange(false);
    }
  };

  return (
    <Router>
      <div className="App">
        <TopNavigation
          selectedTicker={selectedTicker}
          onTickerSelect={handleTickerSelect}
          onFetchChartExchangeData={handleFetchChartExchangeData}
          onRefreshAllTickers={handleRefreshAllTickers}
          onFetchBlocksSummary={handleFetchBlocksSummary}
          isFetchingChartExchange={isFetchingChartExchange}
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