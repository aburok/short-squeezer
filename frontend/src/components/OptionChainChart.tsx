import {
    CategoryScale,
    Chart as ChartJS,
    Filler,
    Legend,
    LinearScale,
    LineElement,
    PointElement,
    Title,
    Tooltip
} from 'chart.js';
import React from 'react';
import { Card } from 'react-bootstrap';
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

interface OptionChainData {
    date: string;
    expirationDate: string;
    strikePrice: number;
    optionType: string;
    volume: number;
    openInterest: number;
    bid?: number;
    ask?: number;
    lastPrice?: number;
    impliedVolatility?: number;
    delta?: number;
    gamma?: number;
    theta?: number;
    vega?: number;
}

interface OptionChainChartProps {
    data: OptionChainData[];
    ticker: string;
    isLoading?: boolean;
}

const OptionChainChart: React.FC<OptionChainChartProps> = ({
    data,
    ticker,
    isLoading = false
}) => {
    if (isLoading) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Option Chain - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                        <p className="mt-2">Loading option chain data...</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    if (!data || data.length === 0) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Option Chain - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <p className="text-muted">No option chain data available for {ticker}</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    // Separate calls and puts
    const calls = data.filter(item => item.optionType.toLowerCase() === 'call');
    const puts = data.filter(item => item.optionType.toLowerCase() === 'put');

    const chartData = {
        labels: data.map(item => `${item.strikePrice} (${new Date(item.expirationDate).toLocaleDateString()})`),
        datasets: [
            {
                label: 'Call Volume',
                data: calls.map(item => item.volume),
                borderColor: 'rgb(34, 197, 94)',
                backgroundColor: 'rgba(34, 197, 94, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y'
            },
            {
                label: 'Put Volume',
                data: puts.map(item => item.volume),
                borderColor: 'rgb(239, 68, 68)',
                backgroundColor: 'rgba(239, 68, 68, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y'
            },
            {
                label: 'Call Open Interest',
                data: calls.map(item => item.openInterest),
                borderColor: 'rgb(34, 197, 94)',
                backgroundColor: 'rgba(34, 197, 94, 0.05)',
                borderWidth: 1,
                fill: true,
                tension: 0.1,
                yAxisID: 'y1'
            },
            {
                label: 'Put Open Interest',
                data: puts.map(item => item.openInterest),
                borderColor: 'rgb(239, 68, 68)',
                backgroundColor: 'rgba(239, 68, 68, 0.05)',
                borderWidth: 1,
                fill: true,
                tension: 0.1,
                yAxisID: 'y1'
            }
        ]
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        interaction: {
            mode: 'index' as const,
            intersect: false,
        },
        plugins: {
            title: {
                display: true,
                text: `Option Chain Analysis - ${ticker}`
            },
            legend: {
                position: 'top' as const,
            },
            tooltip: {
                callbacks: {
                    afterBody: (context: any) => {
                        const dataIndex = context[0].dataIndex;
                        const item = data[dataIndex];
                        return [
                            `Strike: $${item.strikePrice}`,
                            `Type: ${item.optionType}`,
                            `Bid: $${item.bid || 'N/A'}`,
                            `Ask: $${item.ask || 'N/A'}`,
                            `Last Price: $${item.lastPrice || 'N/A'}`,
                            `IV: ${item.impliedVolatility ? (item.impliedVolatility * 100).toFixed(2) + '%' : 'N/A'}`,
                            `Delta: ${item.delta || 'N/A'}`,
                            `Gamma: ${item.gamma || 'N/A'}`,
                            `Theta: ${item.theta || 'N/A'}`,
                            `Vega: ${item.vega || 'N/A'}`
                        ];
                    }
                }
            }
        },
        scales: {
            x: {
                display: true,
                title: {
                    display: true,
                    text: 'Strike Price (Expiration)'
                }
            },
            y: {
                type: 'linear' as const,
                display: true,
                position: 'left' as const,
                title: {
                    display: true,
                    text: 'Volume'
                }
            },
            y1: {
                type: 'linear' as const,
                display: true,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'Open Interest'
                },
                grid: {
                    drawOnChartArea: false,
                },
            }
        }
    };

    return (
        <Card>
            <Card.Header>
                <h5 className="mb-0">Option Chain - {ticker}</h5>
            </Card.Header>
            <Card.Body>
                <div style={{ height: '400px' }}>
                    <Line data={chartData} options={options} />
                </div>
                <div className="mt-3">
                    <small className="text-muted">
                        Option chain data shows call/put volume and open interest for {ticker}.
                        High call volume relative to puts can indicate bullish sentiment.
                    </small>
                </div>
            </Card.Body>
        </Card>
    );
};

export default OptionChainChart;

