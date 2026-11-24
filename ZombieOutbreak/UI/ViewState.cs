using ZombieOutbreak.Core;
using ZombieOutbreak.Systems;

namespace ZombieOutbreak.UI;

public enum Overlay { None, Help, Menu, ConfirmNuke }

/// <summary>
/// 화면·조작 상태 (커서 위치, 일시정지, 속도 등).
/// 게임 규칙 상태(World)와 분리해 두면, 규칙 엔진은 화면을 전혀 몰라도 됩니다.
/// </summary>
public sealed class ViewState
{
    /// <summary>속도 단계별 하루 길이(밀리초). 인덱스 = 속도 단계.</summary>
    public static readonly int[] DayLengthMs = [0, 800, 400, 150];
    public const int MaxSpeed = 3;

    public int CursorRow { get; set; }
    public int CursorCol { get; set; }
    public Country? Selected { get; set; }

    public bool Paused { get; set; }
    public int Speed { get; set; } = 2;
    public Overlay Overlay { get; set; }
    public DrawnEvent? Event { get; set; }
    public string? EventError { get; set; }
    public bool Blink { get; set; }

    public string? Message { get; private set; }
    public ConsoleColor MessageColor { get; private set; }
    public long MessageUntilMs { get; private set; }

    /// <summary>시간이 흘러가는 상태인가? (일시정지, 사건 카드, 메뉴가 없을 때만)</summary>
    public bool IsRunning => !Paused && Event == null && Overlay == Overlay.None;

    public void ShowMessage(string text, bool success, long nowMs)
    {
        Message = text;
        MessageColor = success ? ConsoleColor.Green : ConsoleColor.Red;
        MessageUntilMs = nowMs + 3000;
    }

    public void MoveCursorTo(Country c)
    {
        CursorRow = c.CenterRow;
        CursorCol = c.CenterCol;
        Selected = c;
    }
}
