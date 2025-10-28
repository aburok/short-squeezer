import React, { useState } from 'react';
import { Alert, Button, ButtonGroup, Card, Col, Container, Form, Row } from 'react-bootstrap';
import './FinraDashboard.css';
import FinraShortInterestChart from './FinraShortInterestChart';
import MovableDateRangePicker from './MovableDateRangePicker';

interface FinraDashboardProps {
    defaultSymbol?: string;
}

const FinraDashboard: React.FC<FinraDashboardProps> = ({
    defaultSymbol = 'AAPL'
}) => {
    const [symbol, setSymbol] = useState(defaultSymbol);
    const [startDate, setStartDate] = useState<Date | null>(null);
    const [endDate, setEndDate] = useState<Date | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const handleSymbolChange = (newSymbol: string) => {
        setSymbol(newSymbol.toUpperCase());
    };

    const handleDateRangeChange = (start: Date, end: Date) => {
        setStartDate(start);
        setEndDate(end);
    };

    const handleRefresh = () => {
        setIsLoading(true);
        // Trigger a re-render by updating a dummy state
        setTimeout(() => setIsLoading(false), 100);
    };

    const popularSymbols = [
        'AAPL', 'TSLA', 'GME', 'AMC', 'MSFT', 'AMZN', 'GOOGL', 'META', 'NFLX', 'NVDA',
        'SPY', 'QQQ', 'IWM', 'ARKK', 'PLTR', 'ROKU', 'ZOOM', 'PTON', 'BYND', 'NIO'
    ];

    return (
        <Container fluid className="py-3">
            {/* Controls */}
            <Card className="mb-4">
                <Card.Header>
                    <h4 className="mb-0">FINRA Regulatory Data Dashboard</h4>
                </Card.Header>
                <Card.Body>
                    <Row className="mb-3">
                        <Col md={4}>
                            <Form.Group>
                                <Form.Label>Stock Symbol</Form.Label>
                                <Form.Control
                                    type="text"
                                    value={symbol}
                                    onChange={(e) => handleSymbolChange(e.target.value)}
                                    placeholder="Enter symbol (e.g., AAPL)"
                                    className="text-uppercase"
                                />
                            </Form.Group>
                        </Col>

                        <Col md={6}>
                            <Form.Group>
                                <Form.Label>Date Range (Optional)</Form.Label>
                                <MovableDateRangePicker
                                    onDateRangeChange={handleDateRangeChange}
                                    initialStartDate={startDate || undefined}
                                    initialEndDate={endDate || undefined}
                                />
                            </Form.Group>
                        </Col>

                        <Col md={2}>
                            <Form.Group>
                                <Form.Label>&nbsp;</Form.Label>
                                <Button
                                    onClick={handleRefresh}
                                    disabled={isLoading}
                                    className="w-100"
                                >
                                    {isLoading ? 'Refreshing...' : 'Refresh Data'}
                                </Button>
                            </Form.Group>
                        </Col>
                    </Row>

                    <Row>
                        <Col>
                            <div className="mb-2">
                                <small className="text-muted">Popular Symbols:</small>
                            </div>
                            <ButtonGroup size="sm" className="flex-wrap">
                                {popularSymbols.map((sym) => (
                                    <Button
                                        key={sym}
                                        variant={symbol === sym ? "primary" : "outline-secondary"}
                                        onClick={() => setSymbol(sym)}
                                        className="me-1 mb-1"
                                    >
                                        {sym}
                                    </Button>
                                ))}
                            </ButtonGroup>
                        </Col>
                    </Row>
                </Card.Body>
            </Card>

            {/* Charts */}
            {symbol && (
                <Card className="mb-4">
                    <Card.Header>
                        <h5 className="mb-0">FINRA Short Interest Data - {symbol}</h5>
                    </Card.Header>
                    <Card.Body>
                        <FinraShortInterestChart
                            symbol={symbol}
                            startDate={startDate ? startDate.toISOString().split('T')[0] : undefined}
                            endDate={endDate ? endDate.toISOString().split('T')[0] : undefined}
                        />
                    </Card.Body>
                </Card>
            )}

            {/* Information Panel */}
            <Card>
                <Card.Header>
                    <h5 className="mb-0">About FINRA Short Interest Data</h5>
                </Card.Header>
                <Card.Body>
                    <Alert variant="info">
                        <Alert.Heading>Data Source</Alert.Heading>
                        <p className="mb-0">Financial Industry Regulatory Authority (FINRA) - Official regulatory database</p>
                    </Alert>

                    <Row>
                        <Col md={6}>
                            <h6>Update Frequency</h6>
                            <p>Twice monthly (mid-month and month-end settlement dates)</p>

                            <h6>Coverage</h6>
                            <p>All exchange-listed and OTC equity securities</p>
                        </Col>

                        <Col md={6}>
                            <h6>Key Metrics</h6>
                            <ul>
                                <li><strong>Short Interest %:</strong> Percentage of outstanding shares sold short</li>
                                <li><strong>Days to Cover:</strong> Number of days to cover short positions at average volume</li>
                                <li><strong>Market Value:</strong> Dollar value of short positions</li>
                                <li><strong>Shares Outstanding:</strong> Total shares available for trading</li>
                                <li><strong>Average Daily Volume:</strong> Average trading volume over the period</li>
                            </ul>
                        </Col>
                    </Row>

                    <Alert variant="warning">
                        <strong>Note:</strong> High short interest percentages and low days to cover can indicate potential short squeeze scenarios.
                    </Alert>
                </Card.Body>
            </Card>
        </Container>
    );
};

export default FinraDashboard;