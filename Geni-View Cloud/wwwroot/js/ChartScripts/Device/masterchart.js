// Draw Device Master Chart
function CreateNewDeviceMasterChart(JData, bayNo, deviceData) {
    var index = 1;
    // Draw Chart by Index
    var drawMasterChartByIndex = $("input[id=xAxisbyDate]:checked").val() ? false : true;
    var xAxisValue;
    var chartTitle = drawMasterChartByIndex ? "Device Master Chart by Index" : "Device Master Chart by Date";
    // Note: When joining we need full column id list. This list depends on baycount
    var joinedTableColCount = [1, 2, 3, 4, 5, 6];

    // Structure of MasterDevice
    var joinedTable = new google.visualization.DataTable();

    if (drawMasterChartByIndex)
        joinedTable.addColumn('number', 'LogIndex');
    else
        joinedTable.addColumn('datetime', 'LogDate');

    joinedTable.addColumn('number', 'Voltage');
    joinedTable.addColumn('number', 'Current');
    joinedTable.addColumn('number', 'Temperature');
    joinedTable.addColumn('number', 'Capacity');
    joinedTable.addColumn('number', 'Power');
    joinedTable.addColumn('number', 'State of Charge');

    for (var i = 0; i < JData.length; i++) {
        if (bayNo > 0)
            index = i + bayNo
        else
            index = i + 1;
        var dataTable = new google.visualization.DataTable();
        // Add Data Columns
        if (drawMasterChartByIndex)
            dataTable.addColumn('number', 'LogIndex');
        else
            dataTable.addColumn('datetime', 'LogDate');

        dataTable.addColumn('number', 'BAY' + index + ' - Voltage');
        dataTable.addColumn('number', 'BAY' + index + ' - Current');
        dataTable.addColumn('number', 'BAY' + index + ' - Temperature');
        dataTable.addColumn('number', 'BAY' + index + ' - Capacity');
        dataTable.addColumn('number', 'BAY' + index + ' - Power');
        dataTable.addColumn('number', 'BAY' + index + ' - State of Charge');

        for (var j = 0; j < JData[i].length; j++) {

            if (drawMasterChartByIndex)
                xAxisValue = JData[i][j].LogIndex
            else
                xAxisValue = new Date(JData[i][j].LogDate)

            dataTable.addRow([
                xAxisValue,
                JData[i][j].Voltage,
                JData[i][j].Current,
                JData[i][j].Temperature,
                JData[i][j].RemainingCapacity,
                JData[i][j].Power,
                JData[i][j].RelativeStateofCharge,
            ]);
        }

        if (JData[i].length > 0) {
            if (i == 0) {
                joinedTable = dataTable;
            } else {
                joinedTable = google.visualization.data.join(joinedTable, dataTable, 'full', [[0, 0]], joinedTableColCount, [1, 2, 3, 4, 5, 6]);
                for (var x = 0; x < joinedTable.getNumberOfColumns() - 1; x++)
                    joinedTableColCount[x] = x + 1;
            }
        }
    }

    if (joinedTable.getNumberOfRows() > 0) {
        var temperatureDataTable = new google.visualization.DataTable();
        if (drawMasterChartByIndex)
            temperatureDataTable.addColumn('number', 'LogIndex');
        else
            temperatureDataTable.addColumn('datetime', 'LogDate');

        temperatureDataTable.addColumn('number', 'Device Temperature');
        temperatureDataTable.addColumn('number', 'Device Output Power');
        for (var i = 0; i < deviceData.length; i++) {
            if (drawMasterChartByIndex)
                xAxisValue = deviceData[i].LogIndex
            else
                xAxisValue = new Date(deviceData[i].LogDate)

            temperatureDataTable.addRow([
                xAxisValue,
                deviceData[i].PowerTemperatureOutput,
                deviceData[i].PowerOutput
            ]);
        }

        joinedTable = google.visualization.data.join(joinedTable, temperatureDataTable, 'full', [[0, 0]], joinedTableColCount, [1, 2]);
    }

    var container = document.getElementById('DeviceDiv');
    var containerWidth = (container && container.clientWidth) ? container.clientWidth : 1000;
    var rowCount = joinedTable.getNumberOfRows();

    // Keep tick generation conservative for master chart; let Google auto-pick most labels.
    // Use ticks only for very dense datasets.
    var desiredLabelCount = Math.max(2, Math.floor(containerWidth / 110));
    var skip = Math.max(1, Math.ceil(rowCount / desiredLabelCount));

    var hAxisFontSize = 10;
    if (skip > 35) hAxisFontSize = 7;
    else if (skip > 22) hAxisFontSize = 8;
    else if (skip > 14) hAxisFontSize = 9;

    var dateTicks = null;
    var useAutoTicks = true;
    if (!drawMasterChartByIndex && rowCount > 0) {
        // Use explicit ticks only when data is very dense.
        if (rowCount > desiredLabelCount * 2) {
            useAutoTicks = false;
            dateTicks = [];
            for (var t = 0; t < rowCount; t += skip) {
                dateTicks.push(joinedTable.getValue(t, 0));
            }
            var last = joinedTable.getValue(rowCount - 1, 0);
            if (dateTicks.length && dateTicks[dateTicks.length - 1] !== last) {
                dateTicks.push(last);
            }
        }
    }

    // If horizontal labels don't fit, angle them slightly to ensure visibility.
    var useSlanted = (!drawMasterChartByIndex) && (skip > 10);

    var options = {
        'title': null,
        'width': '100%',
        'height': 600,
        'interpolateNulls': true,
        // Reserve space for angled/horizontal labels
        'chartArea': { left: '10%', width: '70%', bottom: useSlanted ? 90 : 70 },
        focusTarget: 'category',
        explorer: {
            axis: 'horizontal',
            maxZoomIn: 30 /*Max zoom In*/
        },
        vAxis: {
            gridlines: { count: 10 },
            textStyle: { fontSize: 10 }
        },
        hAxis: drawMasterChartByIndex ? {
            gridlines: { count: -1 },
            textStyle: { fontSize: 10 }
        } : {
            format: 'dd/MM/yyyy',
            slantedText: true,
            slantedTextAngle: 45,
            textStyle: { fontSize: hAxisFontSize },
            // Keep Google's auto tick selection so labels appear at appropriate points
            ticks: undefined,
            gridlines: { count: -1, color: 'none' },
            minorGridlines: { color: 'none' }
        },
        legend: { position: 'right', textStyle: { fontSize: 12 } }
    };

    var view = new google.visualization.DataView(joinedTable);
    var chart = new google.visualization.LineChart(container);
    chart.draw(view, options);

    function resizeChart() {
        chart.draw(view, options);
    }
    if (document.addEventListener) {
        window.addEventListener('resize', resizeChart);
    }
    else if (document.attachEvent) {
        window.attachEvent('onresize', resizeChart);
    }
    else {
        window.resize = resizeChart;
    }

    if (view.getNumberOfRows() > 0) {
        var loopCount;
        google.visualization.errors.removeAll(document.getElementById('DeviceDiv'));
        $('#series').find(':checkbox').change(function () {
            var cols = [0];
            $('#series').find(':checkbox:checked').each(function () {
                var value = parseInt($(this).attr('value'));
                for (var i = 0; i < JData.length; i++) {
                    if (JData[i].length > 0) {
                        if (value != 7 && value != 8) {
                            cols.push(6 * i + value);
                        }
                        loopCount = i + 1;
                    }
                }
                if (value == 7 || value == 8)
                    cols.push(value + 6 * (loopCount - 1));
            });

            view.setColumns(cols);
            chart.draw(view, options);
        });
    } else {
        google.visualization.errors.removeAll(document.getElementById('DeviceDiv'));
        google.visualization.errors.addError(document.getElementById('DeviceDiv'), 'No Data Found.');
    }
}