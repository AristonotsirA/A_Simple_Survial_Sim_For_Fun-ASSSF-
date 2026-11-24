using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>타이틀, 브리핑, 연구, 권역 선택, 통계, 엔딩 화면.</summary>
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
            Ui.WriteLine("        Z O M B I E   O U T B R E A K   ─   세계 대응 시뮬레이션", ConsoleColor.DarkGray);
            Console.WriteLine("\n");
            Ui.WriteLine("        정체불명의 바이러스가 죽은 자를 일으켜 세우기 시작했습니다.");
            Ui.WriteLine("        당신은 UN 산하 '세계 대응 사령부(GRC)'의 사령관입니다.");
            Ui.WriteLine("        196개국을 지휘해 확산을 막고, 치료제를 완성해 인류를 지켜내십시오.");
            Console.WriteLine("\n");
            Ui.Write("        [1] ", ConsoleColor.Yellow); Ui.WriteLine($"쉬움     예산 {Difficulty.Easy.StartFunds}, 느린 확산");
            Ui.Write("        [2] ", ConsoleColor.Yellow); Ui.WriteLine($"보통     예산 {Difficulty.Normal.StartFunds}, 권장 난이도");
            Ui.Write("        [3] ", ConsoleColor.Yellow); Ui.WriteLine($"어려움   예산 {Difficulty.Hard.StartFunds}, 빠른 확산 · 느린 연구");
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
        Ui.Title("플레이 방법", "LAST LINE");
        string[] lines =
        [
            "■ 목표",
            "  치료제 연구를 100%까지 끌어올리면 전 세계 배포가 시작됩니다. 남은 좀비가 사라지면 승리합니다.",
            $"  생존자가 세계 인구의 {Simulation.HumanityDefeatRatio * 100:0}% 아래로 떨어지거나, 신뢰도가 0이 되면 패배합니다.",
            "",
            "■ 한 턴 = 1주",
            "  예산 안에서 원하는 만큼 조치를 내린 뒤 [Enter]로 시간을 진행합니다.",
            "",
            "■ 감염은 이렇게 퍼집니다",
            "  · 국가 내부: 질서가 낮고 가난한 나라일수록 빠르게 퍼지고, 부유한 나라는 스스로 버티기도 합니다.",
            "  · 육로: 같은 권역 국가와 이웃 권역으로 넘어갑니다. 섬나라는 육로 전파에 강합니다.",
            "  · 항공: 허브공항이 있는 나라(미국, 중국, 영국, UAE 등)는 감염되면 전 세계로 퍼뜨립니다.",
            "",
            "■ 주요 조치",
            "  · 국경 봉쇄: 유입을 막지만, 봉쇄국이 많을수록 수입이 줄어듭니다.",
            "  · 군 파견 / 격리 구역: 이미 감염된 나라의 확산을 억제합니다.",
            "  · 연구 투자: 치료제 연구 속도를 영구적으로 올립니다. 가장 중요한 투자입니다.",
            "  · 전술 핵: 최후의 수단. 좀비를 전멸시키지만 대가가 큽니다.",
            "",
            "■ 연구와 예산은 '정상 작동 중인 부유한 나라'에서 나옵니다.",
            "  선진국이 무너지면 연구도 경제도 함께 무너집니다. 어디를 지킬지 선택하십시오.",
        ];
        foreach (var line in lines) Ui.WriteLine(" " + line, line.StartsWith('■') ? ConsoleColor.Yellow : ConsoleColor.Gray);
        Ui.Pause();
    }

    public static void Briefing(World w)
    {
        var zero = w.PatientZero;
        Ui.Title("작전 개시", $"난이도 {w.Difficulty.Name}");
        Console.WriteLine();
        Ui.WriteLine("  [1급 기밀] GRC 사령관 귀하", ConsoleColor.Red);
        Console.WriteLine();
        Ui.WriteLine($"  {zero.Region.Name} 권역의 {zero.Name}에서 '사망자가 다시 일어나 사람을 공격한다'는 보고가 접수되었습니다.");
        Ui.WriteLine("  현지 당국은 이미 통제력을 잃어가고 있으며, 감염자는 물린 지 수 시간 만에 변이합니다.");
        Console.WriteLine();
        Ui.WriteLine("  귀하에게 전 세계 대응의 전권이 위임되었습니다.");
        Ui.WriteLine("  확산을 늦추고, 치료제가 완성될 때까지 인류 문명을 지켜내십시오.");
        Console.WriteLine();
        Ui.WriteLine($"  최초 발병국 : {zero.Name} ({zero.WealthName}{(zero.IsHub ? ", 허브공항 보유" : "")})", ConsoleColor.Yellow);
        Ui.WriteLine($"  가용 예산   : {Fmt.Money(w.Funds)}", ConsoleColor.Yellow);
        Console.WriteLine();
        Ui.WriteLine("  TIP: 첫 주에 발병국에 군 파견 + 격리를 하면 초기 확산을 크게 늦출 수 있습니다.", ConsoleColor.DarkGray);
        Ui.Pause(" 아무 키나 눌러 작전을 시작합니다...");
    }

    public static Region? SelectRegion(World w)
    {
        string input = Ui.Prompt("\n 지도에 표시된 권역 번호(1~19)를 입력하세요 (Enter=취소) > ");
        return int.TryParse(input, out int n) && n >= 1 && n <= w.Regions.Count ? w.Regions[n - 1] : null;
    }

    public static void Research(World w)
    {
        string? message = null;
        bool success = true;

        while (true)
        {
            Ui.Title("치료제 연구", $"보유 예산 {Fmt.Money(w.Funds)}");
            Ui.Write(" 진행도    ");
            Ui.WriteLine(Ui.Bar(w.Research / 100, 40) + $" {w.Research:0.0}%", ConsoleColor.Cyan);
            Ui.WriteLine($" 주간 진척  +{w.LastResearchGain:0.00}%  (연구 기반 {w.Capacity() * 100:0}% 가동 중)");
            Console.WriteLine();
            Ui.WriteLine(" 연구 속도는 '투자 단계'와 '정상 작동 중인 고소득·중소득국의 생존자 수'로 결정됩니다.", ConsoleColor.DarkGray);
            Ui.WriteLine(" 선진국이 붕괴하면 연구 기반도 함께 줄어듭니다.", ConsoleColor.DarkGray);
            Ui.Rule();

            for (int lv = 1; lv <= Actions.MaxResearchLevel; lv++)
            {
                bool done = lv <= w.ResearchLevel;
                Ui.Write(done ? "  [완료] " : "  [    ] ", done ? ConsoleColor.Green : ConsoleColor.DarkGray);
                Ui.WriteLine($"{lv}단계 투자   연구 속도 +{lv * Actions.ResearchBonusPerLevel * 100:0}%   비용 {Fmt.Money(Actions.ResearchLevelCost(lv))}",
                    done ? ConsoleColor.Green : ConsoleColor.Gray);
            }
            Ui.Rule();

            if (w.ResearchLevel < Actions.MaxResearchLevel && !w.CureDeployed)
            {
                Ui.Write(" [1] ", ConsoleColor.Yellow);
                Ui.WriteLine($"다음 단계 투자 ({Fmt.Money(Actions.ResearchUpgradeCost(w))})");
            }
            Ui.WriteLine(" [Enter/Esc] 뒤로", ConsoleColor.DarkGray);
            Ui.ShowResult(message, success);

            var key = Ui.ReadKey();
            if (key.Key is ConsoleKey.Enter or ConsoleKey.Escape) return;
            if (key.KeyChar == '1')
            {
                var result = Actions.UpgradeResearch(w);
                message = result.Message;
                success = result.Success;
            }
        }
    }

    public static void Statistics(World w)
    {
        Ui.Title("세계 현황 통계", $"제 {w.Week}주차");

        Ui.WriteLine(" 생존자 비율 추이", ConsoleColor.White);
        Ui.WriteLine(" " + Sparkline(w.History.Select(h => h.Healthy / w.InitialPopulation).ToList(), fixedMax: 1), ConsoleColor.Green);
        Ui.WriteLine(" 좀비 수 추이", ConsoleColor.White);
        Ui.WriteLine(" " + Sparkline(w.History.Select(h => h.Zombies).ToList()), ConsoleColor.Red);
        Ui.Rule();

        Ui.WriteLine(" 피해가 가장 큰 국가 TOP 8", ConsoleColor.White);
        foreach (var c in w.Countries.Where(c => c.IsInfected).OrderByDescending(c => c.Zombies).Take(8))
        {
            Ui.Write("  " + Ui.Pad(c.Name, 22), Ui.CountryColor(c));
            Ui.Write(Ui.Bar(c.ZombieRatio, 20) + " ", Ui.CountryColor(c));
            Ui.WriteLine($"{Ui.PadLeft(Fmt.Pct(c.ZombieRatio), 6)}   좀비 {Fmt.People(c.Zombies)}", ConsoleColor.Gray);
        }
        Ui.Rule();

        Ui.WriteLine(" 권역별 현황", ConsoleColor.White);
        var regions = w.Regions.OrderByDescending(r => r.ZombieRatio).ToList();
        for (int i = 0; i < regions.Count; i += 2)
        {
            for (int j = i; j < Math.Min(i + 2, regions.Count); j++)
            {
                var r = regions[j];
                var color = Ui.InfectionColor(r.ZombieRatio, r.InfectedCount > 0);
                Ui.Write("  " + Ui.Pad(r.Name, 14), color);
                Ui.Write(Ui.Bar(r.ZombieRatio, 12) + Ui.PadLeft(Fmt.Pct(r.ZombieRatio), 7), color);
                Ui.Write(Ui.Pad($"  감염 {r.InfectedCount}/{r.Countries.Count}", 14), ConsoleColor.DarkGray);
                Console.Write("     ");
            }
            Console.WriteLine();
        }
        Ui.Pause();
    }

    /// <summary>숫자 목록을 ▁▂▃▄▅▆▇█ 막대로 표현한 한 줄 그래프. 최근 100주까지 표시.</summary>
    private static string Sparkline(List<double> values, double? fixedMax = null)
    {
        const string blocks = " ▁▂▃▄▅▆▇█";
        var recent = values.TakeLast(100).ToList();
        double max = fixedMax ?? Math.Max(1, recent.DefaultIfEmpty(0).Max());
        return new string(recent.Select(v => blocks[(int)Math.Round(Math.Clamp(v / max, 0, 1) * (blocks.Length - 1))]).ToArray());
    }

    /// <summary>엔딩 화면. 다시 하기를 고르면 true.</summary>
    public static bool Ending(World w, GameResult result)
    {
        double survival = w.TotalHealthy / w.InitialPopulation;
        var (title, color, lines) = result switch
        {
            GameResult.Victory => ("인류 생존", ConsoleColor.Green, new[]
            {
                "마지막 좀비가 쓰러졌습니다. 거리에는 다시 사람들의 목소리가 돌아오고 있습니다.",
                "역사는 이 시기를 '최후의 방어선'이라 부르며, 당신의 이름을 기억할 것입니다.",
            }),
            GameResult.DefeatHumanity => ("문명 붕괴", ConsoleColor.DarkRed, new[]
            {
                "생존자들은 흩어진 벙커와 외딴 섬으로 숨어들었습니다.",
                "인류 문명은 막을 내렸고, 지구의 주인은 바뀌었습니다.",
            }),
            GameResult.DefeatTrust => ("사령관 해임", ConsoleColor.Red, new[]
            {
                "각국 정부가 GRC에 대한 지지를 철회했습니다. 당신은 지휘권을 박탈당했습니다.",
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

        Console.Clear();
        Console.WriteLine("\n\n");
        Ui.WriteLine($"        ━━━━━━━━━━  {title}  ━━━━━━━━━━", color);
        Console.WriteLine();
        foreach (var line in lines) Ui.WriteLine("        " + line);
        Console.WriteLine("\n");
        Ui.WriteLine($"        작전 기간       {w.Week - 1}주");
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

        while (true)
        {
            var key = Ui.ReadKey();
            if (Ui.IsLetter(key, 'r')) return true;
            if (Ui.IsLetter(key, 'q') || key.Key == ConsoleKey.Escape) return false;
        }
    }
}
