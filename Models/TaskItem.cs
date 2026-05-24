namespace TaskHub.Models;

public class TaskItem : ITaskEntity
{
    public TaskItem()
    {
    }

    public TaskItem(
        string title,
        string description,
        TaskPriority priority,
        DateTime deadline,
        HubTaskStatus status)
    {
        Title = title;
        Description = description;
        Priority = priority;
        Deadline = deadline;
        Status = status;
    }

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public DateTime Deadline { get; set; }
    public HubTaskStatus Status { get; set; }

    public bool IsOverdue => Status != HubTaskStatus.Done && Deadline < DateTime.Now;
}
