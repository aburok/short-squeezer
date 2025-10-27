import { useState, useEffect } from 'react';
import { Line } from 'react-chartjs-2';
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
  const [polygonShortInterestData, setPolygonShortInterestData] = useState([]);
  const [polygonShortVolumeData, setPolygonShortVolumeData] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [isRefreshingAll, setIsRefreshingAll] = useState(false);
  const [isFetchingBlocks, setIsFetchingBlocks] = useState(false);
  const [isFetchingIB, setIsFetchingIB] = useState(false);
  const [isFetchingPolygon, setIsFetchingPolygon] = useState(false);
  const [isFetchingAllPolygon, setIsFetchingAllPolygon] = useState(false);
  const [recentlyViewedTickers, setRecentlyViewedTickers] = useState<string[]>([]);

  const handleTickerSelect = (ticker: string) => {
    setSelectedTicker(ticker);
    setError('');
    
    // Track recently viewed tickers
    if (ticker) {
      setRecentlyViewedTickers(prev => {
        const filtered = prev.filter(t => t !== ticker);
        const updated = [ticker, ...filtered].slice(0, 10); // Keep only last 10
        // Save to localStorage
        localStorage.setItem('recentlyViewedTickers', JSON.stringify(updated));
        return updated;
      });
    }
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

  const handleFetchPolygonData = async () => {
    if (!selectedTicker) {
      setError('Please select a ticker first');
      return;
    }

    setIsFetchingPolygon(true);
    setError('');
    
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
        setError(`Successfully fetched ${result.count} new Polygon data points (2 years) for ${selectedTicker}!`);
        // Refresh the chart data
        fetchData();
      } else {
        setError(result.message || 'Failed to fetch Polygon data');
      }
    } catch (err) {
      setError('Error fetching Polygon data: ' + (err as Error).message);
    } finally {
      setIsFetchingPolygon(false);
    }
  };

  const handleFetchAllPolygonData = async () => {
    if (!selectedTicker) {
      setError('Please select a ticker first');
      return;
    }

    setIsFetchingAllPolygon(true);
    setError('');
    
    try {
      const response = await fetch(
        `/api/Polygon/${selectedTicker}/fetch-all`,
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
        setError(message);
        // Refresh the chart data
        fetchData();
      } else {
        setError(result.error || 'Failed to fetch Polygon data');
      }
    } catch (err) {
      setError('Error fetching Polygon data: ' + (err as Error).message);
    } finally {
      setIsFetchingAllPolygon(false);
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
      
      // Debug logging
      console.log('Borrow Fee API Response:', borrowFeeResult);
      console.log('Borrow Fee Response Length:', borrowFeeResult?.length);
      
      // Transform API response to match chart's expected format
      const transformedBorrowFeeData = borrowFeeResult.map((item: any) => {
        // Convert date to ISO string if needed
        let dateValue = item.date || item.Date;
        if (dateValue && typeof dateValue === 'string') {
          // If date is already a string, use it as-is
          dateValue = dateValue;
        } else if (dateValue) {
          // If date is a Date object, convert to ISO string
          dateValue = new Date(dateValue).toISOString();
        }
        
        return {
          date: dateValue,
          fee: Number(item.fee || item.Fee),
          availableShares: item.availableShares || item.AvailableShares
        };
      });
      
      console.log('Transformed Borrow Fee Data:', transformedBorrowFeeData);
      console.log('Transformed Data Length:', transformedBorrowFeeData?.length);
      
      setBorrowFeeData(transformedBorrowFeeData);

      // Fetch Polygon short interest data
      try {
        const polygonShortInterestResponse = await fetch(
          `/api/Polygon/${selectedTicker}/short-interest?startDate=${startDateStr}&endDate=${endDateStr}`
        );
        if (polygonShortInterestResponse.ok) {
          const polygonShortInterestResult = await polygonShortInterestResponse.json();
          console.log('Polygon Short Interest Data:', polygonShortInterestResult);
          setPolygonShortInterestData(polygonShortInterestResult);
        }
      } catch (err) {
        console.warn('Error fetching Polygon short interest:', err);
      }

      // Fetch Polygon short volume data
      try {
        const polygonShortVolumeResponse = await fetch(
          `/api/Polygon/${selectedTicker}/short-volume?startDate=${startDateStr}&endDate=${endDateStr}`
        );
        if (polygonShortVolumeResponse.ok) {
          const polygonShortVolumeResult = await polygonShortVolumeResponse.json();
          console.log('Polygon Short Volume Data:', polygonShortVolumeResult);
          
          // Transform to match chart format
          const transformedPolygonShortVolume = polygonShortVolumeResult.map((item: any) => ({
            date: item.date || item.Date,
            shortVolume: Number(item.shortVolume || 0),
            totalVolume: Number(item.totalVolume || 0),
            shortVolumePercent: Number(item.shortVolumeRatio || 0)
          }));
          
          setPolygonShortVolumeData(transformedPolygonShortVolume);
        }
      } catch (err) {
        console.warn('Error fetching Polygon short volume:', err);
      }

    } catch (err) {
      setError('Error fetching data: ' + (err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedTicker, dateRange]);

  // Load recently viewed tickers from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem('recentlyViewedTickers');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        setRecentlyViewedTickers(Array.isArray(parsed) ? parsed : []);
      } catch (e) {
        console.error('Error loading recently viewed tickers:', e);
      }
    }
  }, []);

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

            <button 
              onClick={handleFetchPolygonData} 
              disabled={!selectedTicker || isFetchingPolygon}
              className="refresh-all-button"
              style={{ marginLeft: '10px' }}
            >
              {isFetchingPolygon ? 'Fetching...' : 'Fetch Polygon Data (2 Years)'}
            </button>

            <button 
              onClick={handleFetchAllPolygonData} 
              disabled={!selectedTicker || isFetchingAllPolygon}
              className="refresh-all-button"
              style={{ marginLeft: '10px' }}
            >
              {isFetchingAllPolygon ? 'Fetching All...' : 'Fetch All Polygon Data'}
            </button>
          </div>
        </div>

        <div className="date-range-control">
          <MovableDateRangePicker onDateRangeChange={handleDateRangeChange} />
        </div>

        {/* Recently Viewed Tickers */}
        {recentlyViewedTickers.length > 0 && (
          <div className="recently-viewed-section">
            <div className="section-label">Recently Viewed:</div>
            <div className="recent-tickers-list">
              {recentlyViewedTickers.map((ticker, index) => (
                <button
                  key={index}
                  onClick={() => handleTickerSelect(ticker)}
                  className={`recent-ticker-btn ${selectedTicker === ticker ? 'active' : ''}`}
                  title={`View ${ticker}`}
                >
                  {ticker}
                </button>
              ))}
            </div>
          </div>
        )}
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

          {/* Polygon Data Charts */}
          {polygonShortInterestData.length > 0 && (
            <div className="chart-container">
              <h3>Polygon Short Interest - {selectedTicker}</h3>
              <div style={{ height: '300px' }}>
                <Line 
                  data={{
                    labels: polygonShortInterestData.map((item: any) => new Date(item.date || item.Date).toLocaleDateString()),
                    datasets: [{
                      label: 'Short Interest',
                      data: polygonShortInterestData.map((item: any) => Number(item.shortInterest || 0)),
                      borderColor: 'rgb(255, 99, 132)',
                      backgroundColor: 'rgba(255, 99, 132, 0.1)',
                      borderWidth: 2,
                      fill: true,
                    }]
                  }}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                  }}
                />
              </div>
            </div>
          )}

          {polygonShortVolumeData.length > 0 && (
            <div className="chart-container">
              <h3>Polygon Short Volume - {selectedTicker}</h3>
              <div style={{ height: '300px' }}>
                <Line 
                  data={{
                    labels: polygonShortVolumeData.map((item: any) => new Date(item.date || item.Date).toLocaleDateString()),
                    datasets: [{
                      label: 'Short Volume %',
                      data: polygonShortVolumeData.map((item: any) => Number(item.shortVolumePercent || 0)),
                      borderColor: 'rgb(54, 162, 235)',
                      backgroundColor: 'rgba(54, 162, 235, 0.1)',
                      borderWidth: 2,
                      fill: true,
                    }]
                  }}
                  options={{
                    responsive: true,
                    maintainAspectRatio: false,
                  }}
                />
              </div>
            </div>
          )}
          
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
