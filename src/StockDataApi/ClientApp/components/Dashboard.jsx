import React, { useState, useEffect } from 'react';
import TickerSearch from './TickerSearch';
import MovableDateRangePicker from './MovableDateRangePicker';
import ShortInterestChart from './ShortInterestChart';
import ShortVolumeChart from './ShortVolumeChart';

const Dashboard = () => {
  // Single ticker state for all charts
  const [selectedTicker, setSelectedTicker] = useState('');
  
  // Single date range state for all charts
  const [dateRange, setDateRange] = useState({
    startDate: null,
    endDate: null
  });
  
  // UI state
  const [isDataFetching, setIsDataFetching] = useState(false);
  const [isRefreshingAll, setIsRefreshingAll] = useState(false);
  const [fetchMessage, setFetchMessage] = useState('');
  const [activeCharts, setActiveCharts] = useState({
    shortInterest: true,
    shortVolume: true,
    borrowFee: true
  });

  // Handle ticker selection from the search component
  const handleTickerSelect = (ticker) => {
    setSelectedTicker(ticker);
    // Clear any previous messages
    setFetchMessage('');
  };

  // Handle date range changes from the date picker
  const handleDateRangeChange = (start, end) => {
    const formattedStartDate = start ? start.toISOString().split('T')[0] : null;
    const formattedEndDate = end ? end.toISOString().split('T')[0] : null;
    
    setDateRange({
      startDate: formattedStartDate,
      endDate: formattedEndDate
    });
    
    console.log(`Date range updated: ${formattedStartDate} to ${formattedEndDate}`);
  };

  // Refresh all tickers from exchanges
  const handleRefreshAllTickers = async () => {
    setIsRefreshingAll(true);
    setFetchMessage('Refreshing all tickers from exchanges...');

    try {
      const response = await fetch('/api/Tickers/refresh-all', {
        method: 'POST'
      });

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const result = await response.text();
      setFetchMessage('Successfully refreshed all tickers from exchanges');
      
      // Clear selected ticker to force a refresh of the ticker search
      setSelectedTicker('');
      
    } catch (error) {
      console.error('Error refreshing all tickers:', error);
      setFetchMessage(`Error refreshing all tickers: ${error.message}`);
    } finally {
      setTimeout(() => {
        setIsRefreshingAll(false);
        setTimeout(() => setFetchMessage(''), 5000); // Show message longer for refresh all
      }, 2000);
    }
  };

  // Fetch latest data for the selected ticker
  const handleFetchData = async () => {
    if (!selectedTicker) {
      alert('Please select a ticker first');
      return;
    }

    setIsDataFetching(true);
    setFetchMessage(`Fetching latest data for ${selectedTicker}...`);

    try {
      const response = await fetch(`/api/Tickers/fetch/${selectedTicker}`, {
        method: 'POST'
      });

      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const result = await response.json();
      setFetchMessage(`Successfully fetched data for ${selectedTicker}`);
      
      // Refresh charts by forcing a re-render
      setSelectedTicker(prevTicker => {
        // This is a trick to force re-render while keeping the same value
        const temp = '';
        setTimeout(() => setSelectedTicker(prevTicker), 100);
        return temp;
      });
    } catch (error) {
      console.error('Error fetching data:', error);
      setFetchMessage(`Error fetching data: ${error.message}`);
    } finally {
      setTimeout(() => {
        setIsDataFetching(false);
        setTimeout(() => setFetchMessage(''), 3000);
      }, 1000);
    }
  };

  // Toggle chart visibility
  const toggleChart = (chartName) => {
    setActiveCharts(prev => ({
      ...prev,
      [chartName]: !prev[chartName]
    }));
  };

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Stock Data Dashboard</h1>
        
        {/* Unified controls section */}
        <div className="unified-controls">
          <div className="control-panel">
            <div className="ticker-control">
              <h3>Select Ticker</h3>
              <TickerSearch onTickerSelect={handleTickerSelect} />
              <div className="button-group">
                <button
                  onClick={handleFetchData}
                  disabled={!selectedTicker || isDataFetching}
                  className="fetch-button"
                >
                  {isDataFetching ? 'Fetching...' : 'Fetch Latest Data'}
                </button>
                <button
                  onClick={handleRefreshAllTickers}
                  disabled={isRefreshingAll}
                  className="refresh-all-button"
                >
                  {isRefreshingAll ? 'Refreshing All...' : 'Refresh All Tickers'}
                </button>
              </div>
            </div>
            
            <div className="date-range-control">
              <h3>Select Date Range</h3>
              <MovableDateRangePicker onDateRangeChange={handleDateRangeChange} />
            </div>
          </div>
          
          {/* Status message */}
          {fetchMessage && (
            <div className={`fetch-message ${isDataFetching ? 'loading' : 'success'}`}>
              {fetchMessage}
            </div>
          )}
          
          {/* Chart toggles */}
          <div className="chart-toggles">
            <button
              className={`toggle-button ${activeCharts.shortInterest ? 'active' : ''}`}
              onClick={() => toggleChart('shortInterest')}
            >
              Short Interest
            </button>
            <button
              className={`toggle-button ${activeCharts.shortVolume ? 'active' : ''}`}
              onClick={() => toggleChart('shortVolume')}
            >
              Short Volume
            </button>
            <button
              className={`toggle-button ${activeCharts.borrowFee ? 'active' : ''}`}
              onClick={() => toggleChart('borrowFee')}
            >
              Borrow Fee
            </button>
          </div>
        </div>
      </div>

      {/* Charts section */}
      {selectedTicker ? (
        <div className="charts-container">
          {activeCharts.shortInterest && (
            <div className="chart-wrapper">
              <h2>Short Interest Data for {selectedTicker}</h2>
              <ShortInterestChart
                symbol={selectedTicker}
                startDate={dateRange.startDate}
                endDate={dateRange.endDate}
              />
            </div>
          )}
          
          {activeCharts.shortVolume && (
            <div className="chart-wrapper">
              <h2>Short Volume Data for {selectedTicker}</h2>
              <ShortVolumeChart
                symbol={selectedTicker}
                startDate={dateRange.startDate}
                endDate={dateRange.endDate}
              />
            </div>
          )}
          
          {activeCharts.borrowFee && (
            <div className="chart-wrapper">
              <h2>Borrow Fee Data for {selectedTicker}</h2>
              <div className="coming-soon">
                <p>Borrow Fee chart coming soon</p>
              </div>
            </div>
          )}
        </div>
      ) : (
        <div className="no-data-message">
          <p>Please select a ticker symbol to view data</p>
        </div>
      )}
    </div>
  );
};

export default Dashboard;