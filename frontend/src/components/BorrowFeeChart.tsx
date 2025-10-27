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
  Filler,
} from 'chart.js';
import { Line } from 'react-chartjs-2';
import moment from 'moment';

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
}

interface BorrowFeeChartProps {
  data: BorrowFeeData[];
  ticker: string;
  isLoading: boolean;
}

const BorrowFeeChart: React.FC<BorrowFeeChartProps> = ({ data, ticker, isLoading }) => {
  if (isLoading) {
    return (
      <div className="chart-container">
        <div className="chart-header">
          <h3>Borrow Fee - {ticker}</h3>
        </div>
        <div className="chart-loading">
          <p>Loading borrow fee data...</p>
        </div>
      </div>
    );
  }

  if (!data || data.length === 0) {
    return (
      <div className="chart-container">
        <div className="chart-header">
          <h3>Borrow Fee - {ticker}</h3>
        </div>
        <div className="chart-no-data">
          <p>No borrow fee data available for {ticker}</p>
        </div>
      </div>
    );
  }

  // Sort data by date to ensure proper chronological order
  const sortedData = [...data].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

  const chartData = {
    labels: sortedData.map(item => moment(item.date).format('MMM DD')),
    datasets: [
      {
        label: 'Borrow Fee (%)',
        data: sortedData.map(item => item.fee),
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgba(255, 99, 132, 0.1)',
        borderWidth: 2,
        fill: true,
        tension: 0.4,
        pointBackgroundColor: 'rgb(255, 99, 132)',
        pointBorderColor: '#fff',
        pointBorderWidth: 2,
        pointRadius: 4,
        pointHoverRadius: 6,
      },
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
          title: function(context: any) {
            const dataIndex = context[0].dataIndex;
            return moment(sortedData[dataIndex].date).format('MMMM DD, YYYY');
          },
          label: function(context: any) {
            const dataIndex = context.dataIndex;
            const item = sortedData[dataIndex];
            let label = `Borrow Fee: ${item.fee.toFixed(2)}%`;
            if (item.availableShares) {
              label += `\nAvailable Shares: ${item.availableShares.toLocaleString()}`;
            }
            return label;
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

  // Calculate statistics
  const fees = sortedData.map(item => item.fee);
  const minFee = Math.min(...fees);
  const maxFee = Math.max(...fees);
  const avgFee = fees.reduce((sum, fee) => sum + fee, 0) / fees.length;
  const latestFee = sortedData[sortedData.length - 1]?.fee;

  return (
    <div className="chart-container">
      <div className="chart-header">
        <h3>Borrow Fee - {ticker}</h3>
        <div className="chart-stats">
          <div className="stat-item">
            <span className="stat-label">Latest:</span>
            <span className="stat-value">{latestFee?.toFixed(2)}%</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Avg:</span>
            <span className="stat-value">{avgFee.toFixed(2)}%</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Min:</span>
            <span className="stat-value">{minFee.toFixed(2)}%</span>
          </div>
          <div className="stat-item">
            <span className="stat-label">Max:</span>
            <span className="stat-value">{maxFee.toFixed(2)}%</span>
          </div>
        </div>
      </div>
      <div className="chart-content">
        <Line data={chartData} options={options} />
      </div>
    </div>
  );
};

export default BorrowFeeChart;

