(function (window, $) {
    "use strict";

    function toNumber(v) {
        if (typeof v === "number") return v;
        if (v === null || v === undefined) return 0;
        var n = parseFloat(v);
        return isFinite(n) ? n : 0;
    }

    function clampPercent(v) {
        if (!isFinite(v)) return 0;
        if (v < 0) return 0;
        if (v > 100) return 100;
        return v;
    }

    function getEl(id) {
        return document.getElementById(id);
    }

    function setText(selector, value) {
        var $el = $(selector);
        if ($el.length) $el.text(value);
    }

    function setWidthPxSafe(el, percent) {
        if (!el) return;
        el.style.width = percent + "%";
        el.setAttribute("data-width", percent);
    }

    function computePercentsFromCounts(c1, c2, c3, c4, c5) {
        // Always compute from the 5 values the widget displays.
        var sum = c1 + c2 + c3 + c4 + c5;

        if (sum <= 0) {
            return { p1: 0, p2: 0, p3: 0, p4: 0, p5: 0, sum: 0 };
        }

        // Use floating math then fix drift by forcing last segment to close to 100.
        var p1 = (c1 * 100) / sum;
        var p2 = (c2 * 100) / sum;
        var p3 = (c3 * 100) / sum;
        var p4 = (c4 * 100) / sum;
        var p5 = 100 - (p1 + p2 + p3 + p4);

        p1 = clampPercent(p1);
        p2 = clampPercent(p2);
        p3 = clampPercent(p3);
        p4 = clampPercent(p4);
        p5 = clampPercent(p5);

        // If p5 went negative due to floating drift, rescale first 4.
        if (p5 < 0) {
            var total = p1 + p2 + p3 + p4;
            if (total > 0) {
                var scale = 100 / total;
                p1 = clampPercent(p1 * scale);
                p2 = clampPercent(p2 * scale);
                p3 = clampPercent(p3 * scale);
                p4 = clampPercent(p4 * scale);
                p5 = 0;
            } else {
                p1 = p2 = p3 = p4 = p5 = 0;
            }
        }

        return { p1: p1, p2: p2, p3: p3, p4: p4, p5: p5, sum: sum };
    }

    function ensureBarLayout() {
        // In case CSS isn't applied due to scoping/regression, enforce minimal layout.
        var bar = getEl("batteryStatusWidget");
        if (!bar) return;

        var barRow = bar.querySelector(".battery-status__bar");
        if (!barRow) return;

        // Don't override if already set by CSS, just ensure it's not collapsed.
        if (!barRow.style.display) barRow.style.display = "flex";
        if (!barRow.style.height) barRow.style.height = "10px";
        if (!barRow.style.overflow) barRow.style.overflow = "hidden";
        if (!barRow.style.borderRadius) barRow.style.borderRadius = "999px";
    }

    function updateBatteryStatus(model) {
        if (!model) return;

        ensureBarLayout();

        var powerModules = toNumber(model.PowerModulesCount);

        var onCharging = toNumber(model.OnDeviceChargingCount);
        var onDischarging = toNumber(model.OnDeviceDischargingCount);
        var onIdle = toNumber(model.OnDeviceIdleCount);
        var offCharging = toNumber(model.OffDeviceChargingCount);
        var offIdle = toNumber(model.OffDeviceIdleCount);

        var efficiency = toNumber(model.EfficiencyScorePercent);

        setText("#bsPowerModules", powerModules);
        setText("#bsEfficiencyScore", efficiency);

        setText("#bsOnCharging", onCharging);
        setText("#bsOnDischarging", onDischarging);
        setText("#bsOnIdle", onIdle);
        setText("#bsOffCharging", offCharging);
        setText("#bsOffIdle", offIdle);

        // Compute widths strictly from the 5 widget values.
        var perc = computePercentsFromCounts(onCharging, onDischarging, onIdle, offCharging, offIdle);

        var seg1 = getEl("bsBarOnCharging");
        var seg2 = getEl("bsBarOnDischarging");
        var seg3 = getEl("bsBarOnIdle");
        var seg4 = getEl("bsBarOffCharging");
        var seg5 = getEl("bsBarOffIdle");

        setWidthPxSafe(seg1, perc.p1);
        setWidthPxSafe(seg2, perc.p2);
        setWidthPxSafe(seg3, perc.p3);
        setWidthPxSafe(seg4, perc.p4);
        setWidthPxSafe(seg5, perc.p5);

        // Helpful for quick verification in DevTools (hover bar).
        var widget = getEl("batteryStatusWidget");
        if (widget) {
            widget.setAttribute(
                "title",
                "Counts: " +
                [onCharging, onDischarging, onIdle, offCharging, offIdle].join(", ") +
                " | Sum: " + perc.sum +
                " | Widths(%): " +
                [perc.p1, perc.p2, perc.p3, perc.p4, perc.p5].map(function (x) { return x.toFixed(2); }).join(", ")
            );
        }

        if (window.console && window.console.log) {
            console.log("[BatteryStatus] counts=", {
                onCharging: onCharging,
                onDischarging: onDischarging,
                onIdle: onIdle,
                offCharging: offCharging,
                offIdle: offIdle,
                sum: perc.sum
            }, "widths=", {
                p1: perc.p1,
                p2: perc.p2,
                p3: perc.p3,
                p4: perc.p4,
                p5: perc.p5
            });
        }
    }

    function load(url) {
        return $.ajax({
            type: "GET",
            dataType: "json",
            url: url
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.BatteryStatus = {
        init: function (options) {
            if (!options || !options.url) return;
            var $widget = $("#batteryStatusWidget");

            load(options.url)
                .done(function (data) {
                    updateBatteryStatus(data);
                    $widget.find('.loader-overlay').fadeOut(300);
                })
                .fail(function (xhr, status, error) {
                    if (window.console && window.console.error) {
                        console.error("Failed to load Battery Status:", status, error);
                    }
                    updateBatteryStatus({
                        PowerModulesCount: 0,
                        OnDeviceChargingCount: 0,
                        OnDeviceDischargingCount: 0,
                        OnDeviceIdleCount: 0,
                        OffDeviceChargingCount: 0,
                        OffDeviceIdleCount: 0,
                        EfficiencyScorePercent: 0
                    });
                    $widget.find('.loader-overlay').fadeOut(300);
                });
        }
    };

})(window, window.jQuery);