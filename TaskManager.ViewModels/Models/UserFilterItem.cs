using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManager.ViewModels.Models;

public partial class UserFilterItem : ObservableObject
{
    private string _displayName;

    public int UserId { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public UserFilterItem(int userId, string displayName)
    {
        UserId = userId;
        _displayName = displayName;
    }

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = displayName;
    }
}
