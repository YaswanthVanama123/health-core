// Draw Chart
// 1 - Json Result
// 2 - Title
// 3 - Legend Name
// 4 - Div Name (id defined before)
// 5 - data column
// 6 - Chart Type
// 7 - colorArray(or single Color)
// 8 - min Value
// 9 - Max Value
function CreateNewBatteryDetailChartbyDate(JData, title, legendName, chartDiv, colName, chartType, colorsArray, minValue, maxValue) {
    var dataTable = new google.visualization.DataTable();
    // Add Data Columns
    dataTable.addColumn('datetime', 'LogDate');
    dataTable.addColumn('number', legendName);

    for (var i = 0; i < JData.length; i++) {
        dataTable.addRow([new Date(JData[i].LogDate), JData[i][colName]]);
    }

    var container = document.getElementById(chartDiv);
    var containerWidth = (container && container.clientWidth) ? container.clientWidth : 600;
    var rowCount = dataTable.getNumberOfRows();

    var desiredLabelCount = Math.max(2, Math.floor(containerWidth / 90));
    var skip = Math.max(1, Math.ceil(rowCount / desiredLabelCount));

    var hAxisFontSize = 10;
    if (skip > 20) hAxisFontSize = 7;
    else if (skip > 12) hAxisFontSize = 8;
    else if (skip > 8) hAxisFontSize = 9;

    var dateTicks = [];
    for (var t = 0; t < rowCount; t += skip) {
        dateTicks.push(dataTable.getValue(t, 0));
    }
    if (rowCount > 0 && dateTicks.length && dateTicks[dateTicks.length - 1] !== dataTable.getValue(rowCount - 1, 0)) {
        dateTicks.push(dataTable.getValue(rowCount - 1, 0));
    }

    var options = {
        'title': title,
        'width': '50%',
        'height': 300,
        'chartArea': { left: '15%', width: '75%' },
        focusTarget: 'category',
        colors: [colorsArray],
        explorer: {
            axis: 'horizontal'
        },
        hAxis: {
            format: 'dd/MM/yyyy',
            slantedText: false,
            textStyle: { fontSize: hAxisFontSize },
            ticks: dateTicks,
            gridlines: {
                count: -1,
                color: 'none'
            },
            minorGridlines: {
                color: 'none'
            }
        },
        vAxis: {
            minValue: minValue,
            maxValue: maxValue,
        },

        legend: { position: 'top' },
        seriesType: chartType

    };

    var chart = new google.visualization.ComboChart(container);
    chart.draw(dataTable, options);

    function resizeChart() {
        var w = (container && container.clientWidth) ? container.clientWidth : containerWidth;
        var desired = Math.max(2, Math.floor(w / 90));
        var newSkip = Math.max(1, Math.ceil(rowCount / desired));

        var newFont = 10;
        if (newSkip > 20) newFont = 7;
        else if (newSkip > 12) newFont = 8;
        else if (newSkip > 8) newFont = 9;

        var newTicks = [];
        for (var nt = 0; nt < rowCount; nt += newSkip) {
            newTicks.push(dataTable.getValue(nt, 0));
        }
        if (rowCount > 0 && newTicks.length && newTicks[newTicks.length - 1] !== dataTable.getValue(rowCount - 1, 0)) {
            newTicks.push(dataTable.getValue(rowCount - 1, 0));
        }

        options.hAxis.textStyle.fontSize = newFont;
        options.hAxis.ticks = newTicks;
        chart.draw(dataTable, options);
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
}

// By Index
function CreateNewBatteryDetailChartbyIndex(JData, title, legendName, chartDiv, colName, chartType, colorsArray, minValue, maxValue) {
    var dataTable = new google.visualization.DataTable();
    // Add Data Columns
    dataTable.addColumn('number', 'LogIndex');
    dataTable.addColumn('number', legendName);

    for (var i = 0; i < JData.length; i++) {
        dataTable.addRow([JData[i].LogIndex, JData[i][colName]]);
    }

    var options = {
        'title': title,
        'width': '50%',
        'height': 300,
        'chartArea': { left: '15%', width: '75%' },
        focusTarget: 'category',
        colors: [colorsArray],
        explorer: {
            axis: 'horizontal'
        },
        hAxis: {
            gridlines: { color: 'none' }
        },
        vAxis: {
            minValue: minValue,
            maxValue: maxValue,
        },
        legend: { position: 'top' },
        seriesType: chartType

    };

    var chart = new google.visualization.ComboChart(document.getElementById(chartDiv));
    chart.draw(dataTable, options);
    function resizeChart() {
        chart.draw(dataTable, options);
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

}

function clearChartContent() {
    var charts = document.getElementsByClassName("charts");

    for (i = 0; i < charts.length; i++) {
        charts[i].innerHTML = "";
    }
}