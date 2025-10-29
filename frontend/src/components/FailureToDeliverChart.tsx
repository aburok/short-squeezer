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

interface FailureToDeliverData {
    date: string;
    failureToDeliver: number;
    price: number;
    volume: number;
    settlementDate?: string;
    cusip?: string;
    companyName?: string;
}

interface FailureToDeliverChartProps {
    data: FailureToDeliverData[];
    ticker: string;
    isLoading?: boolean;
}

const FailureToDeliverChart: React.FC<FailureToDeliverChartProps> = ({
    data,
    ticker,
    isLoading = false
}) => {
    if (isLoading) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Failure to Deliver - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                        <p className="mt-2">Loading failure to deliver data...</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    if (!data || data.length === 0) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Failure to Deliver - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <p className="text-muted">No failure to deliver data available for {ticker}</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    const chartData = {
        labels: data.map(item => new Date(item.date).toLocaleDateString()),
        datasets: [
            {
                label: 'Failure to Deliver (Shares)',
                data: data.map(item => item.failureToDeliver),
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.1,
                yAxisID: 'y'
            },
            {
                label: 'Price ($)',
                data: data.map(item => item.price),
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y1'
            },
            {
                label: 'Volume',
                data: data.map(item => item.volume),
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y2'
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
                text: `Failure to Deliver Analysis - ${ticker}`
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
                            `Settlement Date: ${item.settlementDate ? new Date(item.settlementDate).toLocaleDateString() : 'N/A'}`,
                            `CUSIP: ${item.cusip || 'N/A'}`,
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
                    text: 'Failure to Deliver (Shares)'
                }
            },
            y1: {
                type: 'linear' as const,
                display: true,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'Price ($)'
                },
                grid: {
                    drawOnChartArea: false,
                },
            },
            y2: {
                type: 'linear' as const,
                display: false,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'Volume'
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
                <h5 className="mb-0">Failure to Deliver - {ticker}</h5>
            </Card.Header>
            <Card.Body>
                <div style={{ height: '400px' }}>
                    <Line data={chartData} options={options} />
                </div>
                <div className="mt-3">
                    <small className="text-muted">
                        Failure to deliver data shows shares that were sold but not delivered on settlement date.
                        High failure to deliver can indicate potential short squeeze scenarios.
                    </small>
                </div>
            </Card.Body>
        </Card>
    );
};

export default FailureToDeliverChart;


