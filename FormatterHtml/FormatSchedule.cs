using System.Globalization;

namespace TgPetProject.FormatterHtml;

public class FormatSchedule
{
    public async Task<string> Format(string page)
    {
        var listDayOfWeek = new HashSet<string>
        {
            "пн", "вт", "ср", "чт", "пт", "сб", "вс"
        };
        var flag = true;
        var lines = page
            .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            .Select(x => x.Contains("\u2116") ? "№ пары, Время, Предмет" : x)
            .Where(x => !string.IsNullOrWhiteSpace(x.Trim()) && x.Trim() != "Время" && x.Trim() != "Предмет")
            .ToArray();
        var result = new List<string>();
        var i = 0;
        while (i < lines.Length)
        {
            var line = lines[i].Trim();
            if (listDayOfWeek.Any(x => line.Contains(x)))
            {
                i++;
                continue;
            }

            if (DateTime.TryParse(line, out var date))
            {
                var dayOfWeek = date.ToString("dddd", new CultureInfo("ru-RU"));
                var dayOfWeekWithCapital = char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1).ToLower();
                if (i == 1)
                    result.Add($"{line}, {dayOfWeekWithCapital}");
                else
                    result.Add($"==========================\n\n\n{line}, {dayOfWeekWithCapital}");
            }
            else if (int.TryParse(line, out _))
            {
                var tempLine = $"\n{lines[i].Trim()}, {lines[i + 1].Trim()}, {lines[i + 2].Trim()}";
                i += 2;
                result.Add(tempLine);
            }
            else
            {
                if (flag && line.Contains("Онлайн"))
                {
                    flag = false;
                    result.Add($"==========================\n\n\n{line}");
                }
                else
                {
                    result.Add(line);
                }
            }

            i++;
        }

        return string.Join("\n", result);
    }
}