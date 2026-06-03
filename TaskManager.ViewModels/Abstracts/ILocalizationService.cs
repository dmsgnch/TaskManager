namespace TaskManager.ViewModels.Abstracts;

public interface ILocalizationService
{
    bool IsUkrainianCulture { get; }

    string GetString(string key);

    string FormatString(string key, params object[] args);

    void SetUkrainianCulture(bool isUkrainianCulture);
}
