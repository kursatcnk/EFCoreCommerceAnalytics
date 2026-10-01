// Dashboard grafikleri. Her view component'in grafik kodunu ayrı ayrı yazmak (ve Chart.js'i sayfaya dört kez
// yüklemek) yerine, <canvas data-chart="..."> etiketleri burada tek yerden çiziliyor.
(function () {
    "use strict";

    var palette = ["#4d83ff", "#19d895", "#ffaf00", "#ff6258", "#8862e0", "#2bc0e4", "#ff9f43", "#6c7a89", "#e83e8c", "#20c997"];

    function readJson(value) {
        try {
            return JSON.parse(value || "[]");
        } catch (e) {
            return [];
        }
    }

    function render(canvas) {
        var type = canvas.getAttribute("data-chart");
        var labels = readJson(canvas.getAttribute("data-labels"));
        var values = readJson(canvas.getAttribute("data-values"));
        var axisChart = type === "line" || type === "bar";

        new window.Chart(canvas, {
            type: type,
            data: {
                labels: labels,
                datasets: [{
                    label: canvas.getAttribute("data-label") || "",
                    data: values,
                    backgroundColor: axisChart ? "rgba(77, 131, 255, 0.15)" : labels.map(function (_, i) { return palette[i % palette.length]; }),
                    borderColor: axisChart ? "#4d83ff" : "#ffffff",
                    borderWidth: axisChart ? 2 : 1,
                    fill: type === "line",
                    tension: 0.3,
                    cubicInterpolationMode: "monotone",
                    pointRadius: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: !axisChart, position: "bottom" } },
                scales: axisChart ? { y: { beginAtZero: true, ticks: { precision: 0 } } } : undefined
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        if (!window.Chart) {
            return;
        }
        document.querySelectorAll("canvas[data-chart]").forEach(render);
    });
})();
