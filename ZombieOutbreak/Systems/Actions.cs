using ZombieOutbreak.Core;

namespace ZombieOutbreak.Systems;

public sealed record ActionResult(bool Success, string Message);

/// <summary>플레이어 조치 4가지. 비용 확인 → 효과 적용 → 결과 메시지 반환의 패턴을 따릅니다.</summary>
public static class Actions
{
    public const double BorderCost = 5;
    public const double ContainCost = 20;
    public const int ContainDuration = 28;   // 방역 작전 기간 (일)
    public const double NukeCost = 30;

    public const int MaxResearchLevel = 5;
    public const double BaseResearchRate = 1.1;       // 투자 0단계, 연구 기반 100%일 때 주당 진척(%)
    public const double ResearchBonusPerLevel = 0.15; // 투자 1단계마다 +15%

    /// <summary>level단계 투자에 드는 비용 (1단계 30, 2단계 55, ...)</summary>
    public static double ResearchLevelCost(int level) => 30 + (level - 1) * 25;
    public static double ResearchUpgradeCost(World w) => ResearchLevelCost(w.ResearchLevel + 1);

    /// <summary>국경 봉쇄/해제. 감염 유입을 크게 줄이지만, 봉쇄국이 많을수록 수입이 줄어듭니다.</summary>
    public static ActionResult ToggleBorders(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (c.BordersClosed)
        {
            c.BordersClosed = false;
            return Ok($"{c.Name} 국경 개방");
        }
        if (!TryPay(w, BorderCost)) return NoFunds(BorderCost);

        c.BordersClosed = true;
        if (c.IsInfected) return Ok($"{c.Name} 국경 봉쇄");

        w.Trust -= 2;
        return Ok($"{c.Name} 국경 봉쇄 (감염 전 봉쇄에 대한 반발로 신뢰도 -2)");
    }

    /// <summary>방역 작전: 군 투입 + 격리 구역. 4주간 감염 속도를 낮추고 좀비 처치율을 올립니다.</summary>
    public static ActionResult Contain(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (c.ContainDays > 0) return Fail($"이미 방역 작전 중입니다. ({c.ContainDays}일 남음)");
        if (!TryPay(w, ContainCost)) return NoFunds(ContainCost);

        c.ContainDays = ContainDuration;
        c.Order += 10;
        w.AddNews($"{c.Name} 방역 작전 개시", ConsoleColor.Cyan);
        return Ok($"{c.Name} 방역 작전 개시 (4주)");
    }

    /// <summary>전술 핵: 좀비 전멸, 주민 85% 사망, 신뢰도 급락, 영구 폐허.</summary>
    public static ActionResult Nuke(World w, Country c)
    {
        if (c.Nuked) return Fail("이미 폐허가 된 국가입니다.");
        if (!TryPay(w, NukeCost)) return NoFunds(NukeCost);

        double ratio = c.ZombieRatio;
        double survivors = c.Healthy * 0.15;
        c.Dead += c.Zombies + c.Healthy - survivors;
        c.Healthy = survivors;
        c.Zombies = 0;
        c.Nuked = true;
        c.Collapsed = false;
        c.BordersClosed = false;

        double trustLoss = 15 + (1 - ratio) * 35;   // 감염이 적은 나라일수록 비난이 거셈
        w.Trust -= trustLoss;
        foreach (var n in c.LandNeighbors) n.Order -= 15;

        w.AddNews($"{c.Name}에 전술 핵 투하. 국제사회 충격", ConsoleColor.DarkRed);
        return Ok($"{c.Name} 핵 투하 · 좀비 전멸 · 신뢰도 -{trustLoss:0}");
    }

    public static ActionResult UpgradeResearch(World w)
    {
        if (w.CureDeployed) return Fail("치료제가 이미 완성되었습니다.");
        if (w.ResearchLevel >= MaxResearchLevel) return Fail("연구 투자가 이미 최대 단계입니다.");
        double cost = ResearchUpgradeCost(w);
        if (!TryPay(w, cost)) return NoFunds(cost);

        w.ResearchLevel++;
        w.AddNews($"치료제 연구 투자 {w.ResearchLevel}단계 승인", ConsoleColor.Cyan);
        return Ok($"연구 투자 {w.ResearchLevel}단계 (연구 속도 +{w.ResearchLevel * ResearchBonusPerLevel * 100:0}%)");
    }

    private static bool TryPay(World w, double cost)
    {
        if (w.Funds < cost) return false;
        w.Funds -= cost;
        return true;
    }

    private static ActionResult Ok(string msg) => new(true, msg);
    private static ActionResult Fail(string msg) => new(false, msg);
    private static ActionResult NoFunds(double cost) => new(false, $"예산이 부족합니다 (필요: ${cost:0}B)");
}
