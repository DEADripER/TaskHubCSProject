using System.Globalization;

namespace TaskHub.Utils;

public static class InputHelper
{
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            ConsolePrinter.WriteWarning("Значение не может быть пустым.");
        }
    }

    public static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public static TEnum ReadEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
                && Enum.IsDefined(result))
            {
                return result;
            }

            ConsolePrinter.WriteWarning($"Введите одно из значений: {string.Join(", ", Enum.GetNames<TEnum>())}");
        }
    }

    public static TEnum? ReadOptionalEnum<TEnum>(string prompt) where TEnum : struct, Enum
    {
        Console.Write(prompt);
        var value = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            && Enum.IsDefined(result))
        {
            return result;
        }

        ConsolePrinter.WriteWarning("Значение не изменено: неверный формат.");
        return null;
    }

    public static DateTime ReadDateTime(string prompt)
    {
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "dd.MM.yyyy HH:mm",
            "dd.MM.yyyy"
        };

        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var exactResult))
            {
                return exactResult;
            }

            if (DateTime.TryParse(value, out var result))
            {
                return result;
            }

            ConsolePrinter.WriteWarning("Введите дату в формате 2026-06-01 18:30 или 01.06.2026 18:30.");
        }
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine();

            if (int.TryParse(value, out var number) && number > 0)
            {
                return number;
            }

            ConsolePrinter.WriteWarning("Введите положительное число.");
        }
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Нажмите Enter, чтобы продолжить...");
        Console.ReadLine();
    }
}
