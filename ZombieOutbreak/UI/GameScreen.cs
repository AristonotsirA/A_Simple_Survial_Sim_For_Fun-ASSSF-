using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>
/// 메인 게임 화면 한 장을 버퍼에 그립니다.
///  0행: 제목·날짜·진행 상태      1행: 전 세계 지표
///  2~27행: 세계지도              28행: 구분선
///  29행: 선택한 나라 정보        30행: 조치 단축키
///  31행: 구분선   32~34행: 소식  35행: 도움말/알림
/// </summary>
public static class GameScreen
{
    private const int MapTop = 2;
    private const int MapLeft = 1;
    private const int CountryRow = 29;
    private const int ActionRow = 30;
    private const int NewsTop = 32;
    private const int NewsLines = 3;
    private const int HelpRow = 35;
    private const int CardWidth = 74;

    public static void Render(World w, ViewState v, ScreenBuffer b, long nowMs)
    {
        b.Clear();
        DrawHeader(w, v, b);
        DrawMap(w, v, b);
        Rule(b, 28);
        DrawCountry(v, b);
        DrawActions(w, v, b);
        Rule(b, 31);
        DrawNews(w, b);
        DrawHelp(v, b, nowMs);

        if (v.Event != null) DrawEvent(w, v, b);
        else if (v.Overlay == Overlay.Help) DrawCard(b, "플레이 방법", HelpText.Lines, "아무 키나 누르면 닫힙니다");
        else if (v.Overlay == Overlay.Menu)
            DrawCard(b, "일시정지 메뉴", [("", ConsoleColor.Gray), ("[Esc]  게임으로 돌아가기", ConsoleColor.White), ("[Q]    작전 포기 (타이틀로)", ConsoleColor.Red), ("", ConsoleColor.Gray)], "");
    }

    private static void Rule(ScreenBuffer b, int row) => b.Text(row, 1, new string('─', ScreenBuffer.Width - 2), ConsoleColor.DarkGray);

    // ───────────────────────── 상단 지표 ─────────────────────────

    private static void DrawHeader(World w, ViewState v, ScreenBuffer b)
    {
        int col = b.Text(0, 1, "LAST LINE", ConsoleColor.Red);
        b.Text(0, col + 3, $"{w.Date:yyyy년 MM월 dd일}  ({w.Week}주차)", ConsoleColor.White);

        string status = v.Paused ? "일시정지  [Space] 재개" : $"진행 중  속도 {new string('>', v.Speed)}{new string('.', ViewState.MaxSpeed - v.Speed)}";
        b.Text(0, ScreenBuffer.Width - 1 - Ui.Width(status), status, v.Paused ? ConsoleColor.Yellow : ConsoleColor.Green);

        col = b.Text(1, 1, "예산 ", ConsoleColor.Gray);
        col = b.Text(1, col, Fmt.Money(w.Funds), ConsoleColor.Yellow);
        col = b.Text(1, col, $" (+{w.WeeklyIncome:0}/주)", ConsoleColor.DarkGray);

        col = b.Text(1, col + 3, "신뢰도 ", ConsoleColor.Gray);
        col = b.Text(1, col, Ui.Bar(w.Trust / 100, 10) + $" {w.Trust:0}", Ui.TrustColor(w.Trust));

        col = b.Text(1, col + 3, "치료제 ", ConsoleColor.Gray);
        col = w.CureDeployed
            ? b.Text(1, col, "완성! 배포 중", ConsoleColor.Green)
            : b.Text(1, col, Ui.Bar(w.Research / 100, 10) + $" {w.Research:0.0}%", ConsoleColor.Cyan);

        double survival = w.TotalHealthy / w.InitialPopulation;
        col = b.Text(1, col + 3, "생존 ", ConsoleColor.Gray);
        col = b.Text(1, col, $"{survival * 100:0.0}%", survival > 0.7 ? ConsoleColor.Green : ConsoleColor.Red);
        col = b.Text(1, col + 3, "좀비 ", ConsoleColor.Gray);
        col = b.Text(1, col, Fmt.People(w.TotalZombies), ConsoleColor.Red);
        col = b.Text(1, col + 3, "감염국 ", ConsoleColor.Gray);
        b.Text(1, col, $"{w.InfectedCount}/{w.Countries.Count}", ConsoleColor.Yellow);
    }

    // ───────────────────────── 세계지도 ─────────────────────────

