(function (window, $) {
    "use strict";

    function clampPercent(value) {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }

    function fixRounding(p1, p2, p3) {
        var sum = p1 + p2 + p3;
        if (sum > 0 && sum !== 100) {
            p3 = clampPercent(100 - p1 - p2);
        }
        return { a: p1, b: p2, c: p3 };
    }

    function updateCycleStatus(model) {
        if (!model) return;

        var lowCount = model.LowCount || 0;
        var highCount = model.HighCount || 0;
        var eolCount = model.EndOfLifeCount || 0;

        var total = model.TotalCount || (lowCount + highCount + eolCount) || 0;

        var lowPct = clampPercent(parseFloat(model.LowPercent || 0));
        var highPct = clampPercent(parseFloat(model.HighPercent || 0));
        var eolPct = clampPercent(parseFloat(model.EndOfLifePercent || 0));

        var fixed = fixRounding(lowPct, highPct, eolPct);
        lowPct = fixed.a;
        highPct = fixed.b;
        eolPct = fixed.c;

        $("#cycleLowCount").text(lowCount);
        $("#cycleHighCount").text(highCount);
        $("#cycleEndOfLifeCount").text(eolCount);

        $("#cycleBarLow").css("width", lowPct + "%");
        $("#cycleBarHigh").css("width", highPct + "%");
        $("#cycleBarEol").css("width", eolPct + "%");

        var powerModules = model.PowerModulesCount;
        if (typeof powerModules !== "number") {
            powerModules = total;
        }
        $("#cyclePowerModules").text(powerModules || 0);

        var avg = model.AverageCycleCount;
        if (typeof avg !== "number") {
            avg = Math.round((lowCount + highCount + eolCount) / 3);
        }
        $("#cycleAverageCount").text(isFinite(avg) ? avg : 0);
    }

    function loadCycleStatus(url) {
        var $widget = $("#cycleStatusWidget");
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                updateCycleStatus(data);
                $widget.find('.loader-overlay').fadeOut(300);
            },
            error: function() {
                $widget.find('.loader-overlay').fadeOut(300);
            }
        });
    }

    // Expose a very small API so the Razor view can provide the correct MVC URL.
    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.CycleStatus = {
        init: function (options) {
            if (!options || !options.url) return;
            loadCycleStatus(options.url);
        }
    };

})(window, window.jQuery);
