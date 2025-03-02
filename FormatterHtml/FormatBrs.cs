using System.Text.RegularExpressions;

namespace TgPetProject.FormatterHtml;

public class FormatBrs
{
    public async Task<string> Format(string page)
    {
        var lines = page
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None) // Разбиваем по строкам
            .Select(x => x.Trim()) // Убираем лишние пробелы
            .Where(x => !string.IsNullOrEmpty(x)) // Удаляем пустые строки
            .ToArray();
        var pattern = @"^\d{1,2}.\d{2}$";
        var i = 0;
        var result = new List<string>();
        while (i < lines.Length)
        {
            var line = lines[i].Trim();
            if (line.Contains("Итоговая оценка"))
            {
                var tempLine = $"{line} {lines[i + 1].Trim()}";
                result.Add(tempLine);
                i++;
            }
            else if (Regex.IsMatch(line, pattern))
            {
                if (i == lines.Length - 2)
                {
                    result.Add($"Итоговый балл: {line}\n==========================");
                    i++;
                }
                else
                {
                    result.Add($"Итоговый балл: {line}");
                    i++;
                }
            }
            else
            {
                result.Add(i == 0 ? $"{line}\n" : $"==========================\n{line}\n");
            }

            i++;
        }

        return string.Join("\n", result);
    }
}