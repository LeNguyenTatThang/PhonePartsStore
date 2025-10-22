document.addEventListener("DOMContentLoaded", function () {
    // Chart Area
    var areaCtx = document.getElementById("myAreaChart");
    if (areaCtx) {
        new Chart(areaCtx, {
            type: 'line',
            data: { /* dữ liệu area chart */ },
            options: { /* cấu hình */ }
        });
    }

    // Chart Bar
    var barCtx = document.getElementById("myBarChart");
    if (barCtx) {
        new Chart(barCtx, {
            type: 'bar',
            data: { /* dữ liệu bar chart */ },
            options: { /* cấu hình */ }
        });
    }

    // Chart Pie
    var pieCtx = document.getElementById("myPieChart");
    if (pieCtx) {
        new Chart(pieCtx, {
            type: 'doughnut',
            data: { /* dữ liệu pie chart */ },
            options: { /* cấu hình */ }
        });
    }
});
