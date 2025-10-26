import React from 'react';
import { createRoot } from 'react-dom/client';

// Simple test component
const TestComponent = () => {
  return (
    <div style={{ padding: '20px', backgroundColor: '#f0f0f0', margin: '20px' }}>
      <h2>React is working!</h2>
      <p>If you can see this, React has mounted successfully.</p>
      <p>Current time: {new Date().toLocaleString()}</p>
    </div>
  );
};

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', () => {
  console.log('DOM loaded, looking for dashboard container...');
  
  // Find the container element where we'll mount our React app
  const dashboardContainer = document.getElementById('dashboard-container');
  
  if (dashboardContainer) {
    console.log('Found dashboard container, mounting React...');
    
    try {
      // Create a root for React to render into
      const dashboardRoot = createRoot(dashboardContainer);
      
      // Render the test component
      dashboardRoot.render(<TestComponent />);
      
      console.log('React mounted successfully!');
    } catch (error) {
      console.error('Error mounting React:', error);
    }
  } else {
    console.error('Dashboard container not found!');
  }
});