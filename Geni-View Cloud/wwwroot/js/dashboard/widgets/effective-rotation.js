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

    function updateEffectiveRotation(model) {
        if (!model) return;

        console.log("Effective Rotation Model:", model);

        var goodCount = model.GoodCount || 0;
        var avgCount = model.AverageCount || 0;
        var poorCount = model.PoorCount || 0;

        var total = model.TotalCount || (goodCount + avgCount + poorCount) || 0;

        var goodPct = clampPercent(parseFloat(model.GoodPercent || 0));
        var avgPct = clampPercent(parseFloat(model.AveragePercent || 0));
        var poorPct = clampPercent(parseFloat(model.PoorPercent || 0));

        var fixed = fixRounding(goodPct, avgPct, poorPct);
        goodPct = fixed.a;
        avgPct = fixed.b;
        poorPct = fixed.c;

        $("#rotationGoodCount").text(goodCount);
        $("#rotationAverageCount").text(avgCount);
        $("#rotationPoorCount").text(poorCount);

        $("#rotationBarGood").css("width", goodPct + "%");
        $("#rotationBarAverage").css("width", avgPct + "%");
        $("#rotationBarPoor").css("width", poorPct + "%");

        var powerModules = model.PowerModulesCount;
        if (typeof powerModules !== "number") {
            powerModules = total;
        }
        $("#rotationPowerModules").text(powerModules || 0);

        var score = model.EfficiencyScorePercent;
        console.log("Efficiency Score received:", score, "Type:", typeof score);
        
        if (typeof score !== "number") {
            score = 0;
        }
        
        console.log("Final Efficiency Score displayed:", score);
        $("#rotationEfficiencyScore").text((isFinite(score) ? score : 0) + "%");
    }

    function loadEffectiveRotation(url) {
        var $widget = $("#effectiveRotationWidget");
        $.ajax({
            type: "GET",
            dataType: "json",
            url: url,
            success: function (data) {
                console.log("Raw AJAX response:", data);
                updateEffectiveRotation(data);
                $widget.find('.loader-overlay').fadeOut(300);
            },
            error: function(xhr, status, error) {
                console.error("Failed to load Effective Rotation:", status, error);
                $widget.find('.loader-overlay').fadeOut(300);
            }
        });
    }

    window.DashboardWidgets = window.DashboardWidgets || {};
    window.DashboardWidgets.EffectiveRotation = {
        init: function (options) {
            if (!options || !options.url) return;
            console.log("Initializing Effective Rotation widget with URL:", options.url);
            loadEffectiveRotation(options.url);
        }
    };

})(window, window.jQuery);
