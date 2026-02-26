(function (window, $) {
    "use strict";

    var state = {
        widget: "soc", // soc | cycle | rotation | temp
        cardKey: null,
        pageNumber: 1,
        pageSize: 10,
        search: ""
    };

    var activeRequestId = 0;

    function openPopup() {
        $("#dashboardPopupOverlay").addClass("is-open");
        $("body").css("overflow", "hidden");
    }

    function closePopup() {
        $("#dashboardPopupOverlay").removeClass("is-open");
        $("body").css("overflow", "");
    }

    function debounce(fn, ms) {
        var t = null;
        return function () {
            var args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(null, args); }, ms);
        };
    }

    function showLoading() {
        $("#popupLoader").show();
        var $tb = $("#gvPopupTbody").empty();
        $tb.append("<tr><td colspan='10' style='text-align:center;'>&nbsp;</td></tr>");

        $("#gvPopupPager").empty();
        $("#gvPopupInfo").text("");
        $("#gvPopupPowerModules").text("Power Modules : -");
    }

    function formatDate(v) {
        if (!v) return "-";

        if (typeof v === "string") {
            var m = /\/Date\((\-?\d+)\)\//.exec(v);
            if (m && m[1]) {
                v = parseInt(m[1], 10);
            }
        }

        var d = new Date(v);
        if (isNaN(d.getTime())) return "-";

        var mm = (d.getMonth() + 1).toString().padStart(2, "0");
        var dd = d.getDate().toString().padStart(2, "0");
        var yyyy = d.getFullYear();
        return dd + "/" + mm + "/" + yyyy;
    }

    function escapeHtml(v) {
        if (v == null) return "";
        return String(v)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/\"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function getSocClass(v) {
        if (v == null) return "";
        if (v > 70) return "gv-soc--high";
        if (v >= 30) return "gv-soc--low";
        return "gv-soc--charge";
    }

    function renderCycleCount(v) {
        if (v == null) return "-";
        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='/Content/Img/widgets/popupcc.svg' alt='' />" +
            "<span>" + escapeHtml(v) + "</span>" +
            "</span>"
        );
    }

    function renderTemp(v) {
        if (v == null) return "-";

        var icon = v > 30
            ? "/Content/Img/widgets/popuptemp1.svg"
            : "/Content/Img/widgets/poptemp2.svg";

        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='" + icon + "' alt='' />" +
            "<span>" + escapeHtml(v) + "&deg;C</span>" +
            "</span>"
        );
    }

    function getStatusIcon(status) {
        var s = (status || "").toString();

        if (s === "Charging") {
            return "/Content/Img/widgets/oncharging.svg";
        }

        if (s === "Discharging") {
            return "/Content/Img/widgets/ondischarging.svg";
        }

        if (s === "Idle") {
            return "/Content/Img/widgets/popupidle.svg";
        }

        if (s === "Offline") {
            return "/Content/Img/widgets/Offline.svg";
        }

        if (s === "Charge Now") {
            return "/Content/Img/widgets/CN.svg";
        }

        return null;
    }

    function renderStatus(status) {
        var text = status || "-";
        var icon = getStatusIcon(text);

        if (!icon) return escapeHtml(text);

        return (
            "<span class='gv-cell'>" +
            "<img class='gv-cell__icon' src='" + icon + "' alt='' />" +
            "<span>" + escapeHtml(text) + "</span>" +
            "</span>"
        );
    }

    function renderRows(items) {
        var $tb = $("#gvPopupTbody").empty();

        if (!items || !items.length) {
            $tb.append("<tr><td colspan='10' style='text-align:center;'>No records found</td></tr>");
            return;
        }

        for (var i = 0; i < items.length; i++) {
            var r = items[i];

            var socText = (r.SoC == null ? "-" : (r.SoC + "%"));
            var socClass = getSocClass(r.SoC);

            $tb.append(
                "<tr>" +
                "<td>" + escapeHtml(r.PowerModules || "") + "</td>" +
                "<td>" + escapeHtml(r.AttachedTo || "-") + "</td>" +
                "<td>" + escapeHtml(r.DeviceType || "-") + "</td>" +
                "<td><span class='" + socClass + "'>" + escapeHtml(socText) + "</span></td>" +
                "<td>" + renderCycleCount(r.CycleCount) + "</td>" +
                "<td>" + renderTemp(r.Temperature) + "</td>" +
                "<td>" + renderStatus(r.Status) + "</td>" +
                "<td>" + escapeHtml(r.LastAttached || "-") + "</td>" +
                "<td>" + escapeHtml(formatDate(r.LastCharged)) + "</td>" +
                "<td>" + escapeHtml(formatDate(r.LastDischarged)) + "</td>" +
                "</tr>"
            );
        }
    }

    function renderPager(total, pageNumber, pageSize) {
        var $pager = $("#gvPopupPager").empty();
        var $info = $("#gvPopupInfo").empty();

        if (!total) {
            $info.text("Showing 0 to 0 out of 0");
            return;
        }

        var start = ((pageNumber - 1) * pageSize) + 1;
        var end = Math.min(pageNumber * pageSize, total);
        $info.text("Showing " + start + " to " + end + " out of " + total);

        var totalPages = Math.ceil(total / pageSize);
        if (totalPages <= 1) return;

        function addBtn(text, page, disabled, current) {
            var $a = $("<a href='#' class='gv-page'></a>").text(text);
            if (disabled) $a.addClass("disabled");
            if (current) $a.addClass("current");
            $a.on("click", function (e) {
                e.preventDefault();
                if (disabled) return;
                state.pageNumber = page;
                loadData();
            });
            $pager.append($a);
        }

        addBtn("<", pageNumber - 1, pageNumber <= 1, false);

        var maxButtons = 4;
        var startPage = Math.max(1, pageNumber - Math.floor(maxButtons / 2));
        var endPage = Math.min(totalPages, startPage + maxButtons - 1);
        startPage = Math.max(1, endPage - maxButtons + 1);

        for (var p = startPage; p <= endPage; p++) {
            addBtn(String(p), p, false, p === pageNumber);
        }

        addBtn(">", pageNumber + 1, pageNumber >= totalPages, false);
    }

    function setHeader(powerModulesCount) {
        $("#gvPopupPowerModules").text("Power Modules : " + (powerModulesCount || 0));
    }

    function getUrl() {
        if (state.widget === "cycle") return "/DashboardPopup/GetCycleStatusPopupData";
        if (state.widget === "rotation") return "/DashboardPopup/GetEffectiveRotationPopupData";
        if (state.widget === "temp") return "/DashboardPopup/GetTemperaturePopupData";
        if (state.widget === "moduleStatus") return "/DashboardPopup/GetBatteryStatusPopupData";
        return "/DashboardPopup/GetSocPopupData";
    }

    function loadData() {
        var requestId = ++activeRequestId;

        showLoading();

        $.ajax({
            type: "GET",
            dataType: "json",
            url: getUrl(),
            data: {
                cardKey: state.cardKey,
                search: state.search,
                pageNumber: state.pageNumber,
                pageSize: state.pageSize
            }
        }).done(function (resp) {
            if (requestId !== activeRequestId) return;

            $("#popupLoader").fadeOut(200);

            var items = resp && resp.Items ? resp.Items : [];
            renderRows(items);
            renderPager(resp.Total || 0, resp.PageNumber || 1, resp.PageSize || state.pageSize);
            setHeader(resp.PowerModulesCount || 0);
        }).fail(function (xhr, status, err) {
            if (requestId !== activeRequestId) return;

            if (window.console && window.console.error) {
                console.error("[Popup] load failed:", status, err);
                console.error("[Popup] HTTP:", (xhr && xhr.status), (xhr && xhr.responseText));
            }

            renderRows([]);
            renderPager(0, state.pageNumber, state.pageSize);
            setHeader(0);
        });
    }

    function setPopupHeader(title, iconUrl) {
        $("#gvPopupTitleText").text(title || "");
        if (iconUrl) {
            $("#gvPopupHeaderIcon").attr("src", iconUrl);
        }
    }

    function setSubtitle(cardKey, cardLabel) {
        var $subtitle = $("#gvPopupSubtitle");

        $subtitle
            .removeClass(
                "gv-popup__subtitle--high gv-popup__subtitle--low gv-popup__subtitle--charge " +
                "gv-popup__subtitle--oncharge gv-popup__subtitle--ondischarge gv-popup__subtitle--onidle gv-popup__subtitle--offcharge gv-popup__subtitle--offidle " +
                "gv-popup__subtitle--temp-discharge-normal gv-popup__subtitle--temp-discharge-warning " +
                "gv-popup__subtitle--module-onidle gv-popup__subtitle--module-offcharge gv-popup__subtitle--module-offidle"
            )
            .text(cardLabel || "");

        var key = (cardKey || "").toString();

        if (state.widget === "soc") {
            if (key === "high") $subtitle.addClass("gv-popup__subtitle--high");
            else if (key === "low") $subtitle.addClass("gv-popup__subtitle--low");
            else if (key === "chargeNow") $subtitle.addClass("gv-popup__subtitle--charge");
            return;
        }

        if (state.widget === "cycle") {
            if (key === "high") $subtitle.addClass("gv-popup__subtitle--high");
            else if (key === "low") $subtitle.addClass("gv-popup__subtitle--low");
            else if (key === "eol") $subtitle.addClass("gv-popup__subtitle--charge");
            return;
        }

        if (state.widget === "rotation") {
            if (key === "good") $subtitle.addClass("gv-popup__subtitle--high");
            else if (key === "average") $subtitle.addClass("gv-popup__subtitle--low");
            else if (key === "poor") $subtitle.addClass("gv-popup__subtitle--charge");
            return;
        }

        if (state.widget === "temp") {
            if (key === "chargingNormal") $subtitle.addClass("gv-popup__subtitle--high");
            else if (key === "chargingWarning") $subtitle.addClass("gv-popup__subtitle--charge");
            else if (key === "dischargingNormal") $subtitle.addClass("gv-popup__subtitle--temp-discharge-normal");
            else if (key === "dischargingWarning") $subtitle.addClass("gv-popup__subtitle--temp-discharge-warning");
            return;
        }

        if (state.widget === "moduleStatus") {
            if (key === "onDeviceCharging") $subtitle.addClass("gv-popup__subtitle--oncharge");
            else if (key === "onDeviceDischarging") $subtitle.addClass("gv-popup__subtitle--ondischarge");
            else if (key === "onDeviceIdle") $subtitle.addClass("gv-popup__subtitle--module-onidle");
            else if (key === "offDeviceCharging") $subtitle.addClass("gv-popup__subtitle--module-offcharge");
            else if (key === "offDeviceIdle") $subtitle.addClass("gv-popup__subtitle--module-offidle");
            return;
        }
    }

    function openFor(widget, cardKey, cardLabel, title, iconUrl) {
        state.widget = widget;
        state.cardKey = cardKey;
        state.pageNumber = 1;
        state.search = "";
        $("#gvPopupSearch").val("");

        setPopupHeader(title, iconUrl);
        setSubtitle(cardKey, cardLabel);

        openPopup();
        loadData();
    }

    function bindDelegatedClicks() {
        $(document)
            .off("click.dashboardPopup", "#socWidget .soc-status__card")
            .on("click.dashboardPopup", "#socWidget .soc-status__card", function () {
                var $card = $(this);
                var key = ($card.data("card-key") || "").toString();
                if (!key) return;

                var label = $.trim($card.find(".soc-status__card-label").text());
                openFor("soc", key, label, "State of Charge", "/Content/Img/widgets/SOCHeader.svg");
            })
            .on("mouseenter.dashboardPopup", "#socWidget .soc-status__card", function () { $(this).css("cursor", "pointer"); });

        $(document)
            .off("click.dashboardPopup", "#cycleStatusWidget .cycle-status__card")
            .on("click.dashboardPopup", "#cycleStatusWidget .cycle-status__card", function () {
                var $card = $(this);
                var key = ($card.data("card-key") || "").toString();
                if (!key) return;

                var label = $.trim($card.find(".cycle-status__card-label").text());
                openFor("cycle", key, label, "Cycle Status", "/Content/Img/widgets/CCHeader.svg");
            })
            .on("mouseenter.dashboardPopup", "#cycleStatusWidget .cycle-status__card", function () { $(this).css("cursor", "pointer"); });

        $(document)
            .off("click.dashboardPopup", "#effectiveRotationWidget .effective-rotation__card")
            .on("click.dashboardPopup", "#effectiveRotationWidget .effective-rotation__card", function () {
                var $card = $(this);
                var key = ($card.data("card-key") || "").toString();
                if (!key) return;

                var label = $.trim($card.find(".effective-rotation__card-label").text());
                openFor("rotation", key, label, "Effective Rotation", "/Content/Img/widgets/CCHeader.svg");
            })
            .on("mouseenter.dashboardPopup", "#effectiveRotationWidget .effective-rotation__card", function () { $(this).css("cursor", "pointer"); });

        $(document)
            .off("click.dashboardPopup", "#temperatureWidget .temperature-widget__card")
            .on("click.dashboardPopup", "#temperatureWidget .temperature-widget__card", function () {
                var $card = $(this);
                var key = ($card.data("card-key") || "").toString();
                if (!key) return;

                var $group = $card.closest(".temperature-widget__group");
                var groupTitle = $.trim($group.find(".temperature-widget__group-title").first().text());
                var label = $.trim($card.find(".temperature-widget__card-label").text());
                var subtitle = groupTitle ? (groupTitle + " - " + label) : label;

                openFor("temp", key, subtitle, "Temperature", "/Content/Img/widgets/TempHeader.svg");
            })
            .on("mouseenter.dashboardPopup", "#temperatureWidget .temperature-widget__card", function () { $(this).css("cursor", "pointer"); });

        $(document)
            .off("click.dashboardPopup", "#batteryStatusWidget .battery-status__card")
            .on("click.dashboardPopup", "#batteryStatusWidget .battery-status__card", function () {
                var $card = $(this);
                var key = ($card.data("card-key") || "").toString();
                if (!key) return;

                var $group = $card.closest(".battery-status__group");
                var groupTitle = $.trim($group.find(".battery-status__group-title").first().text());
                var label = $.trim($card.find(".battery-status__card-label").text());
                var subtitle = groupTitle ? (groupTitle + " - " + label) : label;

                openFor("moduleStatus", key, subtitle, "Module Status", "/Content/Img/widgets/BatteryHeader.svg");
            })
            .on("mouseenter.dashboardPopup", "#batteryStatusWidget .battery-status__card", function () { $(this).css("cursor", "pointer"); });
    }

    $(document).ready(function () {
        bindDelegatedClicks();

        $("#dashboardPopupClose").on("click", closePopup);

        // Intentionally DO NOT close on overlay click (outside click)

        // Optional: if you also want to disable Escape-to-close, remove this handler.
        $(document).on("keydown.dashboardPopup", function (e) {
            if (e.key === "Escape" && $("#dashboardPopupOverlay").hasClass("is-open")) {
                closePopup();
            }
        });

        $("#gvPopupSearch").on("input", debounce(function () {
            state.search = $("#gvPopupSearch").val();
            state.pageNumber = 1;
            loadData();
        }, 300));
    });

})(window, window.jQuery);