import React, { useState, useEffect } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  BarChart,
  Bar,
  Area,
  AreaChart,
  ComposedChart
} from 'recharts';

interface FinraShortInterestData {
  date: string;
  settlementDate: string;
  shortInterest: number;
  shortInterestPercent: number;
  marketValue: number;
  sharesOutstanding: number;
  avgDailyVolume: number;
  days2Cover: number;
  symbol?: string;
}

interface FinraShortInterestChartProps {
  symbol: string;
  startDate?: string;
  endDate?: string;
}

const FinraShortInterestChart: React.FC<FinraShortInterestChartProps> = ({ 
  symbol, 
  startDate, 
  endDate 
}) => {
  const [data, setData] = useState<FinraShortInterestData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setLoading(true);
        setError(null);
        
        let url = `/api/finra/short-interest/${symbol}`;
        const params = new URLSearchParams();
        
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        if (params.toString()) {
          url += `?${params.toString()}`;
        }

        const response = await fetch(url);
        if (!response.ok) {
          throw new Error(`Failed to fetch FINRA data: ${response.statusText}`);
        }

        const result = await response.json();
        setData(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to fetch FINRA data');
        console.error('Error fetching FINRA data:', err);
      } finally {
        setLoading(false);
      }
    };

    if (symbol) {
      fetchData();
    }
  }, [symbol, startDate, endDate]);

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: '2-digit'
    });
  };

  const formatNumber = (value: number) => {
    if (value >= 1e9) {
      return `${(value / 1e9).toFixed(1)}B`;
    } else if (value >= 1e6) {
      return `${(value / 1e6).toFixed(1)}M`;
    } else if (value >= 1e3) {
      return `${(value / 1e3).toFixed(1)}K`;
    }
    return value.toString();
  };

  const formatCurrency = (value: number) => {
    if (value >= 1e9) {
      return `$${(value / 1e9).toFixed(1)}B`;
    } else if (value >= 1e6) {
      return `$${(value / 1e6).toFixed(1)}M`;
    } else if (value >= 1e3) {
      return `$${(value / 1e3).toFixed(1)}K`;
    }
    return `$${value.toFixed(0)}`;
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">Loading FINRA data...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-red-500">Error: {error}</div>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-gray-500">No FINRA data available for {symbol}</div>
      </div>
    );
  }

  const chartData = data.map(item => ({
    ...item,
    formattedDate: formatDate(item.settlementDate),
    formattedShortInterest: formatNumber(item.shortInterest),
    formattedMarketValue: formatCurrency(item.marketValue),
    formattedSharesOutstanding: formatNumber(item.sharesOutstanding),
    formattedAvgDailyVolume: formatNumber(item.avgDailyVolume)
  }));

  return (
    <div className="space-y-6">
      {/* Short Interest Percentage Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Short Interest Percentage - {symbol}
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="formattedDate" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={60}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => `${value}%`}
            />
            <Tooltip 
              formatter={(value: number) => [`${value.toFixed(2)}%`, 'Short Interest %']}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Line 
              type="monotone" 
              dataKey="shortInterestPercent" 
              stroke="#ef4444" 
              strokeWidth={2}
              dot={{ fill: '#ef4444', strokeWidth: 2, r: 4 }}
              name="Short Interest %"
            />
          </LineChart>
        </ResponsiveContainer>
      </div>

      {/* Days to Cover Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Days to Cover - {symbol}
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <AreaChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="formattedDate" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={60}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => `${value} days`}
            />
            <Tooltip 
              formatter={(value: number) => [`${value.toFixed(2)} days`, 'Days to Cover']}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Area 
              type="monotone" 
              dataKey="days2Cover" 
              stroke="#f59e0b" 
              fill="#f59e0b"
              fillOpacity={0.3}
              strokeWidth={2}
              name="Days to Cover"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      {/* Market Value Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Short Interest Market Value - {symbol}
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="formattedDate" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={60}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => formatCurrency(value)}
            />
            <Tooltip 
              formatter={(value: number) => [formatCurrency(value), 'Market Value']}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Bar 
              dataKey="marketValue" 
              fill="#3b82f6"
              name="Market Value"
            />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Shares Outstanding vs Short Interest Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Shares Outstanding vs Short Interest - {symbol}
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <ComposedChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="formattedDate" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={60}
            />
            <YAxis 
              yAxisId="left"
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => formatNumber(value)}
            />
            <YAxis 
              yAxisId="right" 
              orientation="right"
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => formatNumber(value)}
            />
            <Tooltip 
              formatter={(value: number, name: string) => [
                formatNumber(value), 
                name === 'sharesOutstanding' ? 'Shares Outstanding' : 'Short Interest'
              ]}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Bar 
              yAxisId="left"
              dataKey="sharesOutstanding" 
              fill="#10b981"
              name="Shares Outstanding"
            />
            <Line 
              yAxisId="right"
              type="monotone" 
              dataKey="shortInterest" 
              stroke="#ef4444" 
              strokeWidth={2}
              dot={{ fill: '#ef4444', strokeWidth: 2, r: 4 }}
              name="Short Interest"
            />
          </ComposedChart>
        </ResponsiveContainer>
      </div>

      {/* Average Daily Volume Chart */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Average Daily Volume - {symbol}
        </h3>
        <ResponsiveContainer width="100%" height={300}>
          <AreaChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="formattedDate" 
              tick={{ fontSize: 12 }}
              angle={-45}
              textAnchor="end"
              height={60}
            />
            <YAxis 
              tick={{ fontSize: 12 }}
              tickFormatter={(value) => formatNumber(value)}
            />
            <Tooltip 
              formatter={(value: number) => [formatNumber(value), 'Avg Daily Volume']}
              labelFormatter={(label) => `Date: ${label}`}
            />
            <Legend />
            <Area 
              type="monotone" 
              dataKey="avgDailyVolume" 
              stroke="#8b5cf6" 
              fill="#8b5cf6"
              fillOpacity={0.3}
              strokeWidth={2}
              name="Avg Daily Volume"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

      {/* Summary Statistics */}
      <div className="bg-white p-6 rounded-lg shadow">
        <h3 className="text-lg font-semibold mb-4 text-gray-800">
          Summary Statistics - {symbol}
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="text-center p-4 bg-red-50 rounded-lg">
            <div className="text-2xl font-bold text-red-600">
              {data.length > 0 ? data[data.length - 1].shortInterestPercent.toFixed(2) : 'N/A'}%
            </div>
            <div className="text-sm text-gray-600">Latest Short Interest %</div>
          </div>
          <div className="text-center p-4 bg-orange-50 rounded-lg">
            <div className="text-2xl font-bold text-orange-600">
              {data.length > 0 ? data[data.length - 1].days2Cover.toFixed(1) : 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Days to Cover</div>
          </div>
          <div className="text-center p-4 bg-blue-50 rounded-lg">
            <div className="text-2xl font-bold text-blue-600">
              {data.length > 0 ? formatCurrency(data[data.length - 1].marketValue) : 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Market Value</div>
          </div>
          <div className="text-center p-4 bg-green-50 rounded-lg">
            <div className="text-2xl font-bold text-green-600">
              {data.length > 0 ? formatNumber(data[data.length - 1].shortInterest) : 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Short Interest</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default FinraShortInterestChart;
