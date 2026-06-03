using CommunityToolkit.Mvvm.ComponentModel;

namespace TaskManager.Models.Models;

public partial class TaskItem : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; private set; }
    
    [ObservableProperty]
    public partial int UserId { get; set; }
    
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    
    [ObservableProperty]
    public partial bool IsCompleted { get; set; }

    public TaskItem(int id)
    {
        Id = id;
    }

    public void UpdateFrom(TaskItem taskItem)
    {
        UserId =  taskItem.UserId;
        Title = taskItem.Title;
        IsCompleted = taskItem.IsCompleted;
    }

    public TaskEditParams ToTaskEditParams()
    {
        return new TaskEditParams(Title, UserId, IsCompleted);
    }
}