    private static void DrawMap(World w, ViewState v, ScreenBuffer b)
    {
        var map = w.Map;
        for (int r = 0; r < map.Height; r++)
            for (int c = 0; c < map.Width; c++)
            {
                int row = MapTop + r, col = MapLeft + c;

                if (r == v.CursorRow && c == v.CursorCol)
                {
                    b.Put(row, col, '+', ConsoleColor.Black, v.Blink ? ConsoleColor.Yellow : ConsoleColor.White);
                    continue;
                }

                if (map.At(r, c) is { } country)
                {
                    // 선택한 나라는 흰색으로 깜빡여서 국경 모양이 보이게 함
                    var color = country == v.Selected && v.Blink ? ConsoleColor.White : Ui.CountryColor(country);
                    b.Put(row, col, Ui.CountryGlyph(country), color);
                }
                else if (map.IsTerritory(r, c))
                {
                    b.Put(row, col, '░', ConsoleColor.DarkGray);
                }
                else if ((r * 7 + c * 3) % 19 == 0)
                {
                    b.Put(row, col, '~', ConsoleColor.DarkBlue);   // 바다 물결 무늬
                }
            }
    }

    // ───────────────────────── 선택한 나라 ─────────────────────────

    private static void DrawCountry(ViewState v, ScreenBuffer b)
    {
        if (v.Selected is not { } c)
        {
            b.Text(CountryRow, 1, "방향키로 지도 위의 나라를 고르세요.   [Tab] 감염이 가장 심한 나라로 이동", ConsoleColor.DarkGray);
            return;
        }

        var color = Ui.CountryColor(c);
        int col = b.Text(CountryRow, 1, Ui.Pad(c.Name, 18), ConsoleColor.White);
        col = b.Text(CountryRow, col, Ui.Pad(c.WealthName + (c.IsHub ? " · 허브공항" : ""), 23), ConsoleColor.DarkGray);
        col = b.Text(CountryRow, col, "감염 ", ConsoleColor.Gray);
        col = b.Text(CountryRow, col, Ui.Bar(c.ZombieRatio, 12) + " " + Ui.Pad(Fmt.Pct(c.ZombieRatio), 7), c.IsInfected ? color : ConsoleColor.DarkGray);
        col = b.Text(CountryRow, col, $"생존 {Ui.Pad(Fmt.People(c.Healthy), 9)}", ConsoleColor.Green);
        col = b.Text(CountryRow, col, $"좀비 {Ui.Pad(Fmt.People(c.Zombies), 9)}", ConsoleColor.Red);

        if (c.Nuked) b.Text(CountryRow, col, "[핵 폐허]", ConsoleColor.DarkGray);
        if (c.Collapsed) col = b.Text(CountryRow, col, "[붕괴] ", ConsoleColor.DarkMagenta);
        if (c.BordersClosed) col = b.Text(CountryRow, col, "[봉쇄] ", ConsoleColor.Cyan);
        if (c.ContainDays > 0) b.Text(CountryRow, col, $"[방역 {c.ContainDays}일]", ConsoleColor.Cyan);
    }

    private static void DrawActions(World w, ViewState v, ScreenBuffer b)
    {
        if (v.Overlay == Overlay.ConfirmNuke && v.Selected != null)
        {
            b.Text(ActionRow, 1, $"정말 {v.Selected.Name}에 핵을 투하합니까? 주민 85%가 사망하고 신뢰도가 크게 떨어집니다.   [Y] 투하   [다른 키] 취소", ConsoleColor.Red);
            return;
        }

        var c = v.Selected;
        bool hasTarget = c != null && !c.Nuked;
        int col = 1;

        if (c is { BordersClosed: true })   // 속성 패턴: c가 null이 아니고 BordersClosed가 true인지 한 번에 검사
            col = Action(b, col, "B", "국경 개방", "무료", hasTarget);
        else
            col = Action(b, col, "B", "국경 봉쇄", Fmt.Money(Actions.BorderCost), hasTarget && w.Funds >= Actions.BorderCost);

        col = Action(b, col, "M", "방역 작전 4주", Fmt.Money(Actions.ContainCost), hasTarget && c!.ContainDays == 0 && w.Funds >= Actions.ContainCost);
        col = Action(b, col, "N", "전술 핵", Fmt.Money(Actions.NukeCost), hasTarget && w.Funds >= Actions.NukeCost);

        col = b.Text(ActionRow, col + 2, "│  ", ConsoleColor.DarkGray);
        if (w.CureDeployed || w.ResearchLevel >= Actions.MaxResearchLevel)
            b.Text(ActionRow, col, $"치료제 연구 투자 최대 ({w.ResearchLevel}단계)", ConsoleColor.DarkGray);
        else
            Action(b, col, "R", $"연구 투자 {w.ResearchLevel}→{w.ResearchLevel + 1}단계", Fmt.Money(Actions.ResearchUpgradeCost(w)),
                w.Funds >= Actions.ResearchUpgradeCost(w));
    }

