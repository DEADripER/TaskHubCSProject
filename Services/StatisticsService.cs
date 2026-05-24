using TaskHub.Models;

namespace TaskHub.Services;

public static class StatisticsService
{
    public static Task<StatisticsResult> BuildAsync(IEnumerable<TaskItem> tasks)
    {
        return Task.Run(() => Build(tasks));
    }

    public static StatisticsResult Build(IEnumerable<TaskItem> tasks)
    {
        var taskList = tasks.ToList();
        var byPriority = Enum.GetValues<TaskPriority>()
            .ToDictionary(priority => priority, priority => taskList.Count(task => task.Priority == priority));

        return new StatisticsResult
        {
            Total = taskList.Count,
            Done = taskList.Count(task => task.Status == HubTaskStatus.Done),
            Overdue = taskList.Count(task => task.IsOverdue),
            ByPriority = byPriority
        };
    }
}
