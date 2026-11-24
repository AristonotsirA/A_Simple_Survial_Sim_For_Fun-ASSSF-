namespace ZombieOutbreak.Core;

public enum Wealth { Low, Mid, High }

/// <summary>국가 하나의 상태. 인구는 생존자(Healthy) / 좀비(Zombies) / 사망자(Dead) 세 칸으로 나뉩니다.</summary>
public sealed class Country
{
    private double order;

    public Country(string code, string name, double population, Wealth wealth, bool isHub)
    {
        Code = code;
        Name = name;
        InitialPopulation = population;
        Wealth = wealth;
        IsHub = isHub;
        Healthy = population;
        Order = BaseOrder;
    }

    // ── 변하지 않는 기본 정보 ──
    public string Code { get; }
    public string Name { get; }
    public double InitialPopulation { get; }
    public Wealth Wealth { get; }
    public bool IsHub { get; }   // 국제 허브 공항 보유 → 항공 전파가 잦음

    // ── 지도에서 계산되는 정보 (WorldMap이 채움) ──
    public List<Country> LandNeighbors { get; } = new();  // 국경을 맞댄 나라
    public List<Country> SeaNeighbors { get; } = new();   // 가까운 바다 건너 나라
    public int CenterRow { get; set; }
    public int CenterCol { get; set; }
    public int Shade { get; set; }   // 청정 상태일 때 이웃끼리 다른 초록색을 쓰기 위한 번호 (0/1)

    // ── 매일 변하는 상태 ──
    public double Healthy { get; set; }
    public double Zombies { get; set; }
    public double Dead { get; set; }

    /// <summary>치안·통제력 (0~100). 화면에는 보이지 않고 내부 계산에만 쓰입니다.</summary>
    public double Order
    {
        get => order;
        set => order = Math.Clamp(value, 0, 100);
    }

    public bool BordersClosed { get; set; }
    public bool Collapsed { get; set; }
    public bool Nuked { get; set; }
    public int ContainDays { get; set; }   // 방역 작전 남은 일수

    // ── 계산 속성 ──
    public double Living => Healthy + Zombies;
    public double ZombieRatio => Living < 1 ? 0 : Zombies / Living;
    public bool IsInfected => Zombies >= 1;

    public double BaseOrder => Wealth switch
    {
        Wealth.High => 85,
        Wealth.Mid => 70,
        _ => 55
    };

    public string WealthName => Wealth switch
    {
        Wealth.High => "고소득국",
        Wealth.Mid => "중소득국",
        _ => "저소득국"
    };
}
