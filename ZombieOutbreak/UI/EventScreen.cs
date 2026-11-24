using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>사건 카드 화면. 선택지를 고르면 효과를 적용하고 소식에 기록합니다.</summary>
public static class EventScreen
{
    private const int CardWidth = 76;
    private const string Margin = "      ";

    public static void Show(World w, DrawnEvent drawn)
    {
        var ev = drawn.Event;
        string? message = null;

        while (true)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine();
            BorderLine('┌', '┐', $" 긴급 보고 · 제 {w.Week}주차 ");
            CardLine("");
            CardLine(ev.Title, ConsoleColor.Yellow);
            if (drawn.Target != null) CardLine($"대상: {drawn.Target.Name} ({Fmt.Pct(drawn.Target.ZombieRatio)} 감염)", ConsoleColor.DarkGray);
            CardLine("");
            foreach (var line in Ui.Wrap(ev.Body(drawn.Target), CardWidth - 6)) CardLine(line);
            CardLine("");
            BorderLine('├', '┤', "");

            for (int i = 0; i < ev.Choices.Length; i++)
            {
                var choice = ev.Choices[i];
                bool affordable = w.Funds >= choice.Cost;
                CardLine($"[{i + 1}] {choice.Label}", affordable ? ConsoleColor.White : ConsoleColor.DarkGray);
                CardLine($"    {choice.Effect}", affordable ? ConsoleColor.Cyan : ConsoleColor.DarkGray);
            }
            CardLine("");
            BorderLine('└', '┘', $" 보유 예산 {Fmt.Money(w.Funds)} ");

            if (message != null) Ui.WriteLine($"\n{Margin}{message}", ConsoleColor.Red);

            int index = Ui.ReadKey().KeyChar - '1';   // '1' → 0, '2' → 1 ...
            if (index < 0 || index >= ev.Choices.Length) continue;

            var picked = ev.Choices[index];
            if (w.Funds < picked.Cost)
            {
                message = "예산이 부족해 이 선택지를 고를 수 없습니다.";
                continue;
            }

            w.Funds -= picked.Cost;
            picked.Apply(w, drawn.Target);
            w.AddNews($"[결정] {ev.Title} → {picked.Label}", ConsoleColor.Cyan);
            return;
        }
    }

    private static void BorderLine(char left, char right, string caption)
    {
        int rest = CardWidth - 2 - Ui.Width(caption);
        Ui.WriteLine(Margin + left + "──" + caption + new string('─', rest - 2) + right, ConsoleColor.DarkYellow);
    }

    private static void CardLine(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        Ui.Write(Margin + "│  ", ConsoleColor.DarkYellow);
        Ui.Write(Ui.Pad(text, CardWidth - 4), color);
        Ui.WriteLine("│", ConsoleColor.DarkYellow);
    }
}
