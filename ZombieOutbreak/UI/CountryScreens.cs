using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>국가 목록(전체 또는 권역별)과 국가 상세/조치 화면.</summary>
public static class CountryScreens
{
    private const int PageSize = 16;

    public static void ShowList(World w, Region? region = null)
    {
        int page = 0;
        bool infectedOnly = false;
        string? message = null;

        while (true)
        {
            var list = w.Countries
                .Where(c => region == null || c.Region == region)
                .Where(c => !infectedOnly || c.IsInfected)
                .OrderByDescending(c => c.Zombies)
                .ThenByDescending(c => c.InitialPopulation)
                .ToList();

            int pages = Math.Max(1, (list.Count + PageSize - 1) / PageSize);
            page = Math.Clamp(page, 0, pages - 1);

            string title = region == null ? "국가 목록" : $"[{region.Number}] {region.Name}";
            string filter = infectedOnly ? "감염국만" : "전체";
            Ui.Title(title, $"{filter} {list.Count}개국 · {page + 1}/{pages} 페이지 · 예산 {Fmt.Money(w.Funds)}");

            if (region != null)
            {
                Ui.Write(" 권역 감염률 ");
                Ui.Write(Ui.Bar(region.ZombieRatio, 20) + " " + Fmt.Pct(region.ZombieRatio),
                    Ui.InfectionColor(region.ZombieRatio, region.InfectedCount > 0));
                Ui.WriteLine($"   이웃 권역: {string.Join(", ", region.Neighbors.Select(n => n.Name))}", ConsoleColor.DarkGray);
                Console.WriteLine();
            }

            Ui.WriteLine(" " + Ui.Pad("번호", 5) + Ui.Pad("국가", 22) + Ui.Pad("권역", 14) + Ui.PadLeft("인구", 10)
                         + Ui.PadLeft("좀비", 10) + Ui.PadLeft("감염률", 9) + Ui.PadLeft("질서", 7) + "   상태", ConsoleColor.DarkGray);

            foreach (var (c, i) in list.Skip(page * PageSize).Take(PageSize).Select((c, i) => (c, i)))
            {
                Ui.Write(" " + Ui.Pad($"{i + 1,3}", 5), ConsoleColor.Yellow);
                Ui.Write(Ui.Pad(c.Name, 22), Ui.CountryColor(c));
                Ui.Write(Ui.Pad(c.Region.Name, 14), ConsoleColor.DarkGray);
                Ui.Write(Ui.PadLeft(Fmt.People(c.Living), 10));
                Ui.Write(Ui.PadLeft(c.IsInfected ? Fmt.People(c.Zombies) : "-", 10), Ui.CountryColor(c));
                Ui.Write(Ui.PadLeft(Fmt.Pct(c.ZombieRatio), 9), Ui.CountryColor(c));
                Ui.Write(Ui.PadLeft($"{c.Order:0}", 7), c.Order < 30 ? ConsoleColor.Red : ConsoleColor.Gray);
                Ui.WriteLine("   " + c.StatusTags, ConsoleColor.Cyan);
            }

            Ui.Rule();
            Ui.ShowResult(message, false);
            message = null;

            string input = Ui.Prompt(" 번호 선택 · 국가명/코드 검색 · [n]다음 [p]이전 [f]감염국만 · Enter=뒤로 > ");
            if (input == "") return;

            if (Ui.IsLetter(input, 'n')) { page++; continue; }
            if (Ui.IsLetter(input, 'p')) { page--; continue; }
            if (Ui.IsLetter(input, 'f')) { infectedOnly = !infectedOnly; page = 0; continue; }

            if (int.TryParse(input, out int number))
            {
                int index = page * PageSize + number - 1;
                if (number >= 1 && number <= PageSize && index < list.Count) ShowCountry(w, list[index]);
                else message = "목록에 없는 번호입니다.";
                continue;
            }

            var found = w.FindCountries(input);
            if (found.Count == 1) ShowCountry(w, found[0]);
            else if (found.Count == 0) message = $"'{input}'에 해당하는 국가가 없습니다.";
            else message = $"여러 국가가 검색되었습니다: {string.Join(", ", found.Take(8).Select(c => c.Name))}";
        }
    }

