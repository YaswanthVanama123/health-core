function CreateNewBatteryMasterChart(JData, drawMasterChartByIndex) {
    var index = 1;
    // Toggle x Axis By Date and LogIndex Properties
    var drawMasterChartByIndex = $("input[id=xAxisbyDate]:checked").val() ? false : true;
    var xAxisValue;

    // Structure of MasterBattery
    var joinedTable = new google.visualization.DataTable();
    if (drawMasterChartByIndex)
        joinedTable.addColumn('number', 'LogIndex');
    else
        joinedTable.addColumn('datetime', 'LogDate');

    joinedTable.addColumn('number', 'Voltage');
    joinedTable.addColumn('number', 'Current');
    joinedTable.addColumn('number', 'Temperature');
    joinedTable.addColumn('number', 'RemainCapacity');
    joinedTable.addColumn('number', 'Power');
    joinedTable.addColumn('number', 'RelativeStateofCharge');
    joinedTable.addColumn('number', 'CalculatedCapacity');
    joinedTable.addColumn('number', 'CycleCount');

    for (var i = 0; i < JData.length; i++) {
        index = i + 1;
        var dataTable = new google.visualization.DataTable();

        if (drawMasterChartByIndex)
            dataTable.addColumn('number', 'LogIndex');
        else
            dataTable.addColumn('datetime', 'LogDate');

        dataTable.addColumn('number', 'Bat' + index + ' - Voltage');
        dataTable.addColumn('number', 'Bat' + index + ' - Current');
        dataTable.addColumn('number', 'Bat' + index + ' - Temperature');
        dataTable.addColumn('number', 'Bat' + index + ' - Capacity');
        dataTable.addColumn('number', 'Bat' + index + ' - Power');
        dataTable.addColumn('number', 'Bat' + index + ' - State of Charge');
        dataTable.addColumn('number', 'Bat' + index + ' - Calculated Capacity');
        dataTable.addColumn('number', 'Bat' + index + ' - Cycle Count');

        for (var j = 0; j < JData[i].length; j++) {
            if (drawMasterChartByIndex)
                xAxisValue = JData[i][j].LogIndex;
            else
                xAxisValue = new Date(JData[i][j].LogDate);

            dataTable.addRow([
                xAxisValue,
                JData[i][j].Voltage,
                JData[i][j].Current,
                JData[i][j].Temperature,
                JData[i][j].RemainingCapacity,
                JData[i][j].Power,
                JData[i][j].RelativeStateofCharge,
                JData[i][j].CalculatedCapacity,
                JData[i][j].CycleCount,
            ]);
        }

        if (JData[i].length > 0) {
            joinedTable = google.visualization.data.join(
                dataTable,
                joinedTable,
                'full',
                [[0, 0]],
                [1, 2, 3, 4, 5, 6, 7, 8],
                [1, 2, 3, 4, 5, 6, 7, 8]
            );
        }
    }

    var container = document.getElementById('MasterChartdiv');
    var containerWidth = (container && container.clientWidth) ? container.clientWidth : 1000;
    var rowCount = joinedTable.getNumberOfRows();

    // Use Google auto ticks for master chart; just adapt font + slant for visibility.
    var desiredLabelCount = Math.max(2, Math.floor(containerWidth / 110));
    var skip = Math.max(1, Math.ceil(rowCount / desiredLabelCount));

    var hAxisFontSize = 10;
    if (skip > 35) hAxisFontSize = 7;
    else if (skip > 22) hAxisFontSize = 8;
    else if (skip > 14) hAxisFontSize = 9;

    var useSlanted = (!drawMasterChartByIndex) && (skip > 10);

    var options = {
        'title': null,
        'width': '100%',
        'height': 600,
        'interpolateNulls': true,
        'chartArea': { left: '10%', width: '70%', bottom: useSlanted ? 90 : 70 },
        focusTarget: 'category',
        explorer: {
            axis: 'horizontal',
            maxZoomIn: 20
        },
        vAxis: {
            gridlines: { count: 10 }
        },
        hAxis: drawMasterChartByIndex ? {
            gridlines: { count: -1 }
        } : {
            format: 'dd/MM/yyyy',
            slantedText: true,
            slantedTextAngle: 45,
            textStyle: { fontSize: hAxisFontSize },
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
        google.visualization.errors.removeAll(document.getElementById('MasterChartdiv'));
        $('#series').find(':checkbox').change(function () {
            var cols = [0];
            $('#series').find(':checkbox:checked').each(function () {
                var value = parseInt($(this).attr('value'));
                for (var i = 0; i < JData.length; i++) {
                    if (JData[i].length > 0) {
                        cols.push(8 * i + value);
                    }
                }
            });
            view.setColumns(cols);
            chart.draw(view, options);
        });
    } else {
        google.visualization.errors.removeAll(document.getElementById('MasterChartdiv'));
        google.visualization.errors.addError(document.getElementById('MasterChartdiv'), 'Please, Check filter options and try again');
    }
}