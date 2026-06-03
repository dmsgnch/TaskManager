using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskManager.Converters;

public sealed class BooleanToVisibilityConverter : IValueConverter
{
    private const string InvertParameter = "invert";

    public Visibility TrueVisibility { get; set; } = Visibility.Visible;

    public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var flag = value is true;

        if (parameter is string stringParameter &&
            string.Equals(stringParameter, InvertParameter, StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag
            ? TrueVisibility
            : FalseVisibility;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        var flag = value is Visibility.Visible;

        if (parameter is string stringParameter &&
            string.Equals(stringParameter, InvertParameter, StringComparison.OrdinalIgnoreCase))
        {
            flag = !flag;
        }

        return flag;
    }
}