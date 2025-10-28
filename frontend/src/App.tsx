import { Container, Nav, Navbar } from 'react-bootstrap';
import { Link, Route, BrowserRouter as Router, Routes } from 'react-router-dom';
import './App.css';
import Dashboard from './components/Dashboard';
import FinraDashboard from './components/FinraDashboard';

function App() {
  return (
    <Router>
      <div className="App">
        <Navbar bg="dark" variant="dark" expand="lg" className="mb-4">
          <Container fluid>
            <Navbar.Brand href="/">Stock Data API</Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" />
            <Navbar.Collapse id="basic-navbar-nav">
              <Nav className="me-auto">
                <Nav.Link as={Link} to="/">Main Dashboard</Nav.Link>
                <Nav.Link as={Link} to="/finra">FINRA Dashboard</Nav.Link>
              </Nav>
            </Navbar.Collapse>
          </Container>
        </Navbar>

        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/finra" element={<FinraDashboard />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;