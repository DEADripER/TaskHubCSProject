using TaskHub.Models;

namespace TaskHub.Utils;

public static class ConsolePrinter
{
    public static void PrintMenu()
    {
        PrintHeader("TaskHub");
        Console.WriteLine("1. Создать задачу");
        Console.WriteLine("2. Просмотреть задачи");
        Console.WriteLine("3. Редактировать задачу");
        Console.WriteLine("4. Удалить задачу");
        Console.WriteLine("5. Отметить задачу выполненной");
        Console.WriteLine("6. Поиск задач");
        Console.WriteLine("7. Статистика");
        Console.WriteLine("8. Сохранить задачи в файл");
        Console.WriteLine("9. Загрузить задачи из файла");
        Console.WriteLine("0. Выход");
        Console.WriteLine();
    }

    public static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(title);
    }

    public static void PrintTasks(IEnumerable<TaskItem> tasks)
    {
        var taskList = tasks.ToList();

        if (taskList.Count == 0)
        {
            WriteWarning("Задач нет.");
            return;
        }

        foreach (var task in taskList)
        {
            Console.WriteLine($"Номер: {task.Id}");
            Console.WriteLine($"Название: {task.Title}");
            Console.WriteLine($"Описание: {task.Description}");
            Console.WriteLine($"Приоритет: {task.Priority}");
            Console.WriteLine($"Дедлайн: {task.Deadline:g}");
            Console.WriteLine($"Статус: {task.Status}");
            Console.WriteLine($"Просрочена: {(task.IsOverdue ? "Да" : "Нет")}");
            Console.WriteLine();
        }
    }

    public static void WriteSuccess(string message)
    {
        Console.WriteLine(message);
    }

    public static void WriteWarning(string message)
    {
        Console.WriteLine(message);
    }

    public static void WriteError(string message)
    {
        Console.WriteLine(message);
    }

    public static void WriteNotification(string message)
    {
        Console.WriteLine($"Уведомление: {message}");
    }
}
