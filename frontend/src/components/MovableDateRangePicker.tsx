import React, { useState, useCallback, useRef, useEffect } from 'react';
import moment from 'moment';
import './MovableDateRangePicker.css';

interface MovableDateRangePickerProps {
  onDateRangeChange: (startDate: Date, endDate: Date) => void;
  initialStartDate?: Date;
  initialEndDate?: Date;
  minDate?: Date;
  maxDate?: Date;
}

const MovableDateRangePicker: React.FC<MovableDateRangePickerProps> = ({
  onDateRangeChange,
  initialStartDate,
  initialEndDate,
  minDate,
  maxDate
}) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [, setDragStart] = useState<number | null>(null);
  const [, setDragOffset] = useState(0);
  const [dragType, setDragType] = useState<'move' | 'resize-start' | 'resize-end' | null>(null);

  // Default to last 30 days if no initial dates provided
  const defaultStartDate = initialStartDate || moment().subtract(30, 'days').toDate();
  const defaultEndDate = initialEndDate || moment().toDate();

  const [selectedRange, setSelectedRange] = useState({
    start: defaultStartDate,
    end: defaultEndDate
  });

  const [minRange] = useState(minDate || moment().subtract(1, 'year').toDate());
  const [maxRange] = useState(maxDate || moment().toDate());

  const totalDays = moment(maxRange).diff(moment(minRange), 'days');
  const rangeDays = moment(selectedRange.end).diff(moment(selectedRange.start), 'days');
  const startOffset = moment(selectedRange.start).diff(moment(minRange), 'days');
  const endOffset = moment(selectedRange.end).diff(moment(minRange), 'days');

  const startPercent = (startOffset / totalDays) * 100;
  const endPercent = (endOffset / totalDays) * 100;
  const rangePercent = (rangeDays / totalDays) * 100;

  const handleMouseDown = useCallback((e: React.MouseEvent) => {
    setIsDragging(true);
    setDragStart(e.clientX);
    setDragOffset(0);
    setDragType('move');
  }, []);

  const handleResizeStart = useCallback((e: React.MouseEvent, handle: 'start' | 'end') => {
    e.stopPropagation();
    setIsDragging(true);
    setDragStart(e.clientX);
    setDragType(`resize-${handle}` as 'resize-start' | 'resize-end');
  }, []);

  const handleMouseUp = useCallback(() => {
    setIsDragging(false);
    setDragStart(null);
    setDragOffset(0);
    setDragType(null);
  }, []);

  const handleUnifiedMouseMove = useCallback((e: MouseEvent) => {
    if (!isDragging || !containerRef.current || !dragType) return;

    const containerRect = containerRef.current.getBoundingClientRect();
    const containerWidth = containerRect.width;
    const mouseX = e.clientX - containerRect.left;
    const mousePercent = (mouseX / containerWidth) * 100;

    if (dragType === 'move') {
      const newStartPercent = Math.max(0, Math.min(100 - rangePercent, startPercent + (mousePercent - startPercent)));
      const newEndPercent = newStartPercent + rangePercent;

      const newStartDate = moment(minRange).add((newStartPercent / 100) * totalDays, 'days').toDate();
      const newEndDate = moment(minRange).add((newEndPercent / 100) * totalDays, 'days').toDate();

      setSelectedRange({ start: newStartDate, end: newEndDate });
      onDateRangeChange(newStartDate, newEndDate);
    } else if (dragType.startsWith('resize-')) {
      const handleType = dragType.split('-')[1] as 'start' | 'end';
      
      if (handleType === 'start') {
        const newStartPercent = Math.max(0, Math.min(endPercent - 1, mousePercent));
        const newStartDate = moment(minRange).add((newStartPercent / 100) * totalDays, 'days').toDate();
        
        setSelectedRange(prev => ({ ...prev, start: newStartDate }));
        onDateRangeChange(newStartDate, selectedRange.end);
      } else if (handleType === 'end') {
        const newEndPercent = Math.max(startPercent + 1, Math.min(100, mousePercent));
        const newEndDate = moment(minRange).add((newEndPercent / 100) * totalDays, 'days').toDate();
        
        setSelectedRange(prev => ({ ...prev, end: newEndDate }));
        onDateRangeChange(selectedRange.start, newEndDate);
      }
    }
  }, [isDragging, dragType, totalDays, rangeDays, minRange, maxRange, selectedRange, onDateRangeChange, startPercent, endPercent]);

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

  const moveRange = (days: number) => {
    const newStart = moment(selectedRange.start).add(days, 'days').toDate();
    const newEnd = moment(selectedRange.end).add(days, 'days').toDate();
    
    // Check bounds
    if (moment(newStart).isBefore(moment(minRange)) || moment(newEnd).isAfter(moment(maxRange))) {
      return;
    }
    
    setSelectedRange({ start: newStart, end: newEnd });
    onDateRangeChange(newStart, newEnd);
  };

  const setPresetRange = (days: number) => {
    const end = moment().toDate();
    const start = moment().subtract(days, 'days').toDate();
    
    setSelectedRange({ start, end });
    onDateRangeChange(start, end);
  };

  return (
    <div className="movable-date-range-picker">
      <div className="date-controls-row">
        <div className="date-range-info">
          <div className="date-display">
            <strong>Start:</strong> {moment(selectedRange.start).format('MMM DD, YYYY')}
          </div>
          <div className="date-display">
            <strong>End:</strong> {moment(selectedRange.end).format('MMM DD, YYYY')}
          </div>
          <div className="date-display">
            <strong>Duration:</strong> {rangeDays} days
          </div>
        </div>

        <div className="timeline-controls">
          <div className="control-buttons">
            <button className="control-button" onClick={() => moveRange(-7)}>
              ← Move Back 7 Days
            </button>
            <button className="control-button" onClick={() => moveRange(7)}>
              Move Forward 7 Days →
            </button>
          </div>
          
          <div className="preset-buttons">
            <button className="preset-button" onClick={() => setPresetRange(7)}>
              Last 7 Days
            </button>
            <button className="preset-button" onClick={() => setPresetRange(30)}>
              Last 30 Days
            </button>
            <button className="preset-button" onClick={() => setPresetRange(90)}>
              Last 90 Days
            </button>
          </div>
        </div>
      </div>

      <div className="timeline-slider-container" ref={containerRef}>
        <div className="timeline-track">
          <div className="timeline-background">
            <div className="timeline-range"
                 style={{
                   left: `${startPercent}%`,
                   width: `${rangePercent}%`
                 }}
                 onMouseDown={handleMouseDown}>
              <div className="timeline-handle timeline-handle-start"
                   onMouseDown={(e) => handleResizeStart(e, 'start')}></div>
              <div className="timeline-handle timeline-handle-end"
                   onMouseDown={(e) => handleResizeStart(e, 'end')}></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MovableDateRangePicker;
