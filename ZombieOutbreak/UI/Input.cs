using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

/// <summary>키 입력 하나를 처리합니다. 게임을 포기하면 false를 반환합니다.</summary>
public static class Input
{
    public static bool Handle(World w, ViewState v, ConsoleKeyInfo key, long now)
    {
        // 1) 사건 카드가 떠 있으면 숫자 키만 받음
        if (v.Event is { } drawn)
        {
            int index = key.KeyChar - '1';   // '1' → 0, '2' → 1 ...
            if (index < 0 || index >= drawn.Event.Choices.Length) return true;

            if (EventDeck.Choose(w, drawn, index)) v.Event = null;
            else v.EventError = "예산이 부족해 이 선택지를 고를 수 없습니다.";
            return true;
        }

        // 2) 덮개 화면(도움말·메뉴·핵 확인) 처리
        switch (v.Overlay)
        {
            case Overlay.Help:
                v.Overlay = Overlay.None;
                return true;

            case Overlay.Menu:
                if (Ui.IsLetter(key, 'q')) return false;
                if (key.Key == ConsoleKey.Escape) v.Overlay = Overlay.None;
                return true;

            case Overlay.ConfirmNuke:
                v.Overlay = Overlay.None;
                if (Ui.IsLetter(key, 'y') && v.Selected != null) Show(v, Actions.Nuke(w, v.Selected), now);
                return true;
        }

        // 3) 일반 조작
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow: MoveCursor(w, v, 0, -2); return true;
            case ConsoleKey.RightArrow: MoveCursor(w, v, 0, 2); return true;
            case ConsoleKey.UpArrow: MoveCursor(w, v, -1, 0); return true;
            case ConsoleKey.DownArrow: MoveCursor(w, v, 1, 0); return true;
            case ConsoleKey.Tab: JumpToNextInfected(w, v); return true;
            case ConsoleKey.Spacebar: v.Paused = !v.Paused; return true;
            case ConsoleKey.Escape: v.Overlay = Overlay.Menu; return true;
            case ConsoleKey.OemPlus or ConsoleKey.Add: v.Speed = Math.Min(ViewState.MaxSpeed, v.Speed + 1); return true;
            case ConsoleKey.OemMinus or ConsoleKey.Subtract: v.Speed = Math.Max(1, v.Speed - 1); return true;
        }

        if (Ui.IsLetter(key, 'h')) v.Overlay = Overlay.Help;
        else if (Ui.IsLetter(key, 'r')) Show(v, Actions.UpgradeResearch(w), now);
        else if (Ui.IsLetter(key, 'b')) WithSelected(v, now, c => Actions.ToggleBorders(w, c));
        else if (Ui.IsLetter(key, 'm')) WithSelected(v, now, c => Actions.Contain(w, c));
        else if (Ui.IsLetter(key, 'n') && v.Selected is { Nuked: false }) v.Overlay = Overlay.ConfirmNuke;

        return true;
    }

    private static void WithSelected(ViewState v, long now, Func<Country, ActionResult> action)
    {
        if (v.Selected == null) v.ShowMessage("먼저 방향키로 나라를 선택하세요.", false, now);
        else Show(v, action(v.Selected), now);
    }

    private static void Show(ViewState v, ActionResult result, long now) => v.ShowMessage(result.Message, result.Success, now);

    /// <summary>커서를 옮기고, 커서 아래에 나라가 있으면 그 나라를 선택합니다. (바다 위에선 이전 선택 유지)</summary>
    private static void MoveCursor(World w, ViewState v, int dRow, int dCol)
    {
        v.CursorRow = Math.Clamp(v.CursorRow + dRow, 0, w.Map.Height - 1);
        v.CursorCol = (v.CursorCol + dCol + w.Map.Width) % w.Map.Width;   // 좌우 끝은 이어짐
        if (w.Map.At(v.CursorRow, v.CursorCol) is { } country) v.Selected = country;
    }

    /// <summary>좀비가 많은 순서로 감염국을 하나씩 돌아가며 선택합니다.</summary>
    private static void JumpToNextInfected(World w, ViewState v)
    {
        var ranking = w.Countries.Where(c => c.IsInfected).OrderByDescending(c => c.Zombies).ToList();
        if (ranking.Count == 0) return;

        int current = v.Selected == null ? -1 : ranking.IndexOf(v.Selected);
        v.MoveCursorTo(ranking[(current + 1) % ranking.Count]);
    }
}
