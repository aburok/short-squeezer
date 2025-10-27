import { useState, useEffect } from 'react';
import TickerSearch from './TickerSearch';
import MovableDateRangePicker from './MovableDateRangePicker';
import ShortInterestChart from './ShortInterestChart';
import ShortVolumeChart from './ShortVolumeChart';
import BorrowFeeChart from './BorrowFeeChart';
import FinraShortInterestChart from './FinraShortInterestChart';

const Dashboard = () => {
  const [selectedTicker, setSelectedTicker] = useState('');
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000), // 30 days ago
    endDate: new Date()
  });
  const [shortInterestData, setShortInterestData] = useState([]);
  const [shortVolumeData, setShortVolumeData] = useState([]);
  const [borrowFeeData, setBorrowFeeData] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [isRefreshingAll, setIsRefreshingAll] = useState(false);
  const [isFetchingBlocks, setIsFetchingBlocks] = useState(false);
  const [isFetchingIB, setIsFetchingIB] = useState(false);

  const handleTickerSelect = (ticker: string) => {
    setSelectedTicker(ticker);
    setError('');
  };

  const handleDateRangeChange = (startDate: Date, endDate: Date) => {
    setDateRange({ startDate, endDate });
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

      if (response.ok) {
        setError('All tickers refreshed successfully!');
        // Reset selected ticker to force refresh
        setSelectedTicker('');
      } else {
        setError('Failed to refresh tickers');
      }
    } catch (err) {
      setError('Error refreshing tickers: ' + (err as Error).message);
    } finally {
      setIsRefreshingAll(false);
    }
  };

  const handleFetchBlocksSummary = async () => {
    setIsFetchingBlocks(true);
    setError('');
    
    try {
      const startDateStr = dateRange.startDate.toISOString().split('T')[0];
      const endDateStr = dateRange.endDate.toISOString().split('T')[0];
      
      const response = await fetch(
        `/api/Finra/blocks-summary/fetch?startDate=${startDateStr}&endDate=${endDateStr}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        setError(`Successfully fetched ${result.count} blocks summary data points!`);
      } else {
        setError(result.message || 'Failed to fetch blocks summary data');
      }
    } catch (err) {
      setError('Error fetching blocks summary: ' + (err as Error).message);
    } finally {
      setIsFetchingBlocks(false);
    }
  };

  const handleFetchIBData = async () => {
    if (!selectedTicker) {
      setError('Please select a ticker first');
      return;
    }

    setIsFetchingIB(true);
    setError('');
    
    try {
      const startDateStr = dateRange.startDate.toISOString().split('T')[0];
      const endDateStr = dateRange.endDate.toISOString().split('T')[0];
      
      const response = await fetch(
        `/api/InteractiveBrokers/${selectedTicker}/fetch?startDate=${startDateStr}&endDate=${endDateStr}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        setError(`Successfully fetched ${result.count} Interactive Brokers data points for ${selectedTicker}!`);
        // Refresh the chart data
        fetchData();
      } else {
        setError(result.message || 'Failed to fetch IB data');
      }
    } catch (err) {
      setError('Error fetching IB data: ' + (err as Error).message);
    } finally {
      setIsFetchingIB(false);
    }
  };

  const fetchData = async () => {
    if (!selectedTicker) return;

    setIsLoading(true);
    setError('');

    try {
      const startDateStr = dateRange.startDate.toISOString().split('T')[0];
      const endDateStr = dateRange.endDate.toISOString().split('T')[0];

      // Fetch short interest data
      const shortInterestResponse = await fetch(
        `/api/ShortInterest/${selectedTicker}?startDate=${startDateStr}&endDate=${endDateStr}`
      );
      const shortInterestResult = await shortInterestResponse.json();
      setShortInterestData(shortInterestResult);

      // Fetch short volume data
      const shortVolumeResponse = await fetch(
        `/api/ShortVolume/${selectedTicker}?startDate=${startDateStr}&endDate=${endDateStr}`
      );
      const shortVolumeResult = await shortVolumeResponse.json();
      setShortVolumeData(shortVolumeResult);

      // Fetch borrow fee data
      const borrowFeeResponse = await fetch(
        `/api/BorrowFee/${selectedTicker}?startDate=${startDateStr}&endDate=${endDateStr}`
      );
      const borrowFeeResult = await borrowFeeResponse.json();
      setBorrowFeeData(borrowFeeResult);

    } catch (err) {
      setError('Error fetching data: ' + (err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedTicker, dateRange]);

  return (
    <div className="dashboard">
      <div className="dashboard-controls">
        <div className="ticker-controls-row">
          <TickerSearch onTickerSelect={handleTickerSelect} />
          
          <div className="button-group">
            <button 
              onClick={fetchData} 
              disabled={!selectedTicker || isLoading}
              className="fetch-button"
            >
              {isLoading ? 'Loading...' : 'Fetch Latest Data'}
            </button>
            
            <button 
              onClick={handleRefreshAllTickers} 
              disabled={isRefreshingAll}
              className="refresh-all-button"
            >
              {isRefreshingAll ? 'Refreshing...' : 'Refresh All Tickers'}
            </button>

            <button 
              onClick={handleFetchBlocksSummary} 
              disabled={isFetchingBlocks}
              className="refresh-all-button"
              style={{ marginLeft: '10px' }}
            >
              {isFetchingBlocks ? 'Fetching...' : 'Fetch FINRA Blocks Summary'}
            </button>

            <button 
              onClick={handleFetchIBData} 
              disabled={!selectedTicker || isFetchingIB}
              className="refresh-all-button"
              style={{ marginLeft: '10px' }}
            >
              {isFetchingIB ? 'Fetching...' : 'Fetch IB Data'}
            </button>
          </div>
        </div>

        <div className="date-range-control">
          <MovableDateRangePicker onDateRangeChange={handleDateRangeChange} />
        </div>
      </div>

      {error && (
        <div className="error-message">
          <p>{error}</p>
        </div>
      )}

      {selectedTicker && (
        <div className="charts-section">
          <div className="chart-container">
            <ShortInterestChart 
              data={shortInterestData} 
              ticker={selectedTicker}
              isLoading={isLoading}
            />
          </div>
          
          <div className="chart-container">
            <ShortVolumeChart 
              data={shortVolumeData} 
              ticker={selectedTicker}
              isLoading={isLoading}
            />
          </div>
          
          <div className="chart-container">
            <BorrowFeeChart 
              data={borrowFeeData} 
              ticker={selectedTicker}
              isLoading={isLoading}
            />
          </div>

          {/* FINRA Data Section */}
          <div className="finra-section">
            <div className="section-header">
              <h2>FINRA Regulatory Data</h2>
              <p>Official short interest data from FINRA regulatory database</p>
            </div>
            <FinraShortInterestChart
              symbol={selectedTicker}
              startDate={dateRange.startDate.toISOString().split('T')[0]}
              endDate={dateRange.endDate.toISOString().split('T')[0]}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default Dashboard;
