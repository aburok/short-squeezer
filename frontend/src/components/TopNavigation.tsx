import React, { useEffect, useState } from 'react';
import { Button, Container, Dropdown, Nav, Navbar, Spinner } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import TickerSearch from './TickerSearch';

interface TopNavigationProps {
  selectedTicker: string;
  onTickerSelect: (ticker: string) => void;
  onFetchPolygonData: () => void;
  onFetchAllPolygonData: () => void;
  onRefreshAllTickers: () => void;
  onFetchBlocksSummary: () => void;
  isFetchingPolygon: boolean;
  isFetchingAllPolygon: boolean;
  isRefreshingAll: boolean;
  isFetchingBlocks: boolean;
}

const TopNavigation: React.FC<TopNavigationProps> = ({ 
  selectedTicker, 
  onTickerSelect,
  onFetchPolygonData,
  onFetchAllPolygonData,
  onRefreshAllTickers,
  onFetchBlocksSummary,
  isFetchingPolygon,
  isFetchingAllPolygon,
  isRefreshingAll,
  isFetchingBlocks
}) => {
  const [recentlyViewedTickers, setRecentlyViewedTickers] = useState<string[]>([]);

  // Load recently viewed tickers from localStorage on mount
  useEffect(() => {
    const saved = localStorage.getItem('recentlyViewedTickers');
    if (saved) {
      try {
        const parsed = JSON.parse(saved);
        setRecentlyViewedTickers(parsed);
      } catch (error) {
        console.error('Error parsing recently viewed tickers:', error);
      }
    }
  }, []);

  // Update recently viewed tickers when a new ticker is selected
  useEffect(() => {
    if (selectedTicker) {
      setRecentlyViewedTickers(prev => {
        const filtered = prev.filter(t => t !== selectedTicker);
        const updated = [selectedTicker, ...filtered].slice(0, 10); // Keep only last 10
        // Save to localStorage
        localStorage.setItem('recentlyViewedTickers', JSON.stringify(updated));
        return updated;
      });
    }
  }, [selectedTicker]);

  const handleTickerSelect = (ticker: string) => {
    onTickerSelect(ticker);
  };

  return (
    <Navbar bg="dark" variant="dark" expand="lg" className="mb-4">
      <Container fluid>
        <Navbar.Brand href="/">Stock Data API</Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="me-auto">
            <Nav.Link as={Link} to="/">Main Dashboard</Nav.Link>
            <Nav.Link as={Link} to="/finra">FINRA Dashboard</Nav.Link>
          </Nav>
          
          {/* Right side: Ticker Search and Recently Viewed */}
          <Nav className="ms-auto">
            <div className="d-flex align-items-center gap-3">
              {/* Ticker Search */}
              <div style={{ minWidth: '250px' }}>
                <TickerSearch onTickerSelect={handleTickerSelect} />
              </div>
              
              {/* Recently Viewed Tickers */}
              {recentlyViewedTickers.length > 0 && (
                <div className="d-flex align-items-center">
                  <span className="me-2 text-light" style={{ fontSize: '0.9rem' }}>Recent:</span>
                  <div className="d-flex flex-wrap gap-1">
                    {recentlyViewedTickers.slice(0, 5).map((ticker, index) => (
                      <Button
                        key={index}
                        variant={selectedTicker === ticker ? "primary" : "outline-light"}
                        size="sm"
                        onClick={() => handleTickerSelect(ticker)}
                        className="me-1"
                        style={{ fontSize: '0.8rem', padding: '0.25rem 0.5rem' }}
                      >
                        {ticker}
                      </Button>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
};

export default TopNavigation;
