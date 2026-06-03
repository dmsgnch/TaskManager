using TaskManager.ViewModels.Abstracts;

namespace TaskManager.ViewModels.Dialogs;

public class TaskEditDialogViewModelFactory : ITaskEditDialogViewModelFactory
{
    private readonly ILocalizationService _localizationService;

    public TaskEditDialogViewModelFactory(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public ITaskEditDialogViewModel Create()
    {
        return new TaskEditDialogViewModel(_localizationService);
    }
}
