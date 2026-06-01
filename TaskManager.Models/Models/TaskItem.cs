namespace TaskManager.Models.Models;

public class TaskItem
{
    public int Id { get; init; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}