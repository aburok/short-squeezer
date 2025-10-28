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

  // Store all data (unfiltered)
  const [allShortInterestData, setAllShortInterestData] = useState<any[]>([]);
  const [allShortVolumeData, setAllShortVolumeData] = useState<any[]>([]);
  const [allBorrowFeeData, setAllBorrowFeeData] = useState<any[]>([]);
  const [allChartExchangeShortInterestData, setAllChartExchangeShortInterestData] = useState<any[]>([]);
  const [allChartExchangeShortVolumeData, setAllChartExchangeShortVolumeData] = useState<any[]>([]);

  // Filtered data for display
  const [shortInterestData, setShortInterestData] = useState<any[]>([]);
  const [shortVolumeData, setShortVolumeData] = useState<any[]>([]);
  const [borrowFeeData, setBorrowFeeData] = useState<any[]>([]);
  const [chartExchangeShortInterestData, setChartExchangeShortInterestData] = useState<any[]>([]);
  const [chartExchangeShortVolumeData, setChartExchangeShortVolumeData] = useState<any[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [isRefreshingAll, setIsRefreshingAll] = useState(false);
  const [isFetchingBlocks, setIsFetchingBlocks] = useState(false);
  const [isFetchingChartExchange, setIsFetchingChartExchange] = useState(false);
  const [isFetchingAllChartExchange, setIsFetchingAllChartExchange] = useState(false);
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
    // Apply filtering immediately when date range changes
    applyDateFiltering(startDate, endDate);
  };

  // Function to filter data by date range
  const applyDateFiltering = (startDate: Date, endDate: Date) => {
    const filterByDate = (data: any[]) => {
      return data.filter(item => {
        const itemDate = new Date(item.date || item.Date);
        return itemDate >= startDate && itemDate <= endDate;
      });
    };

    // Apply filtering to all data arrays
    setShortInterestData(filterByDate(allShortInterestData));
    setShortVolumeData(filterByDate(allShortVolumeData));
    setBorrowFeeData(filterByDate(allBorrowFeeData));
    setChartExchangeShortInterestData(filterByDate(allChartExchangeShortInterestData));
    setChartExchangeShortVolumeData(filterByDate(allChartExchangeShortVolumeData));
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

  const handleFetchChartExchangeData = async () => {
    if (!selectedTicker) {
      setError('Please select a ticker first');
      return;
    }

    setIsFetchingChartExchange(true);
    setError('');

    try {
      const response = await fetch(
        `/api/ChartExchange/${selectedTicker}/fetch?years=2`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        setError(`Successfully fetched ${result.count} new ChartExchange data points (2 years) for ${selectedTicker}!`);
        // Refresh the chart data
        fetchData();
      } else {
        setError(result.message || 'Failed to fetch ChartExchange data');
      }
    } catch (err) {
      setError('Error fetching ChartExchange data: ' + (err as Error).message);
    } finally {
      setIsFetchingChartExchange(false);
    }
  };

  const handleFetchAllChartExchangeData = async () => {
    if (!selectedTicker) {
      setError('Please select a ticker first');
      return;
    }

    setIsFetchingAllChartExchange(true);
    setError('');

    try {
      const response = await fetch(
        `/api/StockData/${selectedTicker}/fetch-chartexchange`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      const result = await response.json();

      if (response.ok && result.success) {
        let message = `ChartExchange data for ${selectedTicker}: `;
        const parts = [];

        if (result.prices.skipped) {
          parts.push(`Prices (skipped - data exists)`);
        } else {
          parts.push(`Prices (${result.prices.fetched} records)`);
        }

        if (result.failureToDeliver.skipped) {
          parts.push(`Failure to Deliver (skipped - data exists)`);
        } else {
          parts.push(`Failure to Deliver (${result.failureToDeliver.fetched} records)`);
        }

        if (result.redditMentions.skipped) {
          parts.push(`Reddit Mentions (skipped - data exists)`);
        } else {
          parts.push(`Reddit Mentions (${result.redditMentions.fetched} records)`);
        }

        if (result.optionChain.skipped) {
          parts.push(`Option Chain (skipped - data exists)`);
        } else {
          parts.push(`Option Chain (${result.optionChain.fetched} records)`);
        }

        if (result.stockSplits.skipped) {
          parts.push(`Stock Splits (skipped - data exists)`);
        } else {
          parts.push(`Stock Splits (${result.stockSplits.fetched} records)`);
        }

        message += parts.join(', ');
        setError(message);
        // Refresh the chart data
        fetchData();
      } else {
        setError(result.error || 'Failed to fetch ChartExchange data');
      }
    } catch (err) {
      setError('Error fetching ChartExchange data: ' + (err as Error).message);
    } finally {
      setIsFetchingAllChartExchange(false);
    }
  };

  const fetchData = async () => {
    if (!selectedTicker) return;

    setIsLoading(true);
    setError('');

    try {
      // Fetch ALL data without date filtering
      console.log(`Fetching all data for ${selectedTicker}...`);

      // Don't fetch FINRA data, only ChartExchange data
      // Set empty arrays for non-ChartExchange data
      setAllShortInterestData([]);
      setAllShortVolumeData([]);

      // Fetch ALL borrow fee data (no date filtering)
      const borrowFeeResponse = await fetch(`/api/BorrowFee/${selectedTicker}`);
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

      setAllBorrowFeeData(transformedBorrowFeeData);

      // Fetch ALL Short Interest data (no date filtering)
      try {
        const shortInterestResponse = await fetch(`/api/ShortInterest/${selectedTicker}`);
        if (shortInterestResponse.ok) {
          const shortInterestResult = await shortInterestResponse.json();
          console.log('Short Interest API Response:', shortInterestResult);
          setAllShortInterestData(shortInterestResult);
        }
      } catch (err) {
        console.warn('Error fetching Short Interest data:', err);
      }

      // Fetch ALL Short Volume data (no date filtering)
      try {
        const shortVolumeResponse = await fetch(`/api/ShortVolume/${selectedTicker}`);
        if (shortVolumeResponse.ok) {
          const shortVolumeResult = await shortVolumeResponse.json();
          console.log('Short Volume API Response:', shortVolumeResult);
          setAllShortVolumeData(shortVolumeResult);
        }
      } catch (err) {
        console.warn('Error fetching Short Volume data:', err);
      }

      // Fetch ALL ChartExchange data from unified endpoint (no date filtering)
      try {
        const stockDataResponse = await fetch(
          `/api/StockData/${selectedTicker}?includeChartExchange=true&includeBorrowFee=false`
        );
        if (stockDataResponse.ok) {
          const stockData = await stockDataResponse.json();
          console.log('Stock Data Response:', stockData);

          // Extract ChartExchange short interest data
          if (stockData.chartExchangeData?.shortInterestData) {
            setAllChartExchangeShortInterestData(stockData.chartExchangeData.shortInterestData);
          }

          // Extract ChartExchange short volume data
          if (stockData.chartExchangeData?.shortVolumeData) {
            // Transform to match chart format
            const transformedChartExchangeShortVolume = stockData.chartExchangeData.shortVolumeData.map((item: any) => ({
              date: item.date || item.Date,
              shortVolume: Number(item.shortVolume || 0),
              totalVolume: Number(item.totalVolume || 0),
              shortVolumePercent: Number(item.shortVolumeRatio || 0)
            }));

            setAllChartExchangeShortVolumeData(transformedChartExchangeShortVolume);
          }
        }
      } catch (err) {
        console.warn('Error fetching ChartExchange data:', err);
      }

      // Apply initial date filtering to display data
      applyDateFiltering(dateRange.startDate, dateRange.endDate);

    } catch (err) {
      setError('Error fetching data: ' + (err as Error).message);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedTicker]); // Only fetch when ticker changes, not when date range changes

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
      <Card className="mb-4">
        <Card.Header>
          <h4 className="mb-0">Stock Data Dashboard</h4>
        </Card.Header>
        <Card.Body>
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
                    onClick={handleFetchChartExchangeData}
                    disabled={!selectedTicker || isFetchingChartExchange}
                  >
                    {isFetchingChartExchange ? (
                      <>
                        <Spinner size="sm" className="me-2" />
                        Fetching...
                      </>
                    ) : (
                      'Fetch ChartExchange Price Data'
                    )}
                  </Dropdown.Item>
                  <Dropdown.Item
                    onClick={handleFetchAllChartExchangeData}
                    disabled={!selectedTicker || isFetchingAllChartExchange}
                  >
                    {isFetchingAllChartExchange ? (
                      <>
                        <Spinner size="sm" className="me-2" />
                        Fetching All...
                      </>
                    ) : (
                      'Fetch All ChartExchange Data'
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
        </Card.Body>
      </Card>

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

          {/* ChartExchange Data Charts */}
          {chartExchangeShortInterestData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">ChartExchange Short Interest - {selectedTicker}</h5>
                  </Card.Header>
                  <Card.Body>
                    <div style={{ height: '300px' }}>
                      <Line
                        data={{
                          labels: chartExchangeShortInterestData.map((item: any) => new Date(item.date || item.Date).toLocaleDateString()),
                          datasets: [{
                            label: 'Short Interest',
                            data: chartExchangeShortInterestData.map((item: any) => Number(item.shortInterest || 0)),
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

          {chartExchangeShortVolumeData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">ChartExchange Short Volume - {selectedTicker}</h5>
                  </Card.Header>
                  <Card.Body>
                    <div style={{ height: '300px' }}>
                      <Line
                        data={{
                          labels: chartExchangeShortVolumeData.map((item: any) => new Date(item.date || item.Date).toLocaleDateString()),
                          datasets: [{
                            label: 'Short Volume %',
                            data: chartExchangeShortVolumeData.map((item: any) => Number(item.shortVolumePercent || 0)),
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
