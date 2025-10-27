import React, { useState } from 'react';
import FinraShortInterestChart from './FinraShortInterestChart';
import MovableDateRangePicker from './MovableDateRangePicker';

interface FinraDashboardProps {
  defaultSymbol?: string;
}

const FinraDashboard: React.FC<FinraDashboardProps> = ({ 
  defaultSymbol = 'AAPL' 
}) => {
  const [symbol, setSymbol] = useState(defaultSymbol);
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSymbolChange = (newSymbol: string) => {
    setSymbol(newSymbol.toUpperCase());
  };

  const handleDateRangeChange = (start: string, end: string) => {
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
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">
                FINRA Short Interest Dashboard
              </h1>
              <p className="mt-2 text-gray-600">
                Comprehensive analysis of short interest data from FINRA regulatory database
              </p>
            </div>
            
            <div className="mt-4 lg:mt-0 flex flex-col sm:flex-row gap-4">
              <button
                onClick={handleRefresh}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Refresh Data
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Controls */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div className="bg-white rounded-lg shadow p-6 mb-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Symbol Input */}
            <div>
              <label htmlFor="symbol" className="block text-sm font-medium text-gray-700 mb-2">
                Stock Symbol
              </label>
              <div className="relative">
                <input
                  type="text"
                  id="symbol"
                  value={symbol}
                  onChange={(e) => handleSymbolChange(e.target.value)}
                  placeholder="Enter stock symbol (e.g., AAPL)"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                />
              </div>
            </div>

            {/* Date Range Picker */}
            <div className="lg:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Date Range (Optional)
              </label>
              <MovableDateRangePicker
                onDateRangeChange={handleDateRangeChange}
                defaultStartDate={startDate}
                defaultEndDate={endDate}
              />
            </div>
          </div>

          {/* Popular Symbols */}
          <div className="mt-6">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Popular Symbols
            </label>
            <div className="flex flex-wrap gap-2">
              {popularSymbols.map((sym) => (
                <button
                  key={sym}
                  onClick={() => setSymbol(sym)}
                  className={`px-3 py-1 text-sm rounded-full border transition-colors ${
                    symbol === sym
                      ? 'bg-blue-600 text-white border-blue-600'
                      : 'bg-gray-100 text-gray-700 border-gray-300 hover:bg-gray-200'
                  }`}
                >
                  {sym}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Charts */}
        {symbol && (
          <div className="space-y-6">
            <FinraShortInterestChart
              symbol={symbol}
              startDate={startDate || undefined}
              endDate={endDate || undefined}
            />
          </div>
        )}

        {/* Information Panel */}
        <div className="mt-8 bg-blue-50 border border-blue-200 rounded-lg p-6">
          <h3 className="text-lg font-semibold text-blue-900 mb-3">
            About FINRA Short Interest Data
          </h3>
          <div className="text-blue-800 space-y-2">
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
            <ul className="list-disc list-inside ml-4 space-y-1">
              <li><strong>Short Interest %:</strong> Percentage of outstanding shares sold short</li>
              <li><strong>Days to Cover:</strong> Number of days to cover short positions at average volume</li>
              <li><strong>Market Value:</strong> Dollar value of short positions</li>
              <li><strong>Shares Outstanding:</strong> Total shares available for trading</li>
              <li><strong>Average Daily Volume:</strong> Average trading volume over the period</li>
            </ul>
            <p className="mt-3">
              <strong>Note:</strong> High short interest percentages and low days to cover can indicate potential short squeeze scenarios.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default FinraDashboard;
