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
  const [isFetchingPolygon, setIsFetchingPolygon] = useState(false);
  const [isFetchingAllPolygon, setIsFetchingAllPolygon] = useState(false);
  const [recentlyViewedTickers, setRecentlyViewedTickers] = useState<string[]>([]);
  const [showActionsMenu, setShowActionsMenu] = useState(false);

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

      // Don't fetch FINRA data, only Polygon data
      // Set empty arrays for non-Polygon data
      setShortInterestData([]);
      setShortVolumeData([]);

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

      // Fetch all Polygon data from unified endpoint
      try {
        const stockDataResponse = await fetch(
          `/api/StockData/${selectedTicker}?startDate=${startDateStr}&endDate=${endDateStr}&includePolygon=true&includeBorrowFee=false`
        );
        if (stockDataResponse.ok) {
          const stockData = await stockDataResponse.json();
          console.log('Stock Data Response:', stockData);
          
          // Extract Polygon short interest data
          if (stockData.polygonData?.shortInterestData) {
            setPolygonShortInterestData(stockData.polygonData.shortInterestData);
          }
          
          // Extract Polygon short volume data
          if (stockData.polygonData?.shortVolumeData) {
            // Transform to match chart format
            const transformedPolygonShortVolume = stockData.polygonData.shortVolumeData.map((item: any) => ({
              date: item.date || item.Date,
              shortVolume: Number(item.shortVolume || 0),
              totalVolume: Number(item.totalVolume || 0),
              shortVolumePercent: Number(item.shortVolumeRatio || 0)
            }));
            
            setPolygonShortVolumeData(transformedPolygonShortVolume);
          }
        }
      } catch (err) {
        console.warn('Error fetching Polygon data:', err);
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
        {/* Combined Row 1: Ticker Search + Recently Viewed + Actions Menu */}
        <div className="combined-controls-row">
          {/* Column 1: Ticker Search (20%) */}
          <div className="ticker-search-col">
            <TickerSearch onTickerSelect={handleTickerSelect} />
          </div>

          {/* Column 2: Recently Viewed (40%) */}
          <div className="recently-viewed-col">
            {recentlyViewedTickers.length > 0 && (
              <div className="recently-viewed-inline">
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

          {/* Column 3: Actions Dropdown Menu (40%) */}
          <div className="actions-menu-col">
            <div className="actions-dropdown">
              <button 
                className="actions-button"
                onClick={() => setShowActionsMenu(!showActionsMenu)}
              >
                Actions {showActionsMenu ? '^' : 'v'}
              </button>
              
              {showActionsMenu && (
                <div className="actions-menu">
                  <button 
                    onClick={() => {
                      setShowActionsMenu(false);
                      handleFetchPolygonData();
                    }}
                    disabled={!selectedTicker || isFetchingPolygon}
                    className="actions-menu-item"
                  >
                    {isFetchingPolygon ? 'Fetching...' : 'Fetch Polygon Price Data'}
                  </button>
                  <button 
                    onClick={() => {
                      setShowActionsMenu(false);
                      handleFetchAllPolygonData();
                    }}
                    disabled={!selectedTicker || isFetchingAllPolygon}
                    className="actions-menu-item"
                  >
                    {isFetchingAllPolygon ? 'Fetching All...' : 'Fetch All Polygon Data'}
                  </button>
                  <button 
                    onClick={() => {
                      setShowActionsMenu(false);
                      handleRefreshAllTickers();
                    }}
                    disabled={isRefreshingAll}
                    className="actions-menu-item"
                  >
                    {isRefreshingAll ? 'Refreshing...' : 'Refresh All Tickers'}
                  </button>
                  <button 
                    onClick={() => {
                      setShowActionsMenu(false);
                      handleFetchBlocksSummary();
                    }}
                    disabled={isFetchingBlocks}
                    className="actions-menu-item"
                  >
                    {isFetchingBlocks ? 'Fetching...' : 'Fetch FINRA Blocks Summary'}
                  </button>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Row 2: Date Range Picker */}
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
