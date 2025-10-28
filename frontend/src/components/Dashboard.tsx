import { useEffect, useState } from 'react';
import { Alert, Card, Col, Container, Row } from 'react-bootstrap';
import { Line } from 'react-chartjs-2';
import BorrowFeeChart from './BorrowFeeChart';
import MovableDateRangePicker from './MovableDateRangePicker';

interface DashboardProps {
  selectedTicker: string;
}

const Dashboard: React.FC<DashboardProps> = ({ selectedTicker }) => {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000), // 30 days ago
    endDate: new Date()
  });

  // Store all data (unfiltered)
  const [allBorrowFeeData, setAllBorrowFeeData] = useState<any[]>([]);
  const [allChartExchangeShortInterestData, setAllChartExchangeShortInterestData] = useState<any[]>([]);
  const [allChartExchangeShortVolumeData, setAllChartExchangeShortVolumeData] = useState<any[]>([]);

  // Filtered data for display
  const [borrowFeeData, setBorrowFeeData] = useState<any[]>([]);
  const [chartExchangeShortInterestData, setChartExchangeShortInterestData] = useState<any[]>([]);
  const [chartExchangeShortVolumeData, setChartExchangeShortVolumeData] = useState<any[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

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

    // Apply filtering to ChartExchange data arrays
    setBorrowFeeData(filterByDate(allBorrowFeeData));
    setChartExchangeShortInterestData(filterByDate(allChartExchangeShortInterestData));
    setChartExchangeShortVolumeData(filterByDate(allChartExchangeShortVolumeData));
  };

  const fetchData = async () => {
    if (!selectedTicker) return;

    setIsLoading(true);
    setError('');

    try {
      // Fetch ALL ChartExchange data using the unified endpoint
      console.log(`Fetching all ChartExchange data for ${selectedTicker}...`);

      // Use the unified StockData endpoint to get all ChartExchange data
      const stockDataResponse = await fetch(
        `/api/StockData/${selectedTicker}?includeChartExchange=true&includeBorrowFee=true&includeFinra=false`
      );

      if (!stockDataResponse.ok) {
        throw new Error(`API request failed: ${stockDataResponse.status}`);
      }

      const stockData = await stockDataResponse.json();
      console.log('Stock Data Response:', stockData);

      // Extract borrow fee data
      if (stockData.borrowFeeData && Array.isArray(stockData.borrowFeeData)) {
        const transformedBorrowFeeData = stockData.borrowFeeData.map((item: any) => ({
          date: item.date || item.Date,
          fee: Number(item.fee || item.Fee || 0),
          availableShares: item.availableShares || item.AvailableShares
        }));
        setAllBorrowFeeData(transformedBorrowFeeData);
        console.log('Borrow Fee Data:', transformedBorrowFeeData);
      } else {
        setAllBorrowFeeData([]);
        console.log('No borrow fee data found');
      }

      // Extract ChartExchange short interest data
      if (stockData.chartExchangeData?.shortInterestData && Array.isArray(stockData.chartExchangeData.shortInterestData)) {
        setAllChartExchangeShortInterestData(stockData.chartExchangeData.shortInterestData);
        console.log('ChartExchange Short Interest Data:', stockData.chartExchangeData.shortInterestData);
      } else {
        setAllChartExchangeShortInterestData([]);
        console.log('No ChartExchange short interest data found');
      }

      // Extract ChartExchange short volume data
      if (stockData.chartExchangeData?.shortVolumeData && Array.isArray(stockData.chartExchangeData.shortVolumeData)) {
        const transformedChartExchangeShortVolume = stockData.chartExchangeData.shortVolumeData.map((item: any) => ({
          date: item.date || item.Date,
          shortVolume: Number(item.shortVolume || 0),
          totalVolume: Number(item.totalVolume || 0),
          shortVolumePercent: Number(item.shortVolumePercent || 0)
        }));
        setAllChartExchangeShortVolumeData(transformedChartExchangeShortVolume);
        console.log('ChartExchange Short Volume Data:', transformedChartExchangeShortVolume);
      } else {
        setAllChartExchangeShortVolumeData([]);
        console.log('No ChartExchange short volume data found');
      }

      // Apply initial date filtering to display data
      applyDateFiltering(dateRange.startDate, dateRange.endDate);

    } catch (err) {
      console.error('Error fetching ChartExchange data:', err);
      setError('Error fetching ChartExchange data: ' + (err as Error).message);

      // Set empty arrays on error to prevent white screen
      setAllBorrowFeeData([]);
      setAllChartExchangeShortInterestData([]);
      setAllChartExchangeShortVolumeData([]);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [selectedTicker]); // Only fetch when ticker changes, not when date range changes

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
          {/* Loading State */}
          {isLoading && (
            <Row className="mb-4">
              <Col>
                <Card>
                  <Card.Body className="text-center py-4">
                    <div className="spinner-border text-primary" role="status">
                      <span className="visually-hidden">Loading...</span>
                    </div>
                    <p className="mt-2">Loading data for {selectedTicker}...</p>
                  </Card.Body>
                </Card>
              </Col>
            </Row>
          )}

          {/* ChartExchange Data Charts */}
          {!isLoading && chartExchangeShortInterestData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">Short Interest - {selectedTicker}</h5>
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

          {!isLoading && chartExchangeShortVolumeData.length > 0 && (
            <Row className="mb-4">
              <Col md={6}>
                <Card>
                  <Card.Header>
                    <h5 className="mb-0">Short Volume - {selectedTicker}</h5>
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
          {!isLoading && (
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
          )}

        </div>
      )}
    </Container>
  );
};

export default Dashboard;
