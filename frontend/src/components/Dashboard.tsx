import { useEffect, useState } from 'react';
import { Alert, Button, Card, Col, Container, Dropdown, Row, Spinner } from 'react-bootstrap';
import { Line } from 'react-chartjs-2';
import BorrowFeeChart from './BorrowFeeChart';
import FinraShortInterestChart from './FinraShortInterestChart';
import MovableDateRangePicker from './MovableDateRangePicker';
import ShortInterestChart from './ShortInterestChart';
import ShortVolumeChart from './ShortVolumeChart';
import TickerSearch from './TickerSearch';

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
    <Container fluid className="py-3">
      {/* Controls Section */}
          {/* Top Row: Ticker Search + Recently Viewed + Actions */}
          <Row className="mb-3">
            <Col md={3}>
              <TickerSearch onTickerSelect={handleTickerSelect} />
            </Col>

            <Col md={5}>
              {recentlyViewedTickers.length > 0 && (
                <div className="d-flex align-items-center">
                  <span className="me-2 text-muted">Recently Viewed:</span>
                  <div className="d-flex flex-wrap gap-1">
                    {recentlyViewedTickers.map((ticker, index) => (
                      <Button
                        key={index}
                        variant={selectedTicker === ticker ? "primary" : "outline-secondary"}
                        size="sm"
                        onClick={() => handleTickerSelect(ticker)}
                        className="me-1"
                      >
                        {ticker}
                      </Button>
                    ))}
                  </div>
                </div>
              )}
            </Col>

            <Col md={4}>
              <Dropdown>
                <Dropdown.Toggle variant="primary" id="actions-dropdown">
                  Actions
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item
                    onClick={handleFetchPolygonData}
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
                    onClick={handleFetchAllPolygonData}
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
                    onClick={handleRefreshAllTickers}
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
                    onClick={handleFetchBlocksSummary}
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
            </Col>
          </Row>

          {/* Date Range Picker */}
          <Row>
            <Col>
              <MovableDateRangePicker onDateRangeChange={handleDateRangeChange} />
            </Col>
          </Row>

      {/* Error Alert */}
      {error && (
        <Alert variant={error.includes('Error') || error.includes('Failed') ? 'danger' : 'success'} dismissible onClose={() => setError('')}>
          {error}
        </Alert>
      )}

      {/* Charts Section */}
      {selectedTicker && (
        <div>
          {/* Legacy Charts */}
          <Row className="mb-4">
            <Col md={6}>
              <Card>
                <Card.Header>
                  <h5 className="mb-0">Short Interest - {selectedTicker}</h5>
                </Card.Header>
                <Card.Body>
                  <ShortInterestChart
                    data={shortInterestData}
                    ticker={selectedTicker}
                    isLoading={isLoading}
                  />
                </Card.Body>
              </Card>
            </Col>

            <Col md={6}>
              <Card>
                <Card.Header>
                  <h5 className="mb-0">Short Volume - {selectedTicker}</h5>
                </Card.Header>
                <Card.Body>
                  <ShortVolumeChart
                    data={shortVolumeData}
                    ticker={selectedTicker}
                    isLoading={isLoading}
                  />
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* Polygon Data Charts */}
          {polygonShortInterestData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">Polygon Short Interest - {selectedTicker}</h5>
                  </Card.Header>
                  <Card.Body>
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
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          )}

          {polygonShortVolumeData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">Polygon Short Volume - {selectedTicker}</h5>
                  </Card.Header>
                  <Card.Body>
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
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          )}

          {/* Borrow Fee Chart */}
          <Row className="mb-4">
            <Col md={6}>
              <Card>
                <Card.Header>
                  <h5 className="mb-0">Borrow Fee - {selectedTicker}</h5>
                </Card.Header>
                <Card.Body>
                  <BorrowFeeChart
                    data={borrowFeeData}
                    ticker={selectedTicker}
                    isLoading={isLoading}
                  />
                </Card.Body>
              </Card>
            </Col>
          </Row>

          {/* FINRA Data Section */}
          <Card className="mb-4">
            <Card.Header>
              <h5 className="mb-0">FINRA Regulatory Data</h5>
              <small className="text-muted">Official short interest data from FINRA regulatory database</small>
            </Card.Header>
            <Card.Body>
              <FinraShortInterestChart
                symbol={selectedTicker}
                startDate={dateRange.startDate.toISOString().split('T')[0]}
                endDate={dateRange.endDate.toISOString().split('T')[0]}
              />
            </Card.Body>
          </Card>
        </div>
      )}
    </Container>
  );
};

export default Dashboard;
