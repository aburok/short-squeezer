import {
  CategoryScale,
  Chart as ChartJS,
  Filler,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import moment from 'moment';
import React from 'react';
import { Badge, Card, Col, Row } from 'react-bootstrap';
import { Line } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

interface BorrowFeeData {
  date: string;
  fee: number;
  availableShares?: number;
  open?: number;
  high?: number;
  low?: number;
  close?: number;
  average?: number;
  dataPointCount?: number;
}

interface BorrowFeeChartProps {
  data: BorrowFeeData[];
  ticker: string;
  isLoading: boolean;
}

const BorrowFeeChart: React.FC<BorrowFeeChartProps> = ({ data, ticker, isLoading }) => {
  if (isLoading) {
    return (
      <Card>
        <Card.Header>
          <h5 className="mb-0">Borrow Fee - {ticker}</h5>
        </Card.Header>
        <Card.Body>
          <div className="text-center py-4">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
            <p className="mt-2 mb-0">Loading borrow fee data...</p>
          </div>
        </Card.Body>
      </Card>
    );
  }

  if (!data || data.length === 0) {
    return (
      <Card>
        <Card.Header>
          <h5 className="mb-0">Borrow Fee - {ticker}</h5>
        </Card.Header>
        <Card.Body>
          <div className="text-center py-4">
            <div className="text-muted">
              <i className="bi bi-graph-down fs-1"></i>
              <p className="mt-2 mb-0">No borrow fee data available for {ticker}</p>
            </div>
          </div>
        </Card.Body>
      </Card>
    );
  }

  // Sort data by date to ensure proper chronological order
  const sortedData = [...data].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

  const chartData = {
    labels: sortedData.map(item => moment(item.date).format('MMM DD')),
    datasets: [
      {
        label: 'Borrow Fee Close (%)',
        data: sortedData.map(item => item.fee),
        borderColor: 'rgb(220, 53, 69)', // Bootstrap danger color
        backgroundColor: 'rgba(220, 53, 69, 0.1)',
        borderWidth: 2,
        fill: true,
        tension: 0.4,
        pointBackgroundColor: 'rgb(220, 53, 69)',
        pointBorderColor: '#fff',
        pointBorderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6,
      },
      {
        label: 'Borrow Fee High (%)',
        data: sortedData.map(item => item.high || item.fee),
        borderColor: 'rgb(25, 135, 84)', // Bootstrap success color
        backgroundColor: 'rgba(25, 135, 84, 0.1)',
        borderWidth: 1,
        fill: false,
        tension: 0.4,
        pointRadius: 2,
        pointHoverRadius: 4,
        borderDash: [5, 5],
      },
      {
        label: 'Borrow Fee Low (%)',
        data: sortedData.map(item => item.low || item.fee),
        borderColor: 'rgb(13, 202, 240)', // Bootstrap info color
        backgroundColor: 'rgba(13, 202, 240, 0.1)',
        borderWidth: 1,
        fill: false,
        tension: 0.4,
        pointRadius: 2,
        pointHoverRadius: 4,
        borderDash: [5, 5],
      }
    ],
  };

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'top' as const,
        labels: {
          usePointStyle: true,
          padding: 20,
        },
      },
      title: {
        display: false, // We're using our own header
      },
      tooltip: {
        mode: 'index' as const,
        intersect: false,
        callbacks: {
          title: function (context: any) {
            const dataIndex = context[0].dataIndex;
            return moment(sortedData[dataIndex].date).format('MMMM DD, YYYY');
          },
          label: function (context: any) {
            const dataIndex = context.dataIndex;
            const item = sortedData[dataIndex];
            const datasetLabel = context.dataset.label;

            if (datasetLabel === 'Borrow Fee Close (%)') {
              let label = `Close: ${item.fee.toFixed(2)}%`;
              if (item.open !== undefined) {
                label += `\nOpen: ${item.open.toFixed(2)}%`;
                label += `\nHigh: ${item.high?.toFixed(2)}%`;
                label += `\nLow: ${item.low?.toFixed(2)}%`;
                label += `\nAverage: ${item.average?.toFixed(2)}%`;
              }
              if (item.availableShares) {
                label += `\nAvg Available: ${item.availableShares.toLocaleString()}`;
              }
              if (item.dataPointCount) {
                label += `\nData Points: ${item.dataPointCount}`;
              }
              return label;
            } else if (datasetLabel === 'Borrow Fee High (%)') {
              return `High: ${item.high?.toFixed(2)}%`;
            } else if (datasetLabel === 'Borrow Fee Low (%)') {
              return `Low: ${item.low?.toFixed(2)}%`;
            }
            return `${datasetLabel}: ${context.parsed.y.toFixed(2)}%`;
          },
        },
      },
    },
    scales: {
      x: {
        display: true,
        title: {
          display: true,
          text: 'Date',
        },
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)',
        },
      },
      y: {
        display: true,
        title: {
          display: true,
          text: 'Borrow Fee (%)',
        },
        grid: {
          display: true,
          color: 'rgba(0, 0, 0, 0.1)',
        },
        beginAtZero: true,
      },
    },
    interaction: {
      mode: 'nearest' as const,
      axis: 'x' as const,
      intersect: false,
    },
  };

  // Calculate statistics using OHLC data
  const highs = sortedData.map(item => item.high || item.fee);
  const lows = sortedData.map(item => item.low || item.fee);
  const averages = sortedData.map(item => item.average || item.fee);

  const minFee = Math.min(...lows);
  const maxFee = Math.max(...highs);
  const avgFee = averages.reduce((sum, fee) => sum + fee, 0) / averages.length;
  const latestFee = sortedData[sortedData.length - 1]?.fee;

  return (
    <Card>
      <Card.Header>
        <div className="d-flex justify-content-between align-items-center">
          <h5 className="mb-0">Borrow Fee - {ticker}</h5>
          <Badge bg="secondary" className="fs-6">
            {sortedData.length} data points
          </Badge>
        </div>
      </Card.Header>
      <Card.Body>
        {/* Statistics Row */}
        <Row className="mb-3">
          <Col xs={6} sm={3}>
            <div className="text-center">
              <div className="h6 text-muted mb-1">Latest Close</div>
              <div className="h5 text-primary mb-0">{latestFee?.toFixed(2)}%</div>
            </div>
          </Col>
          <Col xs={6} sm={3}>
            <div className="text-center">
              <div className="h6 text-muted mb-1">Daily Avg</div>
              <div className="h5 text-info mb-0">{avgFee.toFixed(2)}%</div>
            </div>
          </Col>
          <Col xs={6} sm={3}>
            <div className="text-center">
              <div className="h6 text-muted mb-1">Period Low</div>
              <div className="h5 text-success mb-0">{minFee.toFixed(2)}%</div>
            </div>
          </Col>
          <Col xs={6} sm={3}>
            <div className="text-center">
              <div className="h6 text-muted mb-1">Period High</div>
              <div className="h5 text-danger mb-0">{maxFee.toFixed(2)}%</div>
            </div>
          </Col>
        </Row>

        {/* Chart */}
        <div style={{ height: '300px' }}>
          <Line data={chartData} options={options} />
        </div>
      </Card.Body>
    </Card>
  );
};

export default BorrowFeeChart;