using TaskHub.Core;
using TaskHub.Models;
using TaskHub.Services;
using TaskHub.Storage;
using TaskHub.Utils;

namespace TaskHub;

public static class Program
{
    private const string DataFilePath = "tasks.json";

    public static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var repository = new TaskRepository<TaskItem>();

        using var storage = new JsonTaskStorage(DataFilePath);
        using var deadlineMonitor = new DeadlineMonitor(repository, TimeSpan.FromSeconds(5));

        deadlineMonitor.OverdueTaskFound += OnOverdueTaskFound;
        deadlineMonitor.Start();

        await TryLoadAtStartupAsync(repository, storage);

        var isRunning = true;
        while (isRunning)
        {
            ConsolePrinter.PrintMenu();
            var choice = InputHelper.ReadRequiredString("Выберите пункт меню: ");

            try
            {
                switch (choice)
                {
                    case "1":
                        CreateTask(repository);
                        break;
                    case "2":
                        ShowTasks(repository);
                        break;
                    case "3":
                        EditTask(repository);
                        break;
                    case "4":
                        DeleteTask(repository);
                        break;
                    case "5":
                        MarkTaskAsDone(repository);
                        break;
                    case "6":
                        SearchTasks(repository);
                        break;
                    case "7":
                        await ShowStatisticsAsync(repository);
                        break;
                    case "8":
                        await SaveTasksAsync(repository, storage);
                        break;
                    case "9":
                        await LoadTasksAsync(repository, storage);
                        break;
                    case "0":
                        isRunning = false;
                        break;
                    default:
                        ConsolePrinter.WriteWarning("Такого пункта меню нет.");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsolePrinter.WriteError($"Ошибка: {ex.Message}");
            }

            if (isRunning)
            {
                InputHelper.Pause();
            }
        }

        await SaveTasksAsync(repository, storage);
        ConsolePrinter.WriteSuccess("Работа завершена. Задачи сохранены.");
    }

    private static void CreateTask(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Создание задачи");

        var title = InputHelper.ReadRequiredString("Название: ");
        var description = InputHelper.ReadRequiredString("Описание: ");
        var priority = InputHelper.ReadEnum<TaskPriority>("Приоритет (Low / Medium / High): ");
        var deadline = InputHelper.ReadDateTime("Дедлайн (например 2026-06-01 18:30): ");
        var status = InputHelper.ReadEnum<HubTaskStatus>("Статус (New / InProgress / Done): ");

        var task = new TaskItem(title, description, priority, deadline, status);
        repository.Add(task);

        ConsolePrinter.WriteSuccess("Задача создана.");
    }

    private static void ShowTasks(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Просмотр задач");
        Console.WriteLine("1. Все задачи");
        Console.WriteLine("2. Выполненные");
        Console.WriteLine("3. Невыполненные");
        Console.WriteLine("4. Высокий приоритет");

        var choice = InputHelper.ReadRequiredString("Выберите фильтр: ");
        var tasks = choice switch
        {
            "1" => repository.GetAll(),
            "2" => repository.Find(task => task.Status == HubTaskStatus.Done),
            "3" => repository.Find(task => task.Status != HubTaskStatus.Done),
            "4" => repository.Find(task => task.Priority == TaskPriority.High),
            _ => throw new ArgumentException("Неизвестный фильтр.")
        };

        ConsolePrinter.PrintTasks(tasks);
    }

    private static void EditTask(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Редактирование задачи");
        ConsolePrinter.PrintTasks(repository.GetAll());

        var id = InputHelper.ReadInt("Введите номер задачи: ");
        var task = repository.GetById(id) ?? throw new InvalidOperationException("Задача не найдена.");

        var title = InputHelper.ReadOptionalString($"Название ({task.Title}): ");
        var description = InputHelper.ReadOptionalString($"Описание ({task.Description}): ");
        var priority = InputHelper.ReadOptionalEnum<TaskPriority>($"Приоритет ({task.Priority}): ");
        var status = InputHelper.ReadOptionalEnum<HubTaskStatus>($"Статус ({task.Status}): ");

        if (!string.IsNullOrWhiteSpace(title))
        {
            task.Title = title;
        }

        if (!string.IsNullOrWhiteSpace(description))
        {
            task.Description = description;
        }

        if (priority.HasValue)
        {
            task.Priority = priority.Value;
        }

        if (status.HasValue)
        {
            task.Status = status.Value;
        }

        repository.Update(task);
        ConsolePrinter.WriteSuccess("Задача обновлена.");
    }