    private static int Action(ScreenBuffer b, int col, string key, string label, string cost, bool enabled)
    {
        col = b.Text(ActionRow, col, $"[{key}] ", enabled ? ConsoleColor.Yellow : ConsoleColor.DarkGray);
        col = b.Text(ActionRow, col, label + " ", enabled ? ConsoleColor.White : ConsoleColor.DarkGray);
        return b.Text(ActionRow, col, cost, enabled ? ConsoleColor.Yellow : ConsoleColor.DarkGray) + 4;
    }

    // ───────────────────────── 소식 · 도움말 ─────────────────────────

    private static void DrawNews(World w, ScreenBuffer b)
    {
        var recent = w.News.TakeLast(NewsLines).ToList();
        for (int i = 0; i < recent.Count; i++)
        {
            int col = b.Text(NewsTop + i, 1, $"{recent[i].Date:MM/dd}  ", ConsoleColor.DarkGray);
            b.Text(NewsTop + i, col, recent[i].Text, recent[i].Color);
        }
    }

    private static void DrawHelp(ViewState v, ScreenBuffer b, long nowMs)
    {
        if (v.Message != null && nowMs < v.MessageUntilMs)
            b.Text(HelpRow, 1, "▶ " + v.Message, v.MessageColor);
        else
            b.Text(HelpRow, 1, "[방향키] 나라 선택   [Tab] 다음 감염국   [Space] 일시정지   [+/-] 속도   [H] 도움말   [Esc] 메뉴", ConsoleColor.DarkGray);
    }

    // ───────────────────────── 카드 (사건 · 도움말 · 메뉴) ─────────────────────────

    private static void DrawEvent(World w, ViewState v, ScreenBuffer b)
    {
        var drawn = v.Event!;
        var lines = new List<(string, ConsoleColor)> { ("", ConsoleColor.Gray), (drawn.Event.Title, ConsoleColor.Yellow) };
        if (drawn.Target != null) lines.Add(($"대상: {drawn.Target.Name} (감염 {Fmt.Pct(drawn.Target.ZombieRatio)})", ConsoleColor.DarkGray));
        lines.Add(("", ConsoleColor.Gray));
        lines.AddRange(Ui.Wrap(drawn.Event.Body(drawn.Target), CardWidth - 6).Select(t => (t, ConsoleColor.Gray)));
        lines.Add(("", ConsoleColor.Gray));

        for (int i = 0; i < drawn.Event.Choices.Length; i++)
        {
            var choice = drawn.Event.Choices[i];
            bool ok = w.Funds >= choice.Cost;
            lines.Add(($"[{i + 1}] {choice.Label}", ok ? ConsoleColor.White : ConsoleColor.DarkGray));
            lines.Add(($"    {choice.Effect}", ok ? ConsoleColor.Cyan : ConsoleColor.DarkGray));
        }
        lines.Add(("", ConsoleColor.Gray));
        if (v.EventError != null) lines.Add((v.EventError, ConsoleColor.Red));

        DrawCard(b, $"긴급 보고 · {w.Date:MM월 dd일}", lines, $"숫자 키로 선택 · 예산 {Fmt.Money(w.Funds)}");
    }

    /// <summary>지도 위에 테두리 있는 카드를 그립니다. 카드 영역은 검은 배경으로 덮습니다.</summary>
    private static void DrawCard(ScreenBuffer b, string title, IReadOnlyList<(string Text, ConsoleColor Color)> lines, string footer)
    {
        int left = (ScreenBuffer.Width - CardWidth) / 2;
        int top = MapTop + Math.Max(1, (26 - lines.Count - 2) / 2);
        const ConsoleColor border = ConsoleColor.DarkYellow;

        b.Text(top, left, "┌─ " + Ui.Pad(title + " ", CardWidth - 4).Replace(' ', '─') + "┐", border);
        b.Text(top, left + 3, title + " ", ConsoleColor.White);

        for (int i = 0; i < lines.Count; i++)
        {
            int row = top + 1 + i;
            b.Text(row, left, "│ ", border);
            b.Text(row, left + 2, Ui.Pad(" " + lines[i].Text, CardWidth - 4), lines[i].Color);
            b.Text(row, left + CardWidth - 2, " │", border);
        }

        int bottom = top + 1 + lines.Count;
        b.Text(bottom, left, "└" + new string('─', CardWidth - 2) + "┘", border);
        if (footer.Length > 0) b.Text(bottom, left + 3, $" {footer} ", ConsoleColor.DarkGray);
    }
}
