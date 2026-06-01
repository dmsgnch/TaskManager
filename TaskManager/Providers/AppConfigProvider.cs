using System.Reflection;
using TaskManager.Core.Providers.Abstracts;

namespace TaskManager.Providers;

public class AppConfigProvider : IAppConfigProvider
{
    public string SolutionName => Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
}