    public static void ShowCountry(World w, Country c)
    {
        string? message = null;
        bool success = true;

        while (true)
        {
            Ui.Title($"{c.Name} ({c.Code})", $"{c.Region.Name} · {c.WealthName}{(c.IsHub ? " · 허브공항" : "")}{(c.IsIsland ? " · 섬나라" : "")}");

            var color = Ui.CountryColor(c);
            Ui.Write(" 인구      ");
            Ui.Write($"최초 {Fmt.People(c.InitialPopulation)}  →  ");
            Ui.Write($"생존 {Fmt.People(c.Healthy)}  ", ConsoleColor.Green);
            Ui.Write($"좀비 {Fmt.People(c.Zombies)}  ", ConsoleColor.Red);
            Ui.WriteLine($"사망 {Fmt.People(c.Dead)}", ConsoleColor.DarkGray);

            Ui.Write(" 감염률    ");
            Ui.WriteLine(Ui.Bar(c.ZombieRatio, 30) + " " + Fmt.Pct(c.ZombieRatio), color);
            Ui.Write(" 질서      ");
            Ui.WriteLine(Ui.Bar(c.Order / 100, 30) + $" {c.Order:0}", c.Order < 30 ? ConsoleColor.Red : ConsoleColor.Cyan);

            string state = c.Nuked ? "핵 폐허" : c.Collapsed ? "정부 붕괴" : c.IsInfected ? "감염 확산 중" : "청정";
            Ui.Write(" 상태      ");
            Ui.Write(Ui.Pad(state, 16), color);
            Ui.Write($"국경 {(c.BordersClosed ? "봉쇄" : "개방")}   ");
            Ui.Write($"군 파견 {(c.MilitaryTurns > 0 ? $"{c.MilitaryTurns}주 남음" : "-")}   ");
            Ui.WriteLine($"격리 {(c.QuarantineTurns > 0 ? $"{c.QuarantineTurns}주 남음" : "-")}");
            Ui.Rule();

            Ui.WriteLine($" 조치                                                       보유 예산 {Fmt.Money(w.Funds)}", ConsoleColor.White);
            Option(1, c.BordersClosed ? "국경 개방" : "국경 봉쇄", c.BordersClosed ? 0 : Actions.BorderCost,
                c.BordersClosed ? "봉쇄를 풀어 경제를 회복합니다." : "육로·항공 유입을 크게 줄입니다. 봉쇄국이 많을수록 수입 감소.");
            Option(2, "군 파견 (3주)", Actions.MilitaryCost, "좀비 처치율 대폭 상승, 질서 +10");
            Option(3, "격리 구역 (3주)", Actions.QuarantineCost, "국가 내부 감염 속도 절반");
            Option(4, "긴급 구호", Actions.ReliefCost, "질서 +20, 신뢰도 +2");
            Option(5, "전술 핵 투하", Actions.NukeCost, "좀비 전멸 · 주민 85% 사망 · 신뢰도 급락 · 영구 폐허", ConsoleColor.DarkRed);
            Ui.WriteLine(" [Enter/Esc] 뒤로", ConsoleColor.DarkGray);
            Ui.Rule();
            Ui.ShowResult(message, success);

            var key = Ui.ReadKey();
            if (key.Key is ConsoleKey.Enter or ConsoleKey.Escape) return;

            ActionResult? result = key.KeyChar switch
            {
                '1' => Actions.ToggleBorders(w, c),
                '2' => Actions.DeployMilitary(w, c),
                '3' => Actions.Quarantine(w, c),
                '4' => Actions.Relief(w, c),
                '5' => Ui.Confirm($"정말로 {c.Name}에 핵을 투하합니까? 되돌릴 수 없습니다.") ? Actions.Nuke(w, c) : null,
                _ => null
            };

            if (result != null)
            {
                message = result.Message;
                success = result.Success;
            }
        }
    }

    private static void Option(int key, string label, double cost, string desc, ConsoleColor color = ConsoleColor.White)
    {
        Ui.Write($" [{key}] ", ConsoleColor.Yellow);
        Ui.Write(Ui.Pad(label, 18), color);
        Ui.Write(Ui.Pad(cost > 0 ? Fmt.Money(cost) : "무료", 7), ConsoleColor.Yellow);
        Ui.WriteLine(desc, ConsoleColor.DarkGray);
    }
}
