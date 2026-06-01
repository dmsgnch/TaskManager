namespace TaskManager.Models.Models;

public readonly record struct TaskEditParams(string Title, int UserId, bool IsCompleted);
