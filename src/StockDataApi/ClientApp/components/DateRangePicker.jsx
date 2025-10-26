import React, { useState } from 'react';

const DateRangePicker = ({ onDateRangeChange }) => {
  // Calculate default date range (last 30 days)
  const now = new Date();
  const startDate = new Date(now);
  startDate.setDate(now.getDate() - 30);
  
  const [startDateValue, setStartDateValue] = useState(startDate.toISOString().split('T')[0]);
  const [endDateValue, setEndDateValue] = useState(now.toISOString().split('T')[0]);

  const handleStartDateChange = (e) => {
    const newStartDate = e.target.value;
    setStartDateValue(newStartDate);
    
    if (onDateRangeChange) {
      onDateRangeChange(new Date(newStartDate), new Date(endDateValue));
    }
  };

  const handleEndDateChange = (e) => {
    const newEndDate = e.target.value;
    setEndDateValue(newEndDate);
    
    if (onDateRangeChange) {
      onDateRangeChange(new Date(startDateValue), new Date(newEndDate));
    }
  };

  return (
    <div className="date-range-picker">
      <div className="date-inputs">
        <div className="date-input-group">
          <label htmlFor="start-date">Start Date:</label>
          <input
            type="date"
            id="start-date"
            value={startDateValue}
            onChange={handleStartDateChange}
            className="date-input"
          />
        </div>
        
        <div className="date-input-group">
          <label htmlFor="end-date">End Date:</label>
          <input
            type="date"
            id="end-date"
            value={endDateValue}
            onChange={handleEndDateChange}
            className="date-input"
          />
        </div>
      </div>
      
      <div className="date-range-info">
        <p>Selected range: {startDateValue} to {endDateValue}</p>
      </div>
    </div>
  );
};

export default DateRangePicker;