import { useState } from 'react';
import { Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './App.css';
import Dashboard from './components/Dashboard';
import FinraDashboard from './components/FinraDashboard';
import TopNavigation from './components/TopNavigation';

function App() {
  const [selectedTicker, setSelectedTicker] = useState('');

  const handleTickerSelect = (ticker: string) => {
    setSelectedTicker(ticker);
  };

  return (
    <Router>
      <div className="App">
        <TopNavigation 
          selectedTicker={selectedTicker} 
          onTickerSelect={handleTickerSelect} 
        />

        <Routes>
          <Route path="/" element={<Dashboard selectedTicker={selectedTicker} onTickerSelect={handleTickerSelect} />} />
          <Route path="/finra" element={<FinraDashboard selectedTicker={selectedTicker} onTickerSelect={handleTickerSelect} />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;