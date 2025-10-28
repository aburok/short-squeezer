import React, { useEffect, useState } from 'react';
import { Button, Container, Dropdown, Nav, Navbar, Spinner } from 'react-bootstrap';
import { Link } from 'react-router-dom';
import TickerSearch from './TickerSearch';

interface TopNavigationProps {
  selectedTicker: string;
  onTickerSelect: (ticker: string) => void;
  onFetchChartExchangeData: () => void;
  onRefreshAllTickers: () => void;
  onFetchBlocksSummary: () => void;
  isFetchingChartExchange: boolean;
  isRefreshingAll: boolean;
  isFetchingBlocks: boolean;
}

const TopNavigation: React.FC<TopNavigationProps> = ({ 
  selectedTicker, 
  onTickerSelect,
  onFetchChartExchangeData,
  onRefreshAllTickers,
  onFetchBlocksSummary,
  isFetchingChartExchange,
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

                    {/* Right side: Ticker Search, Recently Viewed, and Actions */}
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

                            {/* Actions Dropdown */}
                            <Dropdown>
                                <Dropdown.Toggle variant="primary" id="actions-dropdown">
                                    Actions
                                </Dropdown.Toggle>
                                <Dropdown.Menu>
                                    <Dropdown.Item
                                        onClick={onFetchPolygonData}
                                        disabled={!selectedTicker || isFetchingPolygon}
                                    >
                                        {isFetchingPolygon ? (
                                            <>
                                                <Spinner size="sm" className="me-2" />
                                                Fetching...
                                            </>
                                        ) : (
                                            'Fetch Polygon Price Data'
                                        )}
                                    </Dropdown.Item>
                                    <Dropdown.Item
                                        onClick={onFetchAllPolygonData}
                                        disabled={!selectedTicker || isFetchingAllPolygon}
                                    >
                                        {isFetchingAllPolygon ? (
                                            <>
                                                <Spinner size="sm" className="me-2" />
                                                Fetching All...
                                            </>
                                        ) : (
                                            'Fetch All Polygon Data'
                                        )}
                                    </Dropdown.Item>
                                    <Dropdown.Divider />
                                    <Dropdown.Item
                                        onClick={onRefreshAllTickers}
                                        disabled={isRefreshingAll}
                                    >
                                        {isRefreshingAll ? (
                                            <>
                                                <Spinner size="sm" className="me-2" />
                                                Refreshing...
                                            </>
                                        ) : (
                                            'Refresh All Tickers'
                                        )}
                                    </Dropdown.Item>
                                    <Dropdown.Item
                                        onClick={onFetchBlocksSummary}
                                        disabled={isFetchingBlocks}
                                    >
                                        {isFetchingBlocks ? (
                                            <>
                                                <Spinner size="sm" className="me-2" />
                                                Fetching...
                                            </>
                                        ) : (
                                            'Fetch FINRA Blocks Summary'
                                        )}
                                    </Dropdown.Item>
                                </Dropdown.Menu>
                            </Dropdown>
                        </div>
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
};

export default TopNavigation;
