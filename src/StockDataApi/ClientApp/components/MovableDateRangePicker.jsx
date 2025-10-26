import React, { useState, useCallback, useRef, useEffect } from 'react';
import moment from 'moment';

const MovableDateRangePicker = ({ onDateRangeChange, initialStartDate, initialEndDate, minDate, maxDate }) => {
  const containerRef = useRef(null);
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState(null);
  const [dragOffset, setDragOffset] = useState(0);
  const [dragType, setDragType] = useState(null); // 'move', 'resize-start', 'resize-end'

  // Calculate default values
  const now = moment();
  const defaultStart = initialStartDate ? moment(initialStartDate) : moment().subtract(30, 'days');
  const defaultEnd = initialEndDate ? moment(initialEndDate) : now;
  const defaultMin = minDate ? moment(minDate) : moment().subtract(1, 'year');
  const defaultMax = maxDate ? moment(maxDate) : now;

  const [selectedRange, setSelectedRange] = useState({
    start: defaultStart,
    end: defaultEnd
  });

  const [minRange, setMinRange] = useState(defaultMin);
  const [maxRange, setMaxRange] = useState(defaultMax);

  // Calculate positions and dimensions
  const totalDays = maxRange.diff(minRange, 'days');
  const rangeDays = selectedRange.end.diff(selectedRange.start, 'days');
  const startPosition = selectedRange.start.diff(minRange, 'days');
  const endPosition = selectedRange.end.diff(minRange, 'days');

  const startPercent = (startPosition / totalDays) * 100;
  const endPercent = (endPosition / totalDays) * 100;
  const rangePercent = (rangeDays / totalDays) * 100;

  const handleMouseDown = useCallback((e) => {
    setIsDragging(true);
    setDragStart(e.clientX);
    setDragOffset(0);
    setDragType('move');
  }, []);

  const handleUnifiedMouseMove = useCallback((e) => {
    if (!isDragging || !containerRef.current || !dragType) return;

    const containerRect = containerRef.current.getBoundingClientRect();
    const containerWidth = containerRect.width;
    const mouseX = e.clientX - containerRect.left;
    const mousePercent = (mouseX / containerWidth) * 100;

    if (dragType === 'move') {
      // Move the entire range
      const daysFromStart = Math.round((mousePercent / 100) * totalDays);
      const newStartDate = minRange.clone().add(daysFromStart, 'days');
      const newEndDate = newStartDate.clone().add(rangeDays, 'days');

      // Check bounds
      if (newStartDate.isBefore(minRange) || newEndDate.isAfter(maxRange)) {
        return;
      }

      setSelectedRange({
        start: newStartDate,
        end: newEndDate
      });

      if (onDateRangeChange) {
        onDateRangeChange(newStartDate.toDate(), newEndDate.toDate());
      }
    } else if (dragType.startsWith('resize-')) {
      // Resize the range
      const daysFromStart = Math.round((mousePercent / 100) * totalDays);
      const newDate = minRange.clone().add(daysFromStart, 'days');

      let newStart = selectedRange.start;
      let newEnd = selectedRange.end;

      if (dragType === 'resize-start') {
        newStart = newDate;
        if (newStart.isAfter(selectedRange.end)) {
          newStart = selectedRange.end.clone().subtract(1, 'day');
        }
        if (newStart.isBefore(minRange)) {
          newStart = minRange.clone();
        }
      } else if (dragType === 'resize-end') {
        newEnd = newDate;
        if (newEnd.isBefore(selectedRange.start)) {
          newEnd = selectedRange.start.clone().add(1, 'day');
        }
        if (newEnd.isAfter(maxRange)) {
          newEnd = maxRange.clone();
        }
      }

      setSelectedRange({
        start: newStart,
        end: newEnd
      });

      if (onDateRangeChange) {
        onDateRangeChange(newStart.toDate(), newEnd.toDate());
      }
    }
  }, [isDragging, dragType, totalDays, rangeDays, minRange, maxRange, selectedRange, onDateRangeChange]);

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
    setDragStart(null);
    setDragOffset(0);
    setDragType(null);
  }, []);

  const handleResizeStart = useCallback((e, handle) => {
    e.stopPropagation();
    setIsDragging(true);
    setDragStart(e.clientX);
    setDragType(`resize-${handle}`);
  }, []);


  useEffect(() => {
    if (isDragging) {
      document.addEventListener('mousemove', handleUnifiedMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
      return () => {
        document.removeEventListener('mousemove', handleUnifiedMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
      };
    }
  }, [isDragging, handleUnifiedMouseMove, handleMouseUp]);

  const formatDate = (date) => date.format('MMM DD, YYYY');

  return (
    <div className="movable-date-range-picker">
      <div className="date-range-info">
        <span className="start-date">{formatDate(selectedRange.start)}</span>
        <span className="separator">to</span>
        <span className="end-date">{formatDate(selectedRange.end)}</span>
        <span className="duration">({rangeDays} days)</span>
      </div>

      <div className="timeline-container" ref={containerRef}>
        <div className="timeline-track">
          <div className="timeline-background" />
          <div 
            className="timeline-range"
            style={{
              left: `${startPercent}%`,
              width: `${rangePercent}%`
            }}
            onMouseDown={handleMouseDown}
          />
          <div 
            className="timeline-handle timeline-handle-start"
            style={{ left: `${startPercent}%` }}
            onMouseDown={(e) => handleResizeStart(e, 'start')}
          />
          <div 
            className="timeline-handle timeline-handle-end"
            style={{ left: `${endPercent}%` }}
            onMouseDown={(e) => handleResizeStart(e, 'end')}
          />
        </div>
        
        <div className="timeline-labels">
          <span className="min-label">{formatDate(minRange)}</span>
          <span className="max-label">{formatDate(maxRange)}</span>
        </div>
      </div>

      <div className="date-range-controls">
        <button 
          className="control-button"
          onClick={() => {
            const newStart = selectedRange.start.clone().subtract(7, 'days');
            const newEnd = selectedRange.end.clone().subtract(7, 'days');
            
            if (newStart.isAfterOrSame(minRange)) {
              setSelectedRange({ start: newStart, end: newEnd });
              if (onDateRangeChange) {
                onDateRangeChange(newStart.toDate(), newEnd.toDate());
              }
            }
          }}
          disabled={selectedRange.start.isSameOrBefore(minRange)}
        >
          ← Move Back
        </button>
        
        <button 
          className="control-button"
          onClick={() => {
            const newStart = selectedRange.start.clone().add(7, 'days');
            const newEnd = selectedRange.end.clone().add(7, 'days');
            
            if (newEnd.isSameOrBefore(maxRange)) {
              setSelectedRange({ start: newStart, end: newEnd });
              if (onDateRangeChange) {
                onDateRangeChange(newStart.toDate(), newEnd.toDate());
              }
            }
          }}
          disabled={selectedRange.end.isSameOrAfter(maxRange)}
        >
          Move Forward →
        </button>
      </div>

      <div className="preset-buttons">
        <button 
          className="preset-button"
          onClick={() => {
            const newEnd = moment();
            const newStart = newEnd.clone().subtract(7, 'days');
            setSelectedRange({ start: newStart, end: newEnd });
            if (onDateRangeChange) {
              onDateRangeChange(newStart.toDate(), newEnd.toDate());
            }
          }}
        >
          Last 7 Days
        </button>
        
        <button 
          className="preset-button"
          onClick={() => {
            const newEnd = moment();
            const newStart = newEnd.clone().subtract(30, 'days');
            setSelectedRange({ start: newStart, end: newEnd });
            if (onDateRangeChange) {
              onDateRangeChange(newStart.toDate(), newEnd.toDate());
            }
          }}
        >
          Last 30 Days
        </button>
        
        <button 
          className="preset-button"
          onClick={() => {
            const newEnd = moment();
            const newStart = newEnd.clone().subtract(90, 'days');
            setSelectedRange({ start: newStart, end: newEnd });
            if (onDateRangeChange) {
              onDateRangeChange(newStart.toDate(), newEnd.toDate());
            }
          }}
        >
          Last 90 Days
        </button>
      </div>
    </div>
  );
};

export default MovableDateRangePicker;
