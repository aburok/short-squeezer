import React from 'react';
import { createRoot } from 'react-dom/client';
import Dashboard from './components/Dashboard';

// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', () => {
  console.log('DOM loaded, looking for dashboard container...');
  
  // Find the container element where we'll mount our React app
  const dashboardContainer = document.getElementById('dashboard-container');
  
  if (dashboardContainer) {
    console.log('Found dashboard container, mounting React Dashboard...');
    
    try {
      // Create a root for React to render into
      const dashboardRoot = createRoot(dashboardContainer);
      
      // Render the Dashboard component
      dashboardRoot.render(<Dashboard />);
      
      console.log('React Dashboard mounted successfully!');
    } catch (error) {
      console.error('Error mounting React Dashboard:', error);
    }
  } else {
    console.error('Dashboard container not found!');
  }
});