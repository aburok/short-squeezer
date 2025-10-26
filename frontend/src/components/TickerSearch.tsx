import React, { useState, useEffect } from 'react';

interface TickerSearchProps {
  onTickerSelect: (ticker: string) => void;
}

const TickerSearch: React.FC<TickerSearchProps> = ({ onTickerSelect }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [tickers, setTickers] = useState<string[]>([]);
  const [filteredTickers, setFilteredTickers] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);

  useEffect(() => {
    fetchTickers();
  }, []);

  useEffect(() => {
    if (searchTerm.length >= 2) {
      const filtered = tickers.filter(ticker =>
        ticker.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setFilteredTickers(filtered.slice(0, 10)); // Limit to 10 suggestions
      setShowSuggestions(true);
    } else {
      setFilteredTickers([]);
      setShowSuggestions(false);
    }
  }, [searchTerm, tickers]);

  const fetchTickers = async () => {
    setIsLoading(true);
    try {
      const response = await fetch('/api/Tickers');
      if (response.ok) {
        const data = await response.json();
        const tickerSymbols = data.map((item: any) => item.symbol).sort();
        setTickers(tickerSymbols);
      }
    } catch (error) {
      console.error('Error fetching tickers:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleTickerSelect = (ticker: string) => {
    setSearchTerm(ticker);
    setShowSuggestions(false);
    onTickerSelect(ticker);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && filteredTickers.length > 0) {
      handleTickerSelect(filteredTickers[0]);
    }
  };

  return (
    <div className="ticker-search">
      <div className="search-input-container">
        <input
          type="text"
          placeholder="Enter stock ticker symbol (e.g., AAPL, TSLA, MSFT)"
          value={searchTerm}
          onChange={handleInputChange}
          onKeyPress={handleKeyPress}
          onFocus={() => setShowSuggestions(searchTerm.length >= 2)}
          className="ticker-input"
        />
        {isLoading && <div className="loading-spinner">Loading...</div>}
      </div>

      {showSuggestions && filteredTickers.length > 0 && (
        <div className="suggestions-dropdown">
          {filteredTickers.map((ticker, index) => (
            <div
              key={index}
              className="suggestion-item"
              onClick={() => handleTickerSelect(ticker)}
            >
              {ticker}
            </div>
          ))}
        </div>
      )}

      {showSuggestions && filteredTickers.length === 0 && searchTerm.length >= 2 && (
        <div className="no-suggestions">
          No tickers found matching "{searchTerm}"
        </div>
      )}
    </div>
  );
};

export default TickerSearch;
