using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>도움말 문구 (타이틀의 [H]와 게임 중 [H]에서 함께 사용).</summary>
public static class HelpText
{
    public static readonly (string Text, ConsoleColor Color)[] Lines =
    [
        ("", ConsoleColor.Gray),
        ("■ 목표", ConsoleColor.Yellow),
        ("  치료제를 100%까지 연구하면 전 세계 배포가 시작됩니다.", ConsoleColor.Gray),
        ("  남은 좀비가 사라지면 승리합니다.", ConsoleColor.Gray),
        ($"  생존자가 세계 인구의 {Simulation.HumanityDefeatRatio * 100:0}% 미만이거나 신뢰도가 0이면 패배입니다.", ConsoleColor.Gray),
        ("", ConsoleColor.Gray),
        ("■ 조작", ConsoleColor.Yellow),
        ("  방향키로 지도 위 나라를 고르고, 단축키로 조치합니다.", ConsoleColor.Gray),
        ("  [B] 국경 봉쇄  감염 유입을 막지만, 봉쇄국이 많을수록 수입이 줄어듭니다.", ConsoleColor.Gray),
        ("  [M] 방역 작전  4주간 감염 속도를 낮추고 좀비를 빠르게 처치합니다.", ConsoleColor.Gray),
        ("  [N] 전술 핵    좀비가 전멸하지만 주민 대부분이 죽습니다. 최후의 수단.", ConsoleColor.Gray),
        ("  [R] 연구 투자  치료제 연구 속도를 영구히 올립니다. 가장 중요합니다.", ConsoleColor.Gray),
        ("", ConsoleColor.Gray),
        ("■ 지도 색깔", ConsoleColor.Yellow),
        ("  초록 청정   금색 감염 시작   빨강 확산   진홍 심각   보라 정부 붕괴   회색 핵 폐허", ConsoleColor.Gray),
        ("", ConsoleColor.Gray),
        ("■ 팁", ConsoleColor.Yellow),
        ("  연구와 예산은 '정상 작동 중인 부유한 나라'에서 나옵니다.", ConsoleColor.Gray),
        ("  허브공항 나라(미국·중국·영국·UAE 등)가 뚫리면 전 세계로 퍼집니다.", ConsoleColor.Gray),
        ("", ConsoleColor.Gray),
    ];
}

/// <summary>타이틀, 브리핑, 엔딩, 창 크기 안내 화면.</summary>
public static class MenuScreens
{
    private static readonly string[] Logo =
    [
        @" _        _     ____  _____     _      ___  _   _  _____ ",
        @"| |      / \   / ___||_   _|   | |    |_ _|| \ | || ____|",
        @"| |     / _ \  \___ \  | |     | |     | | |  \| ||  _|  ",
        @"| |___ / ___ \  ___) | | |     | |___  | | | |\  || |___ ",
        @"|_____/_/   \_\|____/  |_|     |_____||___||_| \_||_____|",
    ];

    /// <summary>타이틀 화면. 선택한 난이도를 반환하고, 종료를 고르면 null.</summary>
    public static Difficulty? Title()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("\n\n");
            foreach (var line in Logo) Ui.WriteLine("        " + line, ConsoleColor.Red);
            Console.WriteLine();
            Ui.WriteLine("        Z O M B I E   O U T B R E A K", ConsoleColor.DarkGray);
            Console.WriteLine("\n");
            Ui.WriteLine("        정체불명의 바이러스가 죽은 자를 일으켜 세우기 시작했습니다.");
            Ui.WriteLine("        당신은 '세계 대응 사령부'의 사령관입니다. 확산을 막고 치료제를 완성하십시오.");
            Console.WriteLine("\n");
            Ui.Write("        [1] ", ConsoleColor.Yellow); Ui.WriteLine("쉬움     느린 확산, 넉넉한 예산");
            Ui.Write("        [2] ", ConsoleColor.Yellow); Ui.WriteLine("보통     권장 난이도");
            Ui.Write("        [3] ", ConsoleColor.Yellow); Ui.WriteLine("어려움   빠른 확산, 느린 연구");
            Console.WriteLine();
            Ui.Write("        [H] ", ConsoleColor.Yellow); Ui.WriteLine("플레이 방법");
            Ui.Write("        [Q] ", ConsoleColor.DarkGray); Ui.WriteLine("종료", ConsoleColor.DarkGray);

