// Member dashboard charts (Chart.js). Reads #dashboard-data and resolves the
// design-token palette at runtime so the charts follow the active theme.
(function () {
    var el = document.getElementById('dashboard-data');
    if (!el || typeof Chart === 'undefined') return;

    var data;
    try { data = JSON.parse(el.textContent); } catch (e) { return; }

    var css = getComputedStyle(document.documentElement);
    function tok(name, fallback) {
        var v = css.getPropertyValue(name);
        return (v && v.trim()) || fallback;
    }
    function withAlpha(c, a) {
        c = (c || '').trim();
        if (c[0] === '#') {
            var n = c.slice(1);
            if (n.length === 3) n = n.split('').map(function (x) { return x + x; }).join('');
            var r = parseInt(n.slice(0, 2), 16), g = parseInt(n.slice(2, 4), 16), b = parseInt(n.slice(4, 6), 16);
            return 'rgba(' + r + ',' + g + ',' + b + ',' + a + ')';
        }
        var m = c.match(/rgba?\(([^)]+)\)/);
        if (m) {
            var p = m[1].split(',').slice(0, 3).map(function (s) { return s.trim(); });
            return 'rgba(' + p.join(',') + ',' + a + ')';
        }
        return c;
    }

    var textMuted     = tok('--color-text-muted', '#6B7799');
    var textSecondary = tok('--color-text-secondary', '#9AA6C4');
    var borderSoft    = tok('--color-border-soft', '#2A3554');
    var accent        = tok('--color-accent', '#6C8CFF');
    var success       = tok('--color-success', '#3FBF8F');
    var surface       = tok('--color-surface', '#131A2E');

    Chart.defaults.color = textMuted;
    var fontFamily = css.getPropertyValue('--font-ui').trim();
    if (fontFamily) Chart.defaults.font.family = fontFamily;

    // ---- "Your sky, over time": bars = nights/month, line = objects discovered all-time ----
    var activityCanvas = document.getElementById('chart-activity');
    if (activityCanvas && data.activity && data.activity.length) {
        new Chart(activityCanvas, {
            data: {
                labels: data.activity.map(function (d) { return d.label; }),
                datasets: [
                    {
                        type: 'line',
                        label: 'Objects discovered',
                        data: data.activity.map(function (d) { return d.cumulative; }),
                        borderColor: success,
                        backgroundColor: withAlpha(success, 0.14),
                        borderWidth: 2,
                        fill: true,
                        tension: 0.35,
                        pointRadius: 2,
                        pointHoverRadius: 4,
                        order: 1
                    },
                    {
                        type: 'bar',
                        label: 'Nights logged',
                        data: data.activity.map(function (d) { return d.count; }),
                        backgroundColor: withAlpha(accent, 0.85),
                        borderRadius: 4,
                        maxBarThickness: 26,
                        order: 2
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { position: 'bottom', labels: { color: textSecondary, boxWidth: 12, padding: 14 } },
                    tooltip: { displayColors: false }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { color: textSecondary } },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0, color: textMuted },
                        grid: { color: borderSoft }
                    }
                }
            }
        });
    }

    // ---- Sky composition (doughnut) ----
    var skyCanvas = document.getElementById('chart-sky');
    if (skyCanvas && data.sky && data.sky.length) {
        new Chart(skyCanvas, {
            type: 'doughnut',
            data: {
                labels: data.sky.map(function (d) { return d.label; }),
                datasets: [{
                    data: data.sky.map(function (d) { return d.count; }),
                    backgroundColor: data.sky.map(function (d) { return tok(d.colorVar, accent); }),
                    borderColor: surface,
                    borderWidth: 2,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    // A server-rendered legend sits under the canvas already.
                    legend: { display: false },
                    tooltip: { displayColors: false }
                }
            }
        });
    }
})();
