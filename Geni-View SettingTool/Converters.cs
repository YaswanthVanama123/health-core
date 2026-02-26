using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Geni_View_SettingTool
{
    // Text converter: bool → "MQTT:Online" / "MQTT:Offline"
    public class MQTTStatusConverter : IValueConverter
    {
        public static readonly MQTTStatusConverter Instance = new MQTTStatusConverter();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? "MQTT:Online" : "MQTT:Offline";

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

    // Background converter: bool → YellowGreen / Red
    public class MQTTBgConverter : IValueConverter
    {
        public static readonly MQTTBgConverter Instance = new MQTTBgConverter();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true
                ? (IBrush)new SolidColorBrush(Colors.YellowGreen)
                : (IBrush)new SolidColorBrush(Colors.Red);

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
