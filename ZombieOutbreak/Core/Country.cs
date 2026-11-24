namespace ZombieOutbreak.Core;

public enum Wealth { Low, Mid, High }

/// <summary>국가 하나의 상태. 인구는 생존자(Healthy) / 좀비(Zombies) / 사망자(Dead) 세 칸으로 나뉩니다.</summary>
public sealed class Country
{
    private double order;

    public Country(string code, string name, Region region, double population, Wealth wealth, bool isHub, bool isIsland)
    {
        Code = code;
        Name = name;
        Region = region;
        InitialPopulation = population;
        Wealth = wealth;
        IsHub = isHub;
        IsIsland = isIsland;
        Healthy = population;
        Order = BaseOrder;
    }

    // ── 변하지 않는 기본 정보 ──
    public string Code { get; }
    public string Name { get; }
    public Region Region { get; }
    public double InitialPopulation { get; }
    public Wealth Wealth { get; }
    public bool IsHub { get; }      // 국제 허브 공항 보유 → 항공 전파가 잦음
    public bool IsIsland { get; }   // 섬나라 → 육로 전파에 강함

    // ── 매 턴 변하는 상태 ──
    public double Healthy { get; set; }
    public double Zombies { get; set; }
    public double Dead { get; set; }

    /// <summary>치안·통제력 (0~100). 낮을수록 감염이 빨라지고 좀비 처치가 어려워집니다.</summary>
    public double Order
    {
        get => order;
        set => order = Math.Clamp(value, 0, 100);
    }

    public bool BordersClosed { get; set; }
    public bool Collapsed { get; set; }
    public bool Nuked { get; set; }
    public int MilitaryTurns { get; set; }
    public int QuarantineTurns { get; set; }

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

    /// <summary>국가 목록에 표시할 상태 꼬리표 (예: "봉쇄 군3 격리2").</summary>
    public string StatusTags
    {
        get
        {
            if (Nuked) return "핵 폐허";
            var tags = new List<string>();
            if (Collapsed) tags.Add("붕괴");
            if (BordersClosed) tags.Add("봉쇄");
            if (MilitaryTurns > 0) tags.Add($"군{MilitaryTurns}");
            if (QuarantineTurns > 0) tags.Add($"격리{QuarantineTurns}");
            return string.Join(' ', tags);
        }
    }
}
