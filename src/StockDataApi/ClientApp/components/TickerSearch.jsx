import React, { useState, useEffect, useRef } from 'react';

const TickerSearch = ({ onTickerSelect }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [suggestions, setSuggestions] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [error, setError] = useState(null);
  const searchRef = useRef(null);

  // Close suggestions when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (searchRef.current && !searchRef.current.contains(event.target)) {
        setShowSuggestions(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Fetch suggestions when search term changes
  useEffect(() => {
    const fetchSuggestions = async () => {
      if (!searchTerm || searchTerm.length < 2) {
        setSuggestions([]);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const response = await fetch(`/api/Tickers/search?query=${encodeURIComponent(searchTerm)}`);
        
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        
        const data = await response.json();
        setSuggestions(data);
      } catch (err) {
        console.error('Error fetching ticker suggestions:', err);
        setError(`Error loading suggestions: ${err.message}`);
      } finally {
        setIsLoading(false);
      }
    };

    // Debounce the search
    const timeoutId = setTimeout(() => {
      fetchSuggestions();
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [searchTerm]);

  const handleInputChange = (e) => {
    setSearchTerm(e.target.value);
    setShowSuggestions(true);
  };

  const handleSelectTicker = (ticker) => {
    setSearchTerm(ticker.symbol);
    setShowSuggestions(false);
    if (onTickerSelect) {
      onTickerSelect(ticker.symbol);
    }
  };

  const handleManualSearch = () => {
    if (searchTerm && searchTerm.trim() !== '') {
      if (onTickerSelect) {
        onTickerSelect(searchTerm.trim().toUpperCase());
      }
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleManualSearch();
    }
  };

  return (
    <div className="ticker-search" ref={searchRef}>
      <div className="search-input-container">
        <input
          type="text"
          value={searchTerm}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          placeholder="Enter ticker symbol (e.g., AAPL)"
          className="search-input"
        />
        <button onClick={handleManualSearch} className="search-button">
          Search
        </button>
      </div>
      
      {showSuggestions && (
        <div className="suggestions-container">
          {isLoading && <div className="loading">Loading...</div>}
          {error && <div className="error">{error}</div>}
          {!isLoading && !error && suggestions.length > 0 && (
            <ul className="suggestions-list">
              {suggestions.map((ticker) => (
                <li 
                  key={ticker.symbol} 
                  onClick={() => handleSelectTicker(ticker)}
                  className="suggestion-item"
                >
                  <span className="ticker-symbol">{ticker.symbol}</span>
                  <span className="ticker-name">{ticker.name}</span>
                </li>
              ))}
            </ul>
          )}
          {!isLoading && !error && suggestions.length === 0 && searchTerm.length >= 2 && (
            <div className="no-results">No matching tickers found</div>
          )}
        </div>
      )}
    </div>
  );
};

export default TickerSearch;

// Made with Bob
