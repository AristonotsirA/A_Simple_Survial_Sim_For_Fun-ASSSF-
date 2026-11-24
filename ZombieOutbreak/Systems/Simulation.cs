using ZombieOutbreak.Core;

namespace ZombieOutbreak.Systems;

/// <summary>
/// 하루 동안 세계가 어떻게 변하는지 계산하는 엔진.
/// 모든 수치는 '1주 기준 비율'로 정의하고, 하루치(Dt = 1/7)만큼 나눠서 적용합니다.
/// 순서: ① 국가 내부 감염/전투 → ② 국가 간 전파 → ③ 연구·경제·신뢰도
/// </summary>
public static class Simulation
{
    public const double HumanityDefeatRatio = 0.4;   // 생존자가 세계 인구의 이 비율 아래로 떨어지면 패배
    private const double Dt = 1.0 / 7;               // 하루 = 1/7주
    private const double TurnRate = 0.75;            // 물린 사람 중 좀비가 되는 비율 (나머지는 사망)
    private const double LandPressureCap = 300_000;  // 좀비가 이 정도면 육로 전파 압력 최대
    private const double AirPressureCap = 50_000;    // 좀비가 이 정도면 항공 전파 확률 최대

    public static void StartOutbreak(World w)
    {
        var candidates = w.Countries.Where(c => c.InitialPopulation >= 5_000_000).ToList();
        // 의료 체계가 약한 나라일수록 최초 발병지가 될 확률이 높음
        var zero = WeightedPick(w.Rng, candidates, c => c.Wealth switch
        {
            Wealth.Low => 3,
            Wealth.Mid => 2,
            _ => 1
        });

        w.PatientZero = zero;
        Seed(w, zero, 3000, "최초 발병");
        w.WeeklyIncome = WeeklyIncome(w);
        w.WeeklyResearch = WeeklyResearch(w);
    }

    public static void Step(World w)
    {
        foreach (var c in w.Countries) UpdateCountry(w, c);
        Spread(w);
        UpdateGlobal(w);
        w.Day++;
    }

    public static GameResult CheckResult(World w)
    {
        if (w.TotalHealthy < w.InitialPopulation * HumanityDefeatRatio) return GameResult.DefeatHumanity;
        if (w.Trust <= 0) return GameResult.DefeatTrust;
        if (w.TotalZombies < 1) return GameResult.Victory;
        if (w.CureDeployed && w.TotalZombies < 10_000) return GameResult.Victory;
        return GameResult.Ongoing;
    }

    /// <summary>국가에 좀비를 투입합니다 (최초 발병, 전파, 사건에서 공통 사용).</summary>
    public static void Seed(World w, Country c, double amount, string reason)
    {
        if (c.Nuked || c.Healthy < 1000) return;

        bool first = !c.IsInfected;
        amount = Math.Min(amount, c.Healthy);
        c.Healthy -= amount;
        c.Zombies += amount;

        if (first)
            w.AddNews($"{c.Name} 첫 감염 확인 ({reason})", c.IsHub ? ConsoleColor.Red : ConsoleColor.Yellow);
    }

    /// <summary>좀비 수를 factor배로 바꿉니다 (늘어난 만큼은 생존자에서, 줄어든 만큼은 사망자로).</summary>
    public static void ScaleZombies(Country c, double factor)
    {
        double delta = Math.Min(c.Zombies * (factor - 1), c.Healthy);
        if (delta >= 0) c.Healthy -= delta;
        else c.Dead -= delta;
        c.Zombies += delta;
    }

    /// <summary>가중치 랜덤 선택. 제네릭(T) 덕분에 어떤 타입의 목록에도 쓸 수 있습니다.</summary>
    public static T WeightedPick<T>(Random rng, IReadOnlyList<T> items, Func<T, double> weight)
    {
        double roll = rng.NextDouble() * items.Sum(weight);
        foreach (var item in items)
        {
            roll -= weight(item);
            if (roll <= 0) return item;
        }
        return items[^1];
    }

    // ───────────────────────── ① 국가 내부 ─────────────────────────

    private static void UpdateCountry(World w, Country c)
    {
        if (c.IsInfected) Fight(w, c);

        // 질서: 좀비 비율이 높을수록 목표치가 떨어지고, 매일 조금씩 목표치에 다가감
        double target = c.BaseOrder - c.ZombieRatio * 130 + (c.ContainDays > 0 ? 15 : 0);
        c.Order += (target - c.Order) * 0.06;

        if (!c.Collapsed && c.ZombieRatio > 0.5 && c.Order < 15)
        {
            c.Collapsed = true;
            w.Trust -= c.Wealth switch { Wealth.High => 4, Wealth.Mid => 1.5, _ => 0.3 };
            w.AddNews($"{c.Name} 정부 기능 마비", ConsoleColor.Magenta);
        }
        else if (c.Collapsed && c.ZombieRatio < 0.05)
        {
            c.Collapsed = false;
            c.Order = 30;
            w.AddNews($"{c.Name} 정부가 기능을 되찾았습니다", ConsoleColor.Green);
        }

        if (c.ContainDays > 0) c.ContainDays--;
    }

