using TaskManager.ViewModels.Enums;

namespace TaskManager.ViewModels.Models;

public sealed record TaskFilterModeItem(
    TaskFilterMode Value,
    string DisplayName)
{
    public override string ToString()
    {
        return DisplayName;
    }
}
