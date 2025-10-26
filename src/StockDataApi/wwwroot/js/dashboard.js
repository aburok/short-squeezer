// Dashboard JavaScript

// Chart objects
let shortInterestChart = null;
let shortVolumeChart = null;
let borrowFeeChart = null;

// Initialize when the document is ready
document.addEventListener('DOMContentLoaded', function () {
    // Set up event listeners
    document.getElementById('loadShortInterestBtn').addEventListener('click', loadShortInterestData);
    document.getElementById('loadShortVolumeBtn').addEventListener('click', loadShortVolumeData);
    document.getElementById('loadBorrowFeeBtn').addEventListener('click', loadBorrowFeeData);
    document.getElementById('refreshDataBtn').addEventListener('click', refreshTickerData);
    document.getElementById('refreshAllTickersBtn').addEventListener('click', refreshAllTickers);
    
    // Initialize charts with empty data
    initializeCharts();
});

// Initialize all charts
function initializeCharts() {
    // Short Interest Chart
    const shortInterestCtx = document.getElementById('shortInterestChart').getContext('2d');
    shortInterestChart = new Chart(shortInterestCtx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Short Interest %',
                data: [],
                borderColor: 'rgb(255, 99, 132)',
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                tension: 0.1
            }]
        },
        options: {
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
        }
    });

    // Short Volume Chart
    const shortVolumeCtx = document.getElementById('shortVolumeChart').getContext('2d');
    shortVolumeChart = new Chart(shortVolumeCtx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Short Volume %',
                data: [],
                borderColor: 'rgb(54, 162, 235)',
                backgroundColor: 'rgba(54, 162, 235, 0.2)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Short Volume %'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Date'
                    }
                }
            }
        }
    });

    // Borrow Fee Chart
    const borrowFeeCtx = document.getElementById('borrowFeeChart').getContext('2d');
    borrowFeeChart = new Chart(borrowFeeCtx, {
        type: 'line',
        data: {
            labels: [],
            datasets: [{
                label: 'Borrow Fee %',
                data: [],
                borderColor: 'rgb(75, 192, 192)',
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Borrow Fee %'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Date'
                    }
                }
            }
        }
    });
}

// Load Short Interest Data
function loadShortInterestData() {
    const symbol = document.getElementById('shortInterestSymbol').value.trim().toUpperCase();
    if (!symbol) {
        showStatus('Please enter a stock symbol', 'danger');
        return;
    }

    fetch(`/api/ShortInterest/${symbol}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            updateShortInterestChart(data, symbol);
        })
        .catch(error => {
            console.error('Error fetching short interest data:', error);
            showStatus(`Error loading short interest data: ${error.message}`, 'danger');
        });
}

// Update Short Interest Chart
function updateShortInterestChart(data, symbol) {
    if (!data || data.length === 0) {
        showStatus(`No short interest data found for ${symbol}`, 'warning');
        return;
    }

    const dates = data.map(item => new Date(item.date).toLocaleDateString());
    const shortInterestValues = data.map(item => item.shortInterest);

    shortInterestChart.data.labels = dates;
    shortInterestChart.data.datasets[0].data = shortInterestValues;
    shortInterestChart.data.datasets[0].label = `${symbol} Short Interest %`;
    shortInterestChart.update();

    showStatus(`Loaded short interest data for ${symbol}`, 'success');
}

// Load Short Volume Data
function loadShortVolumeData() {
    const symbol = document.getElementById('shortVolumeSymbol').value.trim().toUpperCase();
    const exchange = document.getElementById('shortVolumeExchange').value;
    
    if (!symbol) {
        showStatus('Please enter a stock symbol', 'danger');
        return;
    }

    fetch(`/api/ShortVolume/${symbol}?exchange=${exchange}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            updateShortVolumeChart(data, symbol);
        })
        .catch(error => {
            console.error('Error fetching short volume data:', error);
            showStatus(`Error loading short volume data: ${error.message}`, 'danger');
        });
}

// Update Short Volume Chart
function updateShortVolumeChart(data, symbol) {
    if (!data || data.length === 0) {
        showStatus(`No short volume data found for ${symbol}`, 'warning');
        return;
    }

    const dates = data.map(item => new Date(item.date).toLocaleDateString());
    const shortVolumeValues = data.map(item => item.shortVolumePercent);

    shortVolumeChart.data.labels = dates;
    shortVolumeChart.data.datasets[0].data = shortVolumeValues;
    shortVolumeChart.data.datasets[0].label = `${symbol} Short Volume %`;
    shortVolumeChart.update();

    showStatus(`Loaded short volume data for ${symbol}`, 'success');
}

// Load Borrow Fee Data
function loadBorrowFeeData() {
    const symbol = document.getElementById('borrowFeeSymbol').value.trim().toUpperCase();
    const exchange = document.getElementById('borrowFeeExchange').value;
    
    if (!symbol) {
        showStatus('Please enter a stock symbol', 'danger');
        return;
    }

    fetch(`/api/BorrowFee/${symbol}?exchange=${exchange}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            updateBorrowFeeChart(data, symbol);
        })
        .catch(error => {
            console.error('Error fetching borrow fee data:', error);
            showStatus(`Error loading borrow fee data: ${error.message}`, 'danger');
        });
}

// Update Borrow Fee Chart
function updateBorrowFeeChart(data, symbol) {
    if (!data || data.length === 0) {
        showStatus(`No borrow fee data found for ${symbol}`, 'warning');
        return;
    }

    const dates = data.map(item => new Date(item.date).toLocaleDateString());
    const feeValues = data.map(item => item.fee);

    borrowFeeChart.data.labels = dates;
    borrowFeeChart.data.datasets[0].data = feeValues;
    borrowFeeChart.data.datasets[0].label = `${symbol} Borrow Fee %`;
    borrowFeeChart.update();

    showStatus(`Loaded borrow fee data for ${symbol}`, 'success');
}

// Refresh Ticker Data
function refreshTickerData() {
    const symbol = document.getElementById('refreshSymbol').value.trim().toUpperCase();
    const exchange = document.getElementById('refreshExchange').value;
    
    if (!symbol) {
        showStatus('Please enter a stock symbol', 'danger');
        return;
    }

    showStatus(`Refreshing data for ${symbol}...`, 'info');

    fetch(`/api/Tickers/refresh/${symbol}?exchange=${exchange}`, {
        method: 'POST'
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text();
        })
        .then(data => {
            showStatus(data, 'success');
        })
        .catch(error => {
            console.error('Error refreshing ticker data:', error);
            showStatus(`Error refreshing data: ${error.message}`, 'danger');
        });
}

// Refresh All Tickers
function refreshAllTickers() {
    showStatus('Refreshing all tickers... This may take a while.', 'info');

    fetch('/api/Tickers/refresh-all', {
        method: 'POST'
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text();
        })
        .then(data => {
            showStatus(data, 'success');
        })
        .catch(error => {
            console.error('Error refreshing all tickers:', error);
            showStatus(`Error refreshing all tickers: ${error.message}`, 'danger');
        });
}

// Show status message
function showStatus(message, type) {
    const statusElement = document.getElementById('refreshStatus');
    statusElement.textContent = message;
    statusElement.className = `alert alert-${type}`;
    statusElement.style.display = 'block';
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        statusElement.style.display = 'none';
    }, 5000);
}

// Made with Bob
