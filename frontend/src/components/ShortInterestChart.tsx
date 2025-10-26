import React from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import { Line } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

interface ShortInterestData {
  date: string;
  shortInterest: number;
  sharesOutstanding: number;
  shortInterestRatio: number;
}

interface ShortInterestChartProps {
  data: ShortInterestData[];
  ticker: string;
  isLoading: boolean;
}

const ShortInterestChart: React.FC<ShortInterestChartProps> = ({ data, ticker, isLoading }) => {
  if (isLoading) {
    return (
      <div className="chart-container">
        <h3>Short Interest Data - {ticker}</h3>
        <div className="loading-message">Loading short interest data...</div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="chart-container">
        <h3>Short Interest Data - {ticker}</h3>
        <div className="no-data-message">No short interest data available for the selected date range.</div>
      </div>
    );
  }

  const chartData = {
    labels: data.map(item => new Date(item.date).toLocaleDateString()),
    datasets: [
      {
        label: 'Short Interest Ratio (%)',
        data: data.map(item => item.shortInterestRatio),
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgba(255, 99, 132, 0.2)',
        tension: 0.1,
        yAxisID: 'y',
      },
      {
        label: 'Short Interest (Shares)',
        data: data.map(item => item.shortInterest),
        borderColor: 'rgb(54, 162, 235)',
        backgroundColor: 'rgba(54, 162, 235, 0.2)',
        tension: 0.1,
        yAxisID: 'y1',
      }
    ],
  };

  const options = {
    responsive: true,
    interaction: {
      mode: 'index' as const,
      intersect: false,
    },
    plugins: {
      title: {
        display: true,
        text: `Short Interest Analysis - ${ticker}`,
      },
      legend: {
        position: 'top' as const,
      },
    },
    scales: {
      x: {
        display: true,
        title: {
          display: true,
          text: 'Date',
        },
      },
      y: {
        type: 'linear' as const,
        display: true,
        position: 'left' as const,
        title: {
          display: true,
          text: 'Short Interest Ratio (%)',
        },
      },
      y1: {
        type: 'linear' as const,
        display: true,
        position: 'right' as const,
        title: {
          display: true,
          text: 'Short Interest (Shares)',
        },
        grid: {
          drawOnChartArea: false,
        },
      },
    },
  };

  return (
    <div className="chart-container">
      <h3>Short Interest Data - {ticker}</h3>
      <div className="chart-wrapper">
        <Line data={chartData} options={options} />
      </div>
      <div className="chart-info">
        <p><strong>Latest Short Interest Ratio:</strong> {data[data.length - 1]?.shortInterestRatio?.toFixed(2)}%</p>
        <p><strong>Latest Short Interest:</strong> {data[data.length - 1]?.shortInterest?.toLocaleString()} shares</p>
      </div>
    </div>
  );
};

export default ShortInterestChart;
