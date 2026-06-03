using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TaskManager.Converters;

public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public Visibility NullVisibility { get; set; } = Visibility.Collapsed;

    public Visibility NotNullVisibility { get; set; } = Visibility.Visible;

    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is not null
            ? NotNullVisibility
            : NullVisibility;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}