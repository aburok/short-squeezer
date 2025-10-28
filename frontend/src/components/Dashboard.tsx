import { useEffect, useState } from 'react';
import { Alert, Card, Col, Container, Row } from 'react-bootstrap';
import { Line } from 'react-chartjs-2';
import BorrowFeeChart from './BorrowFeeChart';
import FinraShortInterestChart from './FinraShortInterestChart';
import MovableDateRangePicker from './MovableDateRangePicker';
import ShortInterestChart from './ShortInterestChart';
import ShortVolumeChart from './ShortVolumeChart';

interface DashboardProps {
  selectedTicker: string;
  onTickerSelect: (ticker: string) => void;
}

const Dashboard: React.FC<DashboardProps> = ({ selectedTicker, onTickerSelect }) => {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000), // 30 days ago
    endDate: new Date()
  });
  
  // Store all data (unfiltered)
  const [allShortInterestData, setAllShortInterestData] = useState([]);
  const [allShortVolumeData, setAllShortVolumeData] = useState([]);
  const [allBorrowFeeData, setAllBorrowFeeData] = useState([]);
  const [allChartExchangeShortInterestData, setAllChartExchangeShortInterestData] = useState([]);
  const [allChartExchangeShortVolumeData, setAllChartExchangeShortVolumeData] = useState([]);
  
  // Filtered data for display
  const [shortInterestData, setShortInterestData] = useState([]);
  const [shortVolumeData, setShortVolumeData] = useState([]);
  const [borrowFeeData, setBorrowFeeData] = useState([]);
  const [chartExchangeShortInterestData, setChartExchangeShortInterestData] = useState([]);
  const [chartExchangeShortVolumeData, setChartExchangeShortVolumeData] = useState([]);
  
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleTickerSelect = (ticker: string) => {
    onTickerSelect(ticker);
    setError('');
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
  }, [selectedTicker, dateRange]);

  return (
    <Container fluid className="py-3">
      {/* Date Range Picker */}
      <Row className="mb-4">
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
