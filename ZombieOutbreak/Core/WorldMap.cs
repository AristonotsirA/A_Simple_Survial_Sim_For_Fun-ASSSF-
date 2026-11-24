namespace ZombieOutbreak.Core;

/// <summary>
/// 세계지도 격자. 칸마다 '어느 나라 땅인지'를 저장하고,
/// 그 격자로부터 국경 이웃·바다 이웃·지도 중심점을 계산합니다.
/// </summary>
public sealed class WorldMap
{
    private const int SeaRangeRows = 2;   // 바다 이웃으로 칠 거리 (세로 칸)
    private const int SeaRangeCols = 4;   // 바다 이웃으로 칠 거리 (가로 칸)

    private readonly Country?[,] cells;
    private readonly bool[,] territory;

    public WorldMap(string[] rows, string[] codes, Dictionary<string, Country> byCode)
    {
        Height = rows.Length;
        Width = rows[0].Length;
        cells = new Country?[Height, Width];
        territory = new bool[Height, Width];

        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                char ch = rows[r][c];
                if (ch == '#') territory[r, c] = true;
                else if (ch != ' ') cells[r, c] = byCode[codes[ch - 0x100]];
            }

        ComputeCenters();
        ComputeNeighbors();
        ComputeShades();
    }

    public int Width { get; }
    public int Height { get; }

    public Country? At(int row, int col) => cells[row, col];
    public bool IsTerritory(int row, int col) => territory[row, col];

    /// <summary>각 나라의 칸 평균 위치에 가장 가까운 '자기 땅' 칸을 중심점으로 정합니다.</summary>
    private void ComputeCenters()
    {
        var groups = new Dictionary<Country, List<(int R, int C)>>();
        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
                if (cells[r, c] is { } country)
                {
                    if (!groups.TryGetValue(country, out var list)) groups[country] = list = new();
                    list.Add((r, c));
                }

        foreach (var (country, list) in groups)
        {
            double avgR = list.Average(p => p.R), avgC = list.Average(p => p.C);
            var center = list.MinBy(p => (p.R - avgR) * (p.R - avgR) + (p.C - avgC) * (p.C - avgC) / 4);
            country.CenterRow = center.R;
            country.CenterCol = center.C;
        }
    }

    /// <summary>칸이 맞닿으면 국경 이웃, 바다를 사이에 두고 가까우면 바다 이웃.</summary>
    private void ComputeNeighbors()
    {
        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
            {
                if (cells[r, c] is not { } a) continue;

                for (int dr = -SeaRangeRows; dr <= SeaRangeRows; dr++)
                    for (int dc = -SeaRangeCols; dc <= SeaRangeCols; dc++)
                    {
                        int rr = r + dr, cc = (c + dc + Width) % Width;   // 좌우 끝은 이어져 있음 (태평양)
                        if (rr < 0 || rr >= Height || cells[rr, cc] is not { } b || b == a) continue;

                        bool touching = Math.Abs(dr) <= 1 && Math.Abs(dc) <= 1;
                        var list = touching ? a.LandNeighbors : a.SeaNeighbors;
                        if (!list.Contains(b)) list.Add(b);
                    }
            }

        // 국경 이웃이면서 바다 이웃으로도 잡힌 나라는 국경 이웃으로만 남김
        foreach (var country in cells.Cast<Country?>().OfType<Country>().Distinct())
            country.SeaNeighbors.RemoveAll(country.LandNeighbors.Contains);
    }

    /// <summary>국경 이웃끼리 서로 다른 명암을 갖도록 순서대로 배정합니다 (탐욕 색칠).</summary>
    private void ComputeShades()
    {
        var countries = cells.Cast<Country?>().OfType<Country>().Distinct().OrderByDescending(c => c.LandNeighbors.Count);
        foreach (var country in countries)
        {
            int zeros = country.LandNeighbors.Count(n => n.Shade == 0);
            int ones = country.LandNeighbors.Count(n => n.Shade == 1);
            country.Shade = zeros > ones ? 1 : 0;
        }
    }
}
