using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace TaskManager.Resources.Core;

public class LocalizationManager : INotifyPropertyChanged
{
    private const string IndexerPropertyName = "Item[]";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key] =>
        CommonResources.CommonResources.ResourceManager.GetString(
            key,
            CultureInfo.CurrentUICulture) ?? key;

    public void UpdateCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CommonResources.CommonResources.Culture = culture;

        OnPropertyChanged(IndexerPropertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
