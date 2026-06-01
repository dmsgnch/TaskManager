using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using AudioVideoRecorder.Resources.CommonResources;

namespace TaskManager.Resources.Core;

public class LocalizationManager : INotifyPropertyChanged
{
    public CommonResources CommonResources
    {
        get;
        private set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateCurrentCulture(CultureInfo culture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        CommonResources = new CommonResources();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}