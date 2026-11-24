using System.Text;
using ZombieOutbreak.Core;

namespace ZombieOutbreak.UI;

/// <summary>
/// 콘솔 출력 도우미 모음.
/// 핵심 문제: 한글은 콘솔에서 영문 2칸을 차지합니다. string.PadRight는 '글자 수' 기준이라 표가 틀어지므로,
/// 화면상 '칸 수'(display width)를 직접 계산하는 Width / Pad 함수를 씁니다.
/// </summary>
public static class Ui
{
    public const int ScreenWidth = 115;

    public static int Width(string s)
    {
        int width = 0;
        foreach (char ch in s) width += IsWide(ch) ? 2 : 1;
        return width;
    }

    private static bool IsWide(char c) =>
        (c >= 0x1100 && c <= 0x115F) || (c >= 0x2E80 && c <= 0xA4CF) || (c >= 0xAC00 && c <= 0xD7A3) ||
        (c >= 0xF900 && c <= 0xFAFF) || (c >= 0xFF00 && c <= 0xFF60) || (c >= 0xFFE0 && c <= 0xFFE6);

    /// <summary>화면 칸 수 기준으로 오른쪽을 공백으로 채웁니다. 넘치면 자릅니다.</summary>
    public static string Pad(string s, int width)
    {
        var sb = new StringBuilder();
        int used = 0;
        foreach (char ch in s)
        {
            int w = IsWide(ch) ? 2 : 1;
            if (used + w > width) break;
            sb.Append(ch);
            used += w;
        }
        return sb.Append(' ', width - used).ToString();
    }

    public static string PadLeft(string s, int width) => new string(' ', Math.Max(0, width - Width(s))) + s;

    public static void Write(string s, ConsoleColor color = ConsoleColor.Gray)
    {
        Console.ForegroundColor = color;
        Console.Write(s);
        Console.ResetColor();
    }

    public static void WriteLine(string s = "", ConsoleColor color = ConsoleColor.Gray)
    {
        Write(s, color);
        Console.WriteLine();
    }

    public static void Rule(ConsoleColor color = ConsoleColor.DarkGray) => WriteLine(" " + new string('─', ScreenWidth), color);

    /// <summary>화면 제목 줄: 왼쪽에 제목, 오른쪽 끝에 부제.</summary>
    public static void Title(string left, string right = "")
    {
        Console.Clear();
        Write(" " + left, ConsoleColor.White);
        Write(PadLeft(right, ScreenWidth - Width(left)) + "\n", ConsoleColor.DarkGray);
        Rule();
    }

    public static string Bar(double ratio, int width)
    {
        int filled = (int)Math.Round(Math.Clamp(ratio, 0, 1) * width);
        return new string('█', filled) + new string('░', width - filled);
    }

    /// <summary>감염률에 따른 색상. 청정 → 초록, 감염이 심할수록 노랑 → 빨강.</summary>
    public static ConsoleColor InfectionColor(double ratio, bool infected) => !infected ? ConsoleColor.Green : ratio switch
    {
        < 0.01 => ConsoleColor.Yellow,
        < 0.10 => ConsoleColor.DarkYellow,
        < 0.40 => ConsoleColor.Red,
        _ => ConsoleColor.DarkRed
    };

    public static ConsoleColor CountryColor(Country c) =>
        c.Nuked ? ConsoleColor.DarkGray :
        c.Collapsed ? ConsoleColor.Magenta :
        InfectionColor(c.ZombieRatio, c.IsInfected);

    public static ConsoleColor TrustColor(double trust) =>
        trust >= 50 ? ConsoleColor.Green : trust >= 25 ? ConsoleColor.Yellow : ConsoleColor.Red;

    /// <summary>화면 칸 수 기준 자동 줄바꿈.</summary>
    public static List<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        var line = new StringBuilder();
        foreach (var word in text.Split(' '))
        {
            if (line.Length > 0 && Width(line + " " + word) > width)
            {
                lines.Add(line.ToString());
                line.Clear();
            }
            if (line.Length > 0) line.Append(' ');
            line.Append(word);
        }
        if (line.Length > 0) lines.Add(line.ToString());
        return lines;
    }

    public static ConsoleKeyInfo ReadKey() => Console.ReadKey(intercept: true);

    // 한글 입력 상태에서 영문 키를 누르면 자모가 입력되므로(n → ㅜ), 두벌식 자판 기준으로 함께 인정합니다.
    private static readonly Dictionary<char, char> KoreanLayout = new()
    {
        ['f'] = 'ㄹ', ['h'] = 'ㅗ', ['n'] = 'ㅜ', ['p'] = 'ㅔ', ['q'] = 'ㅂ', ['r'] = 'ㄱ', ['y'] = 'ㅛ'
    };

    /// <summary>입력이 영문 letter 키인지 판정 (대소문자, 한글 입력 상태 모두 허용).</summary>
    public static bool IsLetter(string input, char letter) =>
        input.Length == 1 && (char.ToLower(input[0]) == letter || input[0] == KoreanLayout.GetValueOrDefault(letter));

    public static bool IsLetter(ConsoleKeyInfo key, char letter) => IsLetter(key.KeyChar.ToString(), letter);

    public static string Prompt(string label)
    {
        Write(label, ConsoleColor.Yellow);
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static void Pause(string message = " 아무 키나 누르면 계속합니다...")
    {
        Write("\n" + message, ConsoleColor.DarkGray);
        ReadKey();
    }

    public static bool Confirm(string question)
    {
        Write($"\n {question} (Y/N) ", ConsoleColor.Red);
        return IsLetter(ReadKey(), 'y');
    }

    public static void ShowResult(string? message, bool success)
    {
        if (message != null) WriteLine(" ▶ " + message, success ? ConsoleColor.Green : ConsoleColor.Red);
    }
}

/// <summary>숫자 포맷 도우미.</summary>
public static class Fmt
{
    public static string People(double n) => n switch
    {
        >= 1e8 => $"{n / 1e8:0.00}억",
        >= 1e6 => $"{n / 1e4:#,0}만",
        >= 1e4 => $"{n / 1e4:0.0}만",
        >= 1 => $"{n:#,0}명",
        _ => "0"
    };

    public static string Pct(double ratio) => ratio switch
    {
        <= 0 => "-",
        < 0.001 => "<0.1%",
        _ => $"{ratio * 100:0.0}%"
    };

    public static string Money(double funds) => $"${funds:0}B";
}
