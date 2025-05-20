using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace task3.Views;

public class BoolToPlayPauseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRunning)
        {
            return isRunning ? "Пауза" : "Старт";
        }
        return "Старт/Пауза";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}