    private static void DeleteTask(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Удаление задачи");
        ConsolePrinter.PrintTasks(repository.GetAll());

        var id = InputHelper.ReadInt("Введите номер задачи: ");
        var isDeleted = repository.Remove(id);

        if (isDeleted)
        {
            ConsolePrinter.WriteSuccess("Задача удалена.");
        }
        else
        {
            ConsolePrinter.WriteWarning("Задача не найдена.");
        }
    }

    private static void MarkTaskAsDone(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Выполнение задачи");
        ConsolePrinter.PrintTasks(repository.Find(task => task.Status != HubTaskStatus.Done));

        var id = InputHelper.ReadInt("Введите номер задачи: ");
        var task = repository.GetById(id) ?? throw new InvalidOperationException("Задача не найдена.");

        task.Status = HubTaskStatus.Done;
        repository.Update(task);

        ConsolePrinter.WriteSuccess("Задача отмечена как выполненная.");
    }

    private static void SearchTasks(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Поиск задач");
        Console.WriteLine("1. По названию");
        Console.WriteLine("2. По статусу");
        Console.WriteLine("3. По приоритету");

        var choice = InputHelper.ReadRequiredString("Выберите тип поиска: ");

        IReadOnlyList<TaskItem> tasks = choice switch
        {
            "1" => SearchByTitle(repository),
            "2" => SearchByStatus(repository),
            "3" => SearchByPriority(repository),
            _ => throw new ArgumentException("Неизвестный тип поиска.")
        };

        ConsolePrinter.PrintTasks(tasks);
    }

    private static async Task ShowStatisticsAsync(TaskRepository<TaskItem> repository)
    {
        ConsolePrinter.PrintHeader("Статистика");

        var statistics = await StatisticsService.BuildAsync(repository.GetAll());

        Console.WriteLine($"Всего задач: {statistics.Total}");
        Console.WriteLine($"Выполнено: {statistics.Done}");
        Console.WriteLine($"Просрочено: {statistics.Overdue}");
        Console.WriteLine("По приоритетам:");

        foreach (var pair in statistics.ByPriority)
        {
            Console.WriteLine($"- {pair.Key}: {pair.Value}");
        }
    }

    private static IReadOnlyList<TaskItem> SearchByTitle(TaskRepository<TaskItem> repository)
    {
        var titlePart = InputHelper.ReadRequiredString("Введите часть названия: ");
        return repository.Find(task => task.Title.Contains(titlePart, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<TaskItem> SearchByStatus(TaskRepository<TaskItem> repository)
    {
        var status = InputHelper.ReadEnum<HubTaskStatus>("Статус (New / InProgress / Done): ");
        return repository.Find(task => task.Status == status);
    }

    private static IReadOnlyList<TaskItem> SearchByPriority(TaskRepository<TaskItem> repository)
    {
        var priority = InputHelper.ReadEnum<TaskPriority>("Приоритет (Low / Medium / High): ");
        return repository.Find(task => task.Priority == priority);
    }

    private static async Task SaveTasksAsync(TaskRepository<TaskItem> repository, JsonTaskStorage storage)
    {
        await storage.SaveAsync(repository.GetAll());
        ConsolePrinter.WriteSuccess("Задачи сохранены в файл.");
    }

    private static async Task LoadTasksAsync(TaskRepository<TaskItem> repository, JsonTaskStorage storage)
    {
        var tasks = await storage.LoadAsync();
        repository.ReplaceAll(tasks);
        ConsolePrinter.WriteSuccess("Задачи загружены из файла.");
    }

    private static async Task TryLoadAtStartupAsync(TaskRepository<TaskItem> repository, JsonTaskStorage storage)
    {
        try
        {
            if (File.Exists(DataFilePath))
            {
                await LoadTasksAsync(repository, storage);
            }
        }
        catch (Exception ex)
        {
            ConsolePrinter.WriteWarning($"Не удалось загрузить файл при запуске: {ex.Message}");
        }
    }

    private static void OnOverdueTaskFound(TaskItem task)
    {
        ConsolePrinter.WriteNotification($"Просрочена задача: {task.Title} | дедлайн: {task.Deadline:g}");
    }
}