    /// <summary>좀비가 사람을 무는 양(bites)과 사람이 좀비를 처치하는 양(removed)을 계산합니다.</summary>
    private static void Fight(World w, Country c)
    {
        double z0 = c.Zombies, h0 = c.Healthy;
        bool contained = c.ContainDays > 0;

        // 감염력(주당): 기본값 × 소득 보정 × 질서 보정 (질서 100 → 0.7배, 질서 0 → 1.3배)
        double beta = w.Infectivity * c.Wealth switch { Wealth.High => 0.6, Wealth.Mid => 0.85, _ => 1.0 }
                      * (1.3 - c.Order / 100 * 0.6);
        if (contained) beta *= 0.6;
        if (c.Collapsed) beta *= 1.3;
        if (w.CureDeployed) beta *= 0.1;
        double bites = Math.Min(h0, beta * Dt * z0 * h0 / c.Living);

        // 처치율(주당): 소득별 기본값 × 질서 보정 + 방역 보너스
        double kill = c.Wealth switch { Wealth.High => 0.38, Wealth.Mid => 0.26, _ => 0.16 }
                      * (0.4 + 0.6 * c.Order / 100) + 0.02;
        if (contained) kill += 0.25;
        if (c.Collapsed) kill *= 0.3;
        if (w.CureDeployed) kill += 0.3;

        double cureRate = w.CureDeployed ? (c.Collapsed ? 0.1 : 0.3) : 0;
        double removed = z0 * Math.Min(0.98, kill + cureRate) * Dt;
        double cured = removed * cureRate / (kill + cureRate);

        c.Healthy += cured - bites;
        c.Zombies += bites * TurnRate - removed;
        c.Dead += bites * (1 - TurnRate) + removed - cured;

        if (c.Zombies < 1)
        {
            c.Dead += c.Zombies;
            c.Zombies = 0;
            if (z0 >= 100) w.AddNews($"{c.Name}에서 좀비가 모두 사라졌습니다", ConsoleColor.Green);
        }
    }

    // ───────────────────────── ② 국가 간 전파 ─────────────────────────

    private static void Spread(World w)
    {
        // ToList()로 미리 복사: 오늘 새로 감염된 나라가 같은 날 또 퍼뜨리지 않게 함
        var sources = w.Countries.Where(c => c.IsInfected).ToList();

        foreach (var from in sources)
        {
            double pressure = Math.Min(1, from.Zombies / LandPressureCap) * (from.BordersClosed ? 0.5 : 1);
            foreach (var to in from.LandNeighbors) TryCross(w, from, to, 0.35 * pressure, "에서 유입");
            foreach (var to in from.SeaNeighbors) TryCross(w, from, to, 0.05 * pressure, "에서 해상 유입");
            TryAir(w, from);
        }
    }

    private static void TryCross(World w, Country from, Country to, double weeklyChance, string how)
    {
        if (to.IsInfected || weeklyChance <= 0) return;
        if (to.BordersClosed) weeklyChance *= 0.2;

        if (w.Rng.NextDouble() < weeklyChance * Dt)
            Seed(w, to, 50 + w.Rng.Next(250), from.Name + how);
    }

    private static void TryAir(World w, Country from)
    {
        if (from.BordersClosed) return;

        double weeklyChance = (from.IsHub ? 0.9 : 0.15) * Math.Min(1, from.Zombies / AirPressureCap) * (from.Collapsed ? 0.2 : 1);
        if (w.Rng.NextDouble() >= weeklyChance * Dt) return;

        var candidates = w.Countries.Where(c => c != from && !c.Nuked).ToList();
        // 목적지 가중치: 허브공항·부유한 나라일수록, 인구가 많을수록 항공편이 많음
        var to = WeightedPick(w.Rng, candidates, c => (c.IsHub ? 6 : c.Wealth switch
        {
            Wealth.High => 2,
            Wealth.Mid => 1,
            _ => 0.6
        }) * Math.Sqrt(c.InitialPopulation / 10_000_000));
        if (to.IsInfected) return;

        if (to.BordersClosed && w.Rng.NextDouble() < 0.9)
        {
            w.AddNews($"{to.Name} 공항 봉쇄로 {from.Name}발 감염 의심 승객 차단", ConsoleColor.Cyan);
            return;
        }

        Seed(w, to, 20 + w.Rng.Next(80), $"{from.Name}발 항공편");
    }

    // ───────────────────────── ③ 연구 · 경제 · 신뢰도 ─────────────────────────

    private static void UpdateGlobal(World w)
    {
        w.WeeklyResearch = WeeklyResearch(w);
        w.Research += w.WeeklyResearch * Dt;

        if (!w.CureDeployed && w.Research >= 100)
        {
            w.CureDeployed = true;
            w.Trust += 15;
            w.AddNews("치료제 완성! 전 세계 배포를 시작합니다", ConsoleColor.Green);
        }

        w.WeeklyIncome = WeeklyIncome(w);
        w.Funds += w.WeeklyIncome * Dt;

        // 좀비가 많을수록 사령부에 대한 신뢰가 깎이고, 평소에는 조금씩 회복
        w.Trust += (0.4 - w.TotalZombies / w.InitialPopulation * 5) * Dt;
    }

    /// <summary>주당 연구 진척(%) = 기본 속도 × 투자 단계 보너스 × 연구 기반 가동률.</summary>
    private static double WeeklyResearch(World w) => w.CureDeployed ? 0 :
        Actions.BaseResearchRate * w.Difficulty.ResearchMult * (1 + Actions.ResearchBonusPerLevel * w.ResearchLevel) * w.Capacity();

    /// <summary>주당 수입 = 경제 가동률 × 신뢰도 보정 × 국경 봉쇄 손실.</summary>
    private static double WeeklyIncome(World w) =>
        22 * w.Difficulty.IncomeMult * w.Capacity() * (0.5 + w.Trust / 100) * (1 - 0.5 * w.ClosedEconomyShare());
}
