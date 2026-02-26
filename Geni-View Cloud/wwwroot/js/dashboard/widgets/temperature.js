(function (window, $) {
    "use strict";

    function clampPercent(value) {
        if (value < 0) return 0;
        if (value > 100) return 100;
        return value;
    }

    function fixRounding(p1, p2) {
        var sum = p1 + p2;
        if (sum > 0 && sum !== 100) {
            p2 = clampPercent(100 - p1);
        }
        return { a: p1, b: p2 };
    }

    function updateTemperature(model) {
        if (!model) return;

        var chargingNormal = model.ChargingNormalCount || 0;
        var chargingWarning = model.ChargingWarningCount || 0;
        var dischargingNormal = model.DischargingNormalCount || 0;
        var dischargingWarning = model.DischargingWarningCount || 0;

        $("#tempChargingNormalCount").text(chargingNormal);
        $("#tempChargingWarningCount").text(chargingWarning);
        $("#tempDischargingNormalCount").text(dischargingNormal);
        $("#tempDischargingWarningCount").text(dischargingWarning);

        var powerModules = model.PowerModulesCount || 0;
        $("#tempPowerModules").text(powerModules);

        var score = model.EfficiencyScorePercent;
        if (typeof score !== "number") score = 0;
        $("#tempEfficiencyScore").text((isFinite(score) ? score : 0) + "%");

        var normalPct = clampPercent(parseFloat(model.NormalPercent || 0));
        var warningPct = clampPercent(parseFloat(model.WarningPercent || 0));

        var fixed = fixRounding(normalPct, warningPct);
        normalPct = fixed.a;
        warningPct = fixed.b;

        $("#tempBarNormal").css("width", normalPct + "%");
        $("#tempBarWarning").css("width", warningPct + "%");
        $("#tempBarAlert").css("width", "0%");
    }

    function loadTemperature(url) {
        var $widget = $("#temperatureWidget");
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                updateTemperature(data);
                $widget.find('.loader-overlay').fadeOut(300);
            },
            error: function (xhr, status, error) {
                if (window.console && window.console.error) {
                    console.error("Failed to load Temperature:", status, error);
                }
                $widget.find('.loader-overlay').fadeOut(300);
            }
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.Temperature = {
        init: function (options) {
            if (!options || !options.url) return;
            loadTemperature(options.url);
        }
    };

})(window, window.jQuery);