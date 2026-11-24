namespace ZombieOutbreak.Core;

public sealed record NewsItem(DateTime Date, string Text, ConsoleColor Color);

public enum GameResult { Ongoing, Victory, DefeatHumanity, DefeatTrust, Abandoned }

/// <summary>게임 전체 상태를 한곳에 모은 객체. 시스템(Simulation, Actions...)은 이 객체를 읽고 고칩니다.</summary>
public sealed class World
{
    private double trust = 60;
    private double research;
    private readonly double initialCapacityWeight;

    public World(List<Country> countries, WorldMap map, Difficulty difficulty, int seed)
    {
        Countries = countries;
        Map = map;
        Difficulty = difficulty;
        Rng = new Random(seed);
        Funds = difficulty.StartFunds;
        Infectivity = difficulty.Infectivity;
        InitialPopulation = Countries.Sum(c => c.InitialPopulation);
        initialCapacityWeight = Countries.Sum(c => CapacityWeight(c) * c.InitialPopulation);
    }

    public List<Country> Countries { get; }
    public WorldMap Map { get; }
    public Difficulty Difficulty { get; }
    public Random Rng { get; }
    public double InitialPopulation { get; }
    public Country PatientZero { get; set; } = null!;

    public DateTime StartDate { get; } = DateTime.Today;
    public int Day { get; set; }
    public DateTime Date => StartDate.AddDays(Day);
    public int Week => Day / 7 + 1;

    public double Funds { get; set; }
    public double Infectivity { get; set; }
    public int ResearchLevel { get; set; }
    public bool CureDeployed { get; set; }
    public double WeeklyIncome { get; set; }     // 화면 표시용 (주당 수입)
    public double WeeklyResearch { get; set; }   // 화면 표시용 (주당 연구 진척)

    public double Trust
    {
        get => trust;
        set => trust = Math.Clamp(value, 0, 100);
    }

    public double Research
    {
        get => research;
        set => research = Math.Clamp(value, 0, 100);
    }

    public List<NewsItem> News { get; } = new();

    public double TotalHealthy => Countries.Sum(c => c.Healthy);
    public double TotalZombies => Countries.Sum(c => c.Zombies);
    public double TotalDead => Countries.Sum(c => c.Dead);
    public int InfectedCount => Countries.Count(c => c.IsInfected);
    public int CollapsedCount => Countries.Count(c => c.Collapsed);

    public void AddNews(string text, ConsoleColor color = ConsoleColor.Gray) => News.Add(new NewsItem(Date, text, color));

    /// <summary>
    /// 연구·경제를 떠받치는 '정상 작동 중인 국가'의 비율 (0~1).
    /// 고소득국의 생존자일수록 큰 비중을 차지하고, 붕괴한 나라는 0으로 칩니다.
    /// </summary>
    public double Capacity() =>
        Countries.Where(c => !c.Collapsed).Sum(c => CapacityWeight(c) * c.Healthy) / initialCapacityWeight;

    /// <summary>국경을 봉쇄한 나라가 세계 경제에서 차지하는 비율 (0~1).</summary>
    public double ClosedEconomyShare() =>
        Countries.Where(c => c.BordersClosed).Sum(c => CapacityWeight(c) * c.InitialPopulation) / initialCapacityWeight;

    private static double CapacityWeight(Country c) => c.Wealth switch
    {
        Wealth.High => 1.0,
        Wealth.Mid => 0.3,
        _ => 0.05
    };
}
