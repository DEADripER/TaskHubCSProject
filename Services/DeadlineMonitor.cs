using TaskHub.Core;
using TaskHub.Models;
using TaskHub.Utils;

namespace TaskHub.Services;

public sealed class DeadlineMonitor : IDisposable
{
    private readonly TaskRepository<TaskItem> _repository;
    private readonly TimeSpan _interval;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly HashSet<int> _alreadyNotified = new();
    private Task? _monitoringTask;
    private bool _isDisposed;

    public DeadlineMonitor(TaskRepository<TaskItem> repository, TimeSpan interval)
    {
        _repository = repository;
        _interval = interval;
    }

    public event TaskNotification<TaskItem>? OverdueTaskFound;

    public void Start()
    {
        ThrowIfDisposed();
        _monitoringTask ??= Task.Run(MonitorAsync);
    }

    private async Task MonitorAsync()
    {
        while (!_cancellation.IsCancellationRequested)
        {
            try
            {
                var overdueTasks = _repository.Find(task => task.IsOverdue);

                foreach (var task in overdueTasks)
                {
                    if (_alreadyNotified.Add(task.Id))
                    {
                        OverdueTaskFound?.Invoke(task);
                    }
                }

                _alreadyNotified.RemoveWhere(id => overdueTasks.All(task => task.Id != id));

                await Task.Delay(_interval, _cancellation.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ConsolePrinter.WriteWarning($"Фоновая проверка дедлайнов: {ex.Message}");
            }
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(DeadlineMonitor));
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _cancellation.Cancel();

        try
        {
            _monitoringTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
        }

        _cancellation.Dispose();
        _isDisposed = true;
    }
}
