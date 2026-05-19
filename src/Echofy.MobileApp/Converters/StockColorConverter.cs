using System.Globalization;

namespace Echofy.MobileApp.Converters;

public class StockColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int qty && qty > 0)
            return Colors.DarkGreen;
        return Colors.Red;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
