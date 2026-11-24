using ZombieOutbreak.Core;

namespace ZombieOutbreak.UI;

/// <summary>메인 화면: 상단 지표 + 세계 지도 + 최근 소식 + 메뉴.</summary>
public static class HudScreen
{
    private const int TileWidth = 18;
    private const int MapRows = 4;
    private const int MapCols = 6;
    private const int NewsLines = 5;

    public static void Draw(World w, string? message = null, bool success = true)
    {
        Console.Clear();
        DrawHeader(w);
        Ui.Rule();
        DrawMap(w);
        Ui.Rule();
        DrawNews(w);
        Ui.Rule();
        Ui.ShowResult(message, success);
        Ui.Write(" [1]", ConsoleColor.Yellow); Ui.Write("국가 조치  ");
        Ui.Write("[2]", ConsoleColor.Yellow); Ui.Write("권역 보기  ");
        Ui.Write("[3]", ConsoleColor.Yellow); Ui.Write("연구 투자  ");
        Ui.Write("[4]", ConsoleColor.Yellow); Ui.Write("대국민 담화  ");
        Ui.Write("[5]", ConsoleColor.Yellow); Ui.Write("통계  ");
        Ui.Write("[Enter]", ConsoleColor.Green); Ui.Write("다음 주  ");
        Ui.Write("[Esc]", ConsoleColor.DarkGray); Ui.WriteLine("포기", ConsoleColor.DarkGray);
    }

    private static void DrawHeader(World w)
    {
        Ui.Write(" LAST LINE", ConsoleColor.Red);
        Ui.Write("  세계 대응 사령부", ConsoleColor.DarkGray);
        string right = $"제 {w.Week}주차 · 난이도 {w.Difficulty.Name}";
        Ui.WriteLine(Ui.PadLeft(right, Ui.ScreenWidth - 26), ConsoleColor.White);

        // 1줄: 예산 / 신뢰도 / 치료제
        Ui.Write(" 예산 ");
        Ui.Write(Fmt.Money(w.Funds), ConsoleColor.Yellow);
        Ui.Write($" (+{w.LastIncome:0.0}/주)   ", ConsoleColor.DarkGray);

        Ui.Write("신뢰도 ");
        Ui.Write(Ui.Bar(w.Trust / 100, 10) + $" {w.Trust:0}   ", Ui.TrustColor(w.Trust));

        Ui.Write("치료제 ");
        if (w.CureDeployed)
        {
            Ui.WriteLine("완성! 전 세계 배포 중", ConsoleColor.Green);
        }
        else
        {
            Ui.Write(Ui.Bar(w.Research / 100, 10) + $" {w.Research:0.0}%", ConsoleColor.Cyan);
            Ui.WriteLine($" (+{w.LastResearchGain:0.0}/주 · 투자 {w.ResearchLevel}단계)", ConsoleColor.DarkGray);
        }

        // 2줄: 인구 통계
        double healthy = w.TotalHealthy;
        Ui.Write(" 생존 ");
        Ui.Write($"{Fmt.People(healthy)} ({healthy / w.InitialPopulation * 100:0.0}%)   ", ConsoleColor.Green);
        Ui.Write("좀비 ");
        Ui.Write($"{Fmt.People(w.TotalZombies)}   ", ConsoleColor.Red);
        Ui.Write("사망 ");
        Ui.Write($"{Fmt.People(w.TotalDead)}   ", ConsoleColor.DarkGray);
        Ui.Write("감염국 ");
        Ui.Write($"{w.InfectedCount}/{w.Countries.Count}   ", ConsoleColor.Yellow);
        Ui.Write("붕괴국 ");
        Ui.WriteLine($"{w.CollapsedCount}", ConsoleColor.Magenta);
    }

    // ───────────────────────── 세계 지도 ─────────────────────────

    private static void DrawMap(World w)
    {
        var grid = new Region?[MapRows, MapCols];   // 2차원 배열
        foreach (var r in w.Regions) grid[r.MapRow, r.MapCol] = r;

        for (int row = 0; row < MapRows; row++)
        {
            for (int line = 0; line < 3; line++)
            {
                Console.Write(" ");
                for (int col = 0; col < MapCols; col++)
                {
                    DrawTileLine(grid[row, col], line, col);
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
            if (row < MapRows - 1) Console.WriteLine();
        }
    }

    /// <summary>타일 한 칸(18칸 폭)의 line번째 줄을 그립니다. 0: 이름, 1: 감염 막대, 2: 감염국 수</summary>
    private static void DrawTileLine(Region? r, int line, int col)
    {
        if (r == null)
        {
            Ui.Write(Ocean(line, col), ConsoleColor.DarkBlue);
            return;
        }

        int infected = r.InfectedCount;
        double ratio = r.ZombieRatio;
        var color = Ui.InfectionColor(ratio, infected > 0);

        switch (line)
        {
            case 0:
                Ui.Write($"[{r.Number,2}] ", ConsoleColor.DarkGray);
                Ui.Write(Ui.Pad(r.Name, TileWidth - 5), infected > 0 ? color : ConsoleColor.White);
                break;
            case 1:
                Ui.Write(Ui.Bar(ratio, 11) + Ui.PadLeft(Fmt.Pct(ratio), 7), color);
                break;
            default:
                string text = infected == 0
                    ? "청정"
                    : $"감염 {infected}/{r.Countries.Count}" + (r.CollapsedCount > 0 ? $" 붕괴 {r.CollapsedCount}" : "");
                Ui.Write(Ui.Pad(text, TileWidth), infected == 0 ? ConsoleColor.DarkGreen : color);
                break;
        }
    }

    private static string Ocean(int line, int col)
    {
        var chars = new char[TileWidth];
        for (int i = 0; i < TileWidth; i++)
            chars[i] = (i + line * 3 + col * 5) % 7 == 0 ? '~' : ' ';
        return new string(chars);
    }

    private static void DrawNews(World w)
    {
        Ui.WriteLine(" 최근 소식", ConsoleColor.White);
        var recent = w.News.TakeLast(NewsLines).ToList();
        for (int i = 0; i < NewsLines; i++)
        {
            if (i >= recent.Count) { Console.WriteLine(); continue; }
            Ui.Write($" {recent[i].Week,3}주  ", ConsoleColor.DarkGray);
            Ui.WriteLine(Ui.Pad(recent[i].Text, Ui.ScreenWidth - 8), recent[i].Color);
        }
    }
}
