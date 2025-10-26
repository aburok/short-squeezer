import React, { useState, useEffect } from 'react';
import { Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend } from 'chart.js';
import { Line } from 'react-chartjs-2';

// Register Chart.js components
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend);

const ShortInterestChart = ({ symbol, startDate, endDate }) => {
  const [chartData, setChartData] = useState({
    labels: [],
    datasets: [
      {
        label: 'Short Interest %',
        data: [],
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgba(255, 99, 132, 0.2)',
        tension: 0.1
      }
    ]
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    if (!symbol) return;
    
    const fetchData = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        // Build URL with date range parameters if provided
        let url = `/api/ShortInterest/${symbol}`;
        if (startDate && endDate) {
          url += `?startDate=${startDate}&endDate=${endDate}`;
        }
        
        const response = await fetch(url);
        
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        
        const data = await response.json();
        
        if (data && data.length > 0) {
          const labels = data.map(item => new Date(item.date).toLocaleDateString());
          const values = data.map(item => item.shortInterest);
          
          setChartData({
            labels,
            datasets: [
              {
                label: `${symbol} Short Interest %`,
                data: values,
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                tension: 0.1
              }
            ]
          });
        } else {
          setError('No data found');
        }
      } catch (err) {
        console.error('Error fetching short interest data:', err);
        setError(`Error loading data: ${err.message}`);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchData();
  }, [symbol, startDate, endDate]);

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Short Interest %'
        }
      },
      x: {
        title: {
          display: true,
          text: 'Date'
        }
      }
    }
  };

  return (
    <div className="chart-container">
      {isLoading && <div className="loading">Loading...</div>}
      {error && <div className="error">{error}</div>}
      {!isLoading && !error && <Line data={chartData} options={options} />}
    </div>
  );
};

export default ShortInterestChart;

// Made with Bob
