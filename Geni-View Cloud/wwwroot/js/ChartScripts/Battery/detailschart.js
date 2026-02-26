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
            gridlines: {
                count: -1,
                color: 'none',
                units: {
                    days: { format: ['MMM dd'] },
                    hours: { format: ['HH:mm', 'ha'] },
                }
            },
            minorGridlines: {
                units: {
                    hours: { format: ['hh:mm:ss a', 'ha'] },
                    minutes: { format: ['HH:mm a Z', ':mm'] },
                }
            }
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