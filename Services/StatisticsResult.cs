using TaskHub.Models;

namespace TaskHub.Services;

public class StatisticsResult
{
    public int Total { get; init; }
    public int Done { get; init; }
    public int Overdue { get; init; }
    public Dictionary<TaskPriority, int> ByPriority { get; init; } = new();
}
