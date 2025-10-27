import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Dashboard from './components/Dashboard';
import FinraDashboard from './components/FinraDashboard';
import './App.css';

function App() {
  return (
    <Router>
      <div className="App">
        <nav className="app-nav">
          <div className="nav-container">
            <Link to="/" className="nav-link">
              Main Dashboard
            </Link>
            <Link to="/finra" className="nav-link">
              FINRA Dashboard
            </Link>
          </div>
        </nav>
        
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/finra" element={<FinraDashboard />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;