using System.Globalization;
using TaskManager.Resources.Core;
using CommonResources = TaskManager.Resources.CommonResources.CommonResources;
using TaskManager.ViewModels.Abstracts;

namespace TaskManager.Services;

public class LocalizationService : ILocalizationService
{
    private const string EnglishCultureName = "en-US";
    private const string UkrainianCultureName = "uk-UA";

    private static readonly CultureInfo EnglishCulture = CultureInfo.GetCultureInfo(EnglishCultureName);
    private static readonly CultureInfo UkrainianCulture = CultureInfo.GetCultureInfo(UkrainianCultureName);

    private readonly LocalizationManager _localizationManager;

    public LocalizationService(LocalizationManager localizationManager)
    {
        _localizationManager = localizationManager;
    }

    public bool IsUkrainianCulture =>
        string.Equals(
            CultureInfo.CurrentUICulture.Name,
            UkrainianCultureName,
            StringComparison.OrdinalIgnoreCase);

    public string GetString(string key)
    {
        return CommonResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public string FormatString(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentUICulture, GetString(key), args);
    }

    public void SetUkrainianCulture(bool isUkrainianCulture)
    {
        var culture = isUkrainianCulture
            ? UkrainianCulture
            : EnglishCulture;

        CommonResources.Culture = culture;
        _localizationManager.UpdateCurrentCulture(culture);
    }
}