            var key = Ui.ReadKey();
            switch (key.KeyChar)
            {
                case '1': return Difficulty.Easy;
                case '2': return Difficulty.Normal;
                case '3': return Difficulty.Hard;
            }
            if (Ui.IsLetter(key, 'h')) HowToPlay();
            else if (Ui.IsLetter(key, 'q') || key.Key == ConsoleKey.Escape) return null;
        }
    }

    private static void HowToPlay()
    {
        Console.Clear();
        Ui.WriteLine("\n  플레이 방법\n", ConsoleColor.White);
        foreach (var (text, color) in HelpText.Lines) Ui.WriteLine("  " + text, color);
        Ui.Pause("  아무 키나 누르면 돌아갑니다...");
    }

    public static void Briefing(World w)
    {
        var zero = w.PatientZero;
        Console.Clear();
        Console.WriteLine("\n\n");
        Ui.WriteLine("    [1급 기밀] 사령관 귀하", ConsoleColor.Red);
        Console.WriteLine();
        Ui.WriteLine($"    {zero.Name}에서 '죽은 사람이 다시 일어나 사람을 공격한다'는 보고가 접수되었습니다.");
        Ui.WriteLine("    현지 당국은 이미 통제력을 잃어가고 있습니다.");
        Console.WriteLine();
        Ui.WriteLine("    귀하에게 전 세계 대응의 전권이 위임되었습니다.");
        Ui.WriteLine("    확산을 늦추고, 치료제가 완성될 때까지 인류를 지켜내십시오.");
        Console.WriteLine();
        Ui.WriteLine($"    최초 발병국   {zero.Name} ({zero.WealthName}{(zero.IsHub ? ", 허브공항" : "")})", ConsoleColor.Yellow);
        Ui.WriteLine($"    가용 예산     {Fmt.Money(w.Funds)}", ConsoleColor.Yellow);
        Console.WriteLine();
        Ui.WriteLine("    시간은 자동으로 흐릅니다. 급할 땐 [Space]로 멈추고 생각하세요.", ConsoleColor.DarkGray);
        Ui.WriteLine("    TIP: 시작하자마자 발병국에 [M] 방역 작전을 걸면 초기 확산을 크게 늦출 수 있습니다.", ConsoleColor.DarkGray);
        Ui.Pause("    아무 키나 눌러 작전을 시작합니다...");
    }

    /// <summary>창이 지도를 담기에 충분히 큰지 확인하고, 작으면 키울 때까지 안내합니다.</summary>
    public static void EnsureWindowSize()
    {
        int needW = ScreenBuffer.Width + 1, needH = ScreenBuffer.Height;
        if (Console.WindowWidth >= needW && Console.WindowHeight >= needH) return;

        while (Console.WindowWidth < needW || Console.WindowHeight < needH)
        {
            Console.Clear();
            Ui.WriteLine("\n  창이 너무 작아 세계지도를 표시할 수 없습니다.", ConsoleColor.Yellow);
            Ui.WriteLine($"  현재 {Console.WindowWidth} x {Console.WindowHeight}  →  필요 {needW} x {needH} 이상");
            Ui.WriteLine("\n  창을 최대화하거나 글꼴 크기를 줄여 주세요 (Ctrl + 마우스 휠).", ConsoleColor.DarkGray);
            Thread.Sleep(500);
        }
        Console.Clear();
    }

    /// <summary>엔딩 화면. 다시 하기를 고르면 true.</summary>
    public static bool Ending(World w, GameResult result)
    {
        double survival = w.TotalHealthy / w.InitialPopulation;
        var (title, color, lines) = result switch
        {
            GameResult.Victory => ("인류 생존", ConsoleColor.Green, new[]
            {
                "마지막 좀비가 쓰러졌습니다. 거리에 다시 사람들의 목소리가 돌아옵니다.",
                "역사는 이 시기를 '최후의 방어선'이라 부를 것입니다.",
            }),
            GameResult.DefeatHumanity => ("문명 붕괴", ConsoleColor.DarkRed, new[]
            {
                "생존자들은 흩어진 벙커와 외딴 섬으로 숨어들었습니다.",
                "인류 문명은 막을 내렸고, 지구의 주인은 바뀌었습니다.",
            }),
            GameResult.DefeatTrust => ("사령관 해임", ConsoleColor.Red, new[]
            {
                "각국 정부가 사령부에 대한 지지를 철회했습니다. 당신은 지휘권을 박탈당했습니다.",
                "세계는 각자도생의 길로 들어섰습니다.",
            }),
            _ => ("작전 포기", ConsoleColor.DarkGray, new[] { "사령관이 지휘를 내려놓았습니다." }),
        };

        string grade = result != GameResult.Victory ? "F" : survival switch
        {
            >= 0.9 => "S",
            >= 0.75 => "A",
            >= 0.6 => "B",
            >= 0.45 => "C",
            _ => "D"
        };

        Console.ResetColor();
        Console.Clear();
        Console.WriteLine("\n\n");
        Ui.WriteLine($"        ━━━━━━━━━━  {title}  ━━━━━━━━━━", color);
        Console.WriteLine();
        foreach (var line in lines) Ui.WriteLine("        " + line);
        Console.WriteLine("\n");
        Ui.WriteLine($"        작전 기간       {w.Day}일 ({w.Week}주차)");
        Ui.WriteLine($"        생존자          {Fmt.People(w.TotalHealthy)}  (세계 인구의 {survival * 100:0.0}%)", ConsoleColor.Green);
        Ui.WriteLine($"        사망자          {Fmt.People(w.TotalDead)}", ConsoleColor.DarkGray);
        Ui.WriteLine($"        감염된 국가     {w.Countries.Count(c => c.Dead > 0 || c.IsInfected)} / {w.Countries.Count}");
        Ui.WriteLine($"        붕괴한 국가     {w.CollapsedCount}", ConsoleColor.Magenta);
        Ui.WriteLine($"        핵 투하         {w.Countries.Count(c => c.Nuked)}회", ConsoleColor.DarkRed);
        Ui.WriteLine($"        치료제 진행도   {w.Research:0.0}%", ConsoleColor.Cyan);
        Console.WriteLine();
        Ui.Write("        최종 등급   ");
        Ui.WriteLine(grade, grade == "F" ? ConsoleColor.Red : ConsoleColor.Yellow);
        Console.WriteLine("\n");
        Ui.Write("        [R] ", ConsoleColor.Yellow); Ui.Write("다시 하기     ");
        Ui.Write("[Q] ", ConsoleColor.DarkGray); Ui.WriteLine("종료", ConsoleColor.DarkGray);

        while (Console.KeyAvailable) Console.ReadKey(true);   // 게임 중 눌린 키가 남아 있으면 비움
        while (true)
        {
            var key = Ui.ReadKey();
            if (Ui.IsLetter(key, 'r')) return true;
            if (Ui.IsLetter(key, 'q') || key.Key == ConsoleKey.Escape) return false;
        }
    }
}
