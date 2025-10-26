import React from 'react';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
);

interface ShortVolumeData {
  date: string;
  shortVolume: number;
  totalVolume: number;
  shortVolumeRatio: number;
}

interface ShortVolumeChartProps {
  data: ShortVolumeData[];
  ticker: string;
  isLoading: boolean;
}

const ShortVolumeChart: React.FC<ShortVolumeChartProps> = ({ data, ticker, isLoading }) => {
  if (isLoading) {
    return (
      <div className="chart-container">
        <h3>Short Volume Data - {ticker}</h3>
        <div className="loading-message">Loading short volume data...</div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="chart-container">
        <h3>Short Volume Data - {ticker}</h3>
        <div className="no-data-message">No short volume data available for the selected date range.</div>
      </div>
    );
  }

  const chartData = {
    labels: data.map(item => new Date(item.date).toLocaleDateString()),
    datasets: [
      {
        label: 'Short Volume Ratio (%)',
        data: data.map(item => item.shortVolumeRatio),
        backgroundColor: 'rgba(75, 192, 192, 0.6)',
        borderColor: 'rgba(75, 192, 192, 1)',
        borderWidth: 1,
        yAxisID: 'y',
      },
      {
        label: 'Short Volume (Shares)',
        data: data.map(item => item.shortVolume),
        backgroundColor: 'rgba(255, 159, 64, 0.6)',
        borderColor: 'rgba(255, 159, 64, 1)',
        borderWidth: 1,
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
        text: `Short Volume Analysis - ${ticker}`,
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
          text: 'Short Volume Ratio (%)',
        },
      },
      y1: {
        type: 'linear' as const,
        display: true,
        position: 'right' as const,
        title: {
          display: true,
          text: 'Short Volume (Shares)',
        },
        grid: {
          drawOnChartArea: false,
        },
      },
    },
  };

  return (
    <div className="chart-container">
      <h3>Short Volume Data - {ticker}</h3>
      <div className="chart-wrapper">
        <Bar data={chartData} options={options} />
      </div>
      <div className="chart-info">
        <p><strong>Latest Short Volume Ratio:</strong> {data[data.length - 1]?.shortVolumeRatio?.toFixed(2)}%</p>
        <p><strong>Latest Short Volume:</strong> {data[data.length - 1]?.shortVolume?.toLocaleString()} shares</p>
        <p><strong>Latest Total Volume:</strong> {data[data.length - 1]?.totalVolume?.toLocaleString()} shares</p>
      </div>
    </div>
  );
};

export default ShortVolumeChart;
