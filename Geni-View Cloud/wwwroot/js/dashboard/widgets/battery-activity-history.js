(function (window, $) {
    "use strict";

    function toNumber(v) {
        if (typeof v === "number") return v;
        var n = parseFloat(v);
        return isFinite(n) ? n : 0;
    }

    function buildChartData(model) {
        var days = (model && model.Days) ? model.Days : [];

        var labels = [];
        var online = [];
        var offline = [];

        for (var i = 0; i < days.length; i++) {
            labels.push(days[i].Label || "");
            online.push(toNumber(days[i].BatteriesOnline));
            offline.push(toNumber(days[i].BatteriesOffline));
        }

        return {
            labels: labels,
            online: online,
            offline: offline
        };
    }

    function getMaxValue(chartData) {
        var max = 0;

        for (var i = 0; i < chartData.online.length; i++) {
            if (chartData.online[i] > max) max = chartData.online[i];
        }
        for (var j = 0; j < chartData.offline.length; j++) {
            if (chartData.offline[j] > max) max = chartData.offline[j];
        }

        return max;
    }

    function getNiceStep(maxY) {
        // Prefer an "index-like" set of ticks (0..max in clean increments).
        // Goal: ~5-6 ticks maximum.
        if (maxY <= 5) return 1;
        if (maxY <= 10) return 2;
        if (maxY <= 25) return 5;
        if (maxY <= 50) return 10;
        if (maxY <= 100) return 20;
        return Math.ceil(maxY / 5 / 10) * 10; // coarse step for larger values
    }

    function calcYAxis(chartData) {
        var max = getMaxValue(chartData);

        // Always show a visible Y scale.
        // If max is 0 (no data), still show 0..10.
        var maxY = max <= 0 ? 10 : max;

        var step = getNiceStep(maxY);

        // Round maxY up to the next tick.
        maxY = Math.ceil(maxY / step) * step;

        // Ensure at least 2 steps so the axis doesn't collapse visually.
        if (maxY < step * 2) {
            maxY = step * 2;
        }

        return { maxY: maxY, step: step };
    }

    function destroyChart(existing) {
        if (existing && typeof existing.destroy === "function") {
            existing.destroy();
        }
        return null;
    }

    function renderChart(canvas, chartData, yAxis) {
        var ctx = canvas.getContext("2d");

        return new window.Chart(ctx, {
            type: "bar",
            data: {
                labels: chartData.labels,
                datasets: [
                    {
                        label: "Battery Online",
                        data: chartData.online,
                        backgroundColor: "#60A5FA",
                        borderRadius: 8,
                        borderSkipped: false,
                        barThickness: 10,
                        maxBarThickness: 12,
                        categoryPercentage: 0.65,
                        barPercentage: 0.85
                    },
                    {
                        label: "Battery Offline",
                        data: chartData.offline,
                        backgroundColor: "#A78BFA",
                        borderRadius: 8,
                        borderSkipped: false,
                        barThickness: 10,
                        maxBarThickness: 12,
                        categoryPercentage: 0.65,
                        barPercentage: 0.85
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        enabled: true,
                        backgroundColor: "rgba(17,24,39,.92)",
                        titleColor: "#fff",
                        bodyColor: "#fff",
                        padding: 10,
                        displayColors: true
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            color: "#6b7280",
                            font: { size: 10 },
                            maxRotation: 0,
                            minRotation: 0
                        }
                    },
                    y: {
                        beginAtZero: true,
                        min: 0,
                        max: yAxis.maxY,
                        grid: { color: "rgba(107,114,128,.18)" },
                        ticks: {
                            color: "#6b7280",
                            font: { size: 10 },
                            stepSize: yAxis.step,
                            precision: 0
                        }
                    }
                }
            }
        });
    }

    function load(url) {
        return $.ajax({
            type: "GET",
            dataType: "json",
            url: url
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};

    window.DashboardWidgets.BatteryActivityHistory = (function () {
        var chart = null;

        function init(options) {
            if (!options || !options.url) return;
            var $widget = $("#batteryActivityHistoryWidget");

            if (!window.Chart) {
                if (window.console && window.console.error) {
                    console.error("Chart.js not loaded. BatteryActivityHistory widget skipped.");
                }
                $widget.find('.loader-overlay').fadeOut(300);
                return;
            }

            var canvas = document.getElementById("batteryActivityHistoryCanvas");
            if (!canvas) {
                $widget.find('.loader-overlay').fadeOut(300);
                return;
            }

            // Ensure we have a consistent height like the UX card.
            if (canvas.parentElement) {
                canvas.parentElement.style.height = "220px";
            }

            load(options.url)
                .done(function (model) {
                    var chartData = buildChartData(model);
                    var yAxis = calcYAxis(chartData);

                    chart = destroyChart(chart);
                    chart = renderChart(canvas, chartData, yAxis);
                    $widget.find('.loader-overlay').fadeOut(300);
                })
                .fail(function() {
                    $widget.find('.loader-overlay').fadeOut(300);
                });
        }

        return {
            init: init
        };
    })();

})(window, window.jQuery);
