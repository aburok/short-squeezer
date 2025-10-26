// Site JavaScript

// Enable tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Initialize any Bootstrap components
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });
    
    // Fix chart sizing issues
    Chart.defaults.maintainAspectRatio = false;
    
    // Wrap all canvas elements in chart containers if not already wrapped
    document.querySelectorAll('canvas').forEach(function(canvas) {
        if (!canvas.parentElement.classList.contains('chart-container')) {
            var container = document.createElement('div');
            container.className = 'chart-container';
            canvas.parentNode.insertBefore(container, canvas);
            container.appendChild(canvas);
        }
    });
});

// Made with Bob
