import React, { useState } from 'react';
import FinraShortInterestChart from './FinraShortInterestChart';
import MovableDateRangePicker from './MovableDateRangePicker';
import './FinraDashboard.css';

interface FinraDashboardProps {
  defaultSymbol?: string;
}

const FinraDashboard: React.FC<FinraDashboardProps> = ({ 
  defaultSymbol = 'AAPL' 
}) => {
  const [symbol, setSymbol] = useState(defaultSymbol);
  const [startDate, setStartDate] = useState<Date | null>(null);
  const [endDate, setEndDate] = useState<Date | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSymbolChange = (newSymbol: string) => {
    setSymbol(newSymbol.toUpperCase());
  };

  const handleDateRangeChange = (start: Date, end: Date) => {
    setStartDate(start);
    setEndDate(end);
  };

  const handleRefresh = () => {
    setIsLoading(true);
    // Trigger a re-render by updating a dummy state
    setTimeout(() => setIsLoading(false), 100);
  };

  const popularSymbols = [
    'AAPL', 'TSLA', 'GME', 'AMC', 'MSFT', 'AMZN', 'GOOGL', 'META', 'NFLX', 'NVDA',
    'SPY', 'QQQ', 'IWM', 'ARKK', 'PLTR', 'ROKU', 'ZOOM', 'PTON', 'BYND', 'NIO'
  ];

  return (
    <div className="finra-dashboard">
      {/* Controls */}
      <div className="finra-controls">
        <div className="control-group">
          {/* Symbol Input */}
          <div className="control-item">
            <label htmlFor="symbol">Stock Symbol</label>
            <input
              type="text"
              id="symbol"
              value={symbol}
              onChange={(e) => handleSymbolChange(e.target.value)}
              placeholder="Enter stock symbol (e.g., AAPL)"
              className="symbol-input"
            />
          </div>

          {/* Date Range Picker */}
          <div className="control-item">
            <label>Date Range (Optional)</label>
            <MovableDateRangePicker
              onDateRangeChange={handleDateRangeChange}
              initialStartDate={startDate || undefined}
              initialEndDate={endDate || undefined}
            />
          </div>

          {/* Refresh Button */}
          <div className="control-item">
            <button
              onClick={handleRefresh}
              className="refresh-button"
              disabled={isLoading}
            >
              {isLoading ? 'Refreshing...' : 'Refresh Data'}
            </button>
          </div>
        </div>

        {/* Popular Symbols */}
        <div className="popular-symbols">
          <label>Popular Symbols</label>
          <div className="symbol-buttons">
            {popularSymbols.map((sym) => (
              <button
                key={sym}
                onClick={() => setSymbol(sym)}
                className={`symbol-button ${symbol === sym ? 'active' : ''}`}
              >
                {sym}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Charts */}
      {symbol && (
        <div className="finra-charts-container">
          <FinraShortInterestChart
            symbol={symbol}
            startDate={startDate ? startDate.toISOString().split('T')[0] : undefined}
            endDate={endDate ? endDate.toISOString().split('T')[0] : undefined}
          />
        </div>
      )}

      {/* Information Panel */}
      <div className="finra-info-panel">
        <h3>About FINRA Short Interest Data</h3>
        <div className="info-content">
          <p>
            <strong>Data Source:</strong> Financial Industry Regulatory Authority (FINRA) - Official regulatory database
          </p>
          <p>
            <strong>Update Frequency:</strong> Twice monthly (mid-month and month-end settlement dates)
          </p>
          <p>
            <strong>Coverage:</strong> All exchange-listed and OTC equity securities
          </p>
          <p>
            <strong>Key Metrics:</strong>
          </p>
          <ul>
            <li><strong>Short Interest %:</strong> Percentage of outstanding shares sold short</li>
            <li><strong>Days to Cover:</strong> Number of days to cover short positions at average volume</li>
            <li><strong>Market Value:</strong> Dollar value of short positions</li>
            <li><strong>Shares Outstanding:</strong> Total shares available for trading</li>
            <li><strong>Average Daily Volume:</strong> Average trading volume over the period</li>
          </ul>
          <p className="note">
            <strong>Note:</strong> High short interest percentages and low days to cover can indicate potential short squeeze scenarios.
          </p>
        </div>
      </div>
    </div>
  );
};

export default FinraDashboard;
