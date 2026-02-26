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

    function formatAmpHours(value) {
        if (typeof value !== "number") value = parseFloat(value);
        if (!isFinite(value)) value = 0;
        return value.toFixed(1) + " AH";
    }

    function updateBatteryEfficiency(model) {
        if (!model) return;

        var powerModules = model.PowerModulesCount || 0;
        $("#bePowerModules").text(powerModules);

        var efficiency = model.EfficiencyScorePercent;
        if (typeof efficiency !== "number") efficiency = parseInt(efficiency, 10);
        if (!isFinite(efficiency)) efficiency = 0;
        $("#beEfficiencyScore").text(efficiency + "%");

        // Correct binding:
        // - Total power in Amps     => TotalRemainingCapacitySum (sum of SlowChangingDataA_RemainingCapacity)
        // - Utilized power          => InUseRemainingCapacitySum (sum of Remaining_Capacity for EventCode==18)
        var totalAmps = model.TotalRemainingCapacitySum;
        var utilizedAmps = model.InUseRemainingCapacitySum;

        $("#beInUseAmps").text(formatAmpHours(totalAmps));
        $("#beTotalAmps").text(formatAmpHours(utilizedAmps));

        var inUsePct = clampPercent(parseFloat(model.InUsePercent || efficiency || 0));
        var idlePct = clampPercent(parseFloat(model.IdlePercent || (100 - inUsePct) || 0));

        var fixed = fixRounding(inUsePct, idlePct);
        inUsePct = fixed.a;
        idlePct = fixed.b;

        $("#beBarInUse").css("width", inUsePct + "%");
        $("#beBarIdle").css("width", idlePct + "%");
    }

    function loadBatteryEfficiency(url) {
        var $widget = $("#batteryEfficiencyWidget");
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                updateBatteryEfficiency(data);
                $widget.find('.loader-overlay').fadeOut(300);
            },
            error: function (xhr, status, error) {
                if (window.console && window.console.error) {
                    console.error("Failed to load Battery Efficiency:", status, error);
                }
                $widget.find('.loader-overlay').fadeOut(300);
            }
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.BatteryEfficiency = {
        init: function (options) {
            if (!options || !options.url) return;
            loadBatteryEfficiency(options.url);
        }
    };

})(window, window.jQuery);