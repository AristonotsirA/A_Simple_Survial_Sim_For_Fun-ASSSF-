using System.Text;

namespace ZombieOutbreak.UI;

/// <summary>
/// 화면 버퍼(더블 버퍼링).
/// 매 프레임 Console.Clear()로 지우고 다시 그리면 화면이 깜빡입니다.
/// 대신 메모리 속 '가상 화면'에 먼저 그린 뒤, 직전 프레임과 달라진 줄만 콘솔에 덮어씁니다.
/// </summary>
public sealed class ScreenBuffer
{
    public const int Width = 119;   // 120칸 창에서 마지막 칸에 쓰면 줄바꿈이 일어나므로 1칸 여유
    public const int Height = 36;

    private const char WideTail = '\0';   // 한글처럼 2칸짜리 글자의 '뒤쪽 칸' 표시

    // readonly record struct: 값만 담는 가벼운 구조체. == 비교가 자동으로 '내용 비교'가 됩니다.
    private readonly record struct Cell(char Ch, ConsoleColor Fg, ConsoleColor Bg);

    private readonly Cell[,] cells = new Cell[Height, Width];
    private readonly Cell[,] shown = new Cell[Height, Width];
    private bool redrawAll = true;

    public void Clear()
    {
        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
                cells[r, c] = new Cell(' ', ConsoleColor.Gray, ConsoleColor.Black);
    }

    public void Put(int row, int col, char ch, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width) return;

        // 2칸 글자를 반만 덮어쓰면 줄이 틀어지므로, 남은 반쪽을 공백으로 정리
        if (cells[row, col].Ch == WideTail && col > 0) cells[row, col - 1] = cells[row, col - 1] with { Ch = ' ' };
        if (col + 1 < Width && cells[row, col + 1].Ch == WideTail) cells[row, col + 1] = cells[row, col + 1] with { Ch = ' ' };

        cells[row, col] = new Cell(ch, fg, bg);
    }

    /// <summary>문자열을 쓰고, 다음에 쓸 열 위치를 반환합니다.</summary>
    public int Text(int row, int col, string text, ConsoleColor fg, ConsoleColor bg = ConsoleColor.Black)
    {
        foreach (char ch in text)
        {
            bool wide = Ui.IsWide(ch);
            if (col + (wide ? 2 : 1) > Width) break;
            Put(row, col, ch, fg, bg);
            if (wide) cells[row, col + 1] = new Cell(WideTail, fg, bg);
            col += wide ? 2 : 1;
        }
        return col;
    }

    /// <summary>창 크기가 바뀌는 등 화면이 망가졌을 때, 다음 Flush에서 전부 다시 그리게 합니다.</summary>
    public void Invalidate() => redrawAll = true;

    public void Flush()
    {
        var run = new StringBuilder();
        for (int r = 0; r < Height; r++)
        {
            if (!redrawAll && RowUnchanged(r)) continue;

            Console.SetCursorPosition(0, r);
            var fg = cells[r, 0].Fg;
            var bg = cells[r, 0].Bg;

            for (int c = 0; c < Width; c++)
            {
                var cell = cells[r, c];
                shown[r, c] = cell;
                if (cell.Ch == WideTail) continue;

                // 색이 바뀌는 지점에서만 출력 → Console 호출 횟수를 최소화
                if (cell.Fg != fg || cell.Bg != bg)
                {
                    Emit(run, fg, bg);
                    fg = cell.Fg;
                    bg = cell.Bg;
                }
                run.Append(cell.Ch);
            }
            Emit(run, fg, bg);
        }
        Console.ResetColor();
        redrawAll = false;
    }

    private bool RowUnchanged(int r)
    {
        for (int c = 0; c < Width; c++)
            if (cells[r, c] != shown[r, c]) return false;
        return true;
    }

    private static void Emit(StringBuilder run, ConsoleColor fg, ConsoleColor bg)
    {
        if (run.Length == 0) return;
        Console.ForegroundColor = fg;
        Console.BackgroundColor = bg;
        Console.Write(run.ToString());
        run.Clear();
    }
}
