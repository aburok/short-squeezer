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

interface RedditMentionsData {
    date: string;
    mentions: number;
    sentimentScore?: number;
    sentimentLabel?: string;
    subreddit?: string;
    upvotes?: number;
    comments?: number;
    engagementScore?: number;
}

interface RedditMentionsChartProps {
    data: RedditMentionsData[];
    ticker: string;
    isLoading?: boolean;
}

const RedditMentionsChart: React.FC<RedditMentionsChartProps> = ({
    data,
    ticker,
    isLoading = false
}) => {
    if (isLoading) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Reddit Mentions - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <div className="spinner-border" role="status">
                            <span className="visually-hidden">Loading...</span>
                        </div>
                        <p className="mt-2">Loading Reddit mentions data...</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    if (!data || data.length === 0) {
        return (
            <Card>
                <Card.Header>
                    <h5 className="mb-0">Reddit Mentions - {ticker}</h5>
                </Card.Header>
                <Card.Body>
                    <div className="text-center py-4">
                        <p className="text-muted">No Reddit mentions data available for {ticker}</p>
                    </div>
                </Card.Body>
            </Card>
        );
    }

    const chartData = {
        labels: data.map(item => new Date(item.date).toLocaleDateString()),
        datasets: [
            {
                label: 'Mentions Count',
                data: data.map(item => item.mentions),
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.1,
                yAxisID: 'y'
            },
            {
                label: 'Sentiment Score',
                data: data.map(item => item.sentimentScore || 0),
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1,
                yAxisID: 'y1'
            },
            {
                label: 'Engagement Score',
                data: data.map(item => item.engagementScore || 0),
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
                text: `Reddit Mentions Analysis - ${ticker}`
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
                            `Sentiment: ${item.sentimentLabel || 'N/A'}`,
                            `Subreddit: ${item.subreddit || 'N/A'}`,
                            `Upvotes: ${item.upvotes || 0}`,
                            `Comments: ${item.comments || 0}`
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
                    text: 'Mentions Count'
                }
            },
            y1: {
                type: 'linear' as const,
                display: true,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'Sentiment Score'
                },
                grid: {
                    drawOnChartArea: false,
                },
                min: -1,
                max: 1
            },
            y2: {
                type: 'linear' as const,
                display: false,
                position: 'right' as const,
                title: {
                    display: true,
                    text: 'Engagement Score'
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
                <h5 className="mb-0">Reddit Mentions - {ticker}</h5>
            </Card.Header>
            <Card.Body>
                <div style={{ height: '400px' }}>
                    <Line data={chartData} options={options} />
                </div>
                <div className="mt-3">
                    <small className="text-muted">
                        Reddit mentions data shows social media sentiment and engagement around {ticker}.
                        High mentions with positive sentiment can indicate retail investor interest.
                    </small>
                </div>
            </Card.Body>
        </Card>
    );
};

export default RedditMentionsChart;


