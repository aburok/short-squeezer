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

interface StockSplitData {
    date: string;
    splitRatio: string;
    splitFactor: number;
    fromFactor: number;
    toFactor: number;
    exDate?: string;
    recordDate?: string;
    payableDate?: string;
    announcementDate?: string;
    companyName?: string;
}

interface StockSplitChartProps {
    data: StockSplitData[];
    ticker: string;
    isLoading?: boolean;
}

const StockSplitChart: React.FC<StockSplitChartProps> = ({
    data,
    ticker,
    isLoading = false
}) => {
    if (isLoading) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Stock Splits - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                        <p className="mt-2">Loading stock split data...</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    if (!data || data.length === 0) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Stock Splits - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <p className="text-muted">No stock split data available for {ticker}</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    const chartData = {
        labels: data.map(item => new Date(item.date).toLocaleDateString()),
        datasets: [
            {
                label: 'Split Factor',
                data: data.map(item => item.splitFactor),
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.1,
                yAxisID: 'y'
            },
            {
                label: 'From Factor',
                data: data.map(item => item.fromFactor),
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y1'
            },
            {
                label: 'To Factor',
                data: data.map(item => item.toFactor),
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.1)',
                borderWidth: 2,
                fill: false,
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
                text: `Stock Split History - ${ticker}`
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
                            `Split Ratio: ${item.splitRatio}`,
                            `Ex-Date: ${item.exDate ? new Date(item.exDate).toLocaleDateString() : 'N/A'}`,
                            `Record Date: ${item.recordDate ? new Date(item.recordDate).toLocaleDateString() : 'N/A'}`,
                            `Payable Date: ${item.payableDate ? new Date(item.payableDate).toLocaleDateString() : 'N/A'}`,
                            `Announcement Date: ${item.announcementDate ? new Date(item.announcementDate).toLocaleDateString() : 'N/A'}`,
                            `Company: ${item.companyName || 'N/A'}`
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
                    text: 'Date'
                }
            },
            y: {
                type: 'linear' as const,
                display: true,
                position: 'left' as const,
                title: {
                    display: true,
                    text: 'Split Factor'
                }
            },
            y1: {
                type: 'linear' as const,
                display: true,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'From/To Factor'
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
                <h5 className="mb-0">Stock Splits - {ticker}</h5>
            </Card.Header>
            <Card.Body>
                <div style={{ height: '400px' }}>
                    <Line data={chartData} options={options} />
                </div>
                <div className="mt-3">
                    <small className="text-muted">
                        Stock split history for {ticker}. Stock splits increase the number of shares outstanding
                        while proportionally reducing the share price, making shares more accessible to retail investors.
                    </small>
                </div>
            </Card.Body>
        </Card>
    );
};

export default StockSplitChart;
