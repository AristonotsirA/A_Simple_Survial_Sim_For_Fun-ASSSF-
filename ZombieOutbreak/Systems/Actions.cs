using ZombieOutbreak.Core;

namespace ZombieOutbreak.Systems;

public sealed record ActionResult(bool Success, string Message);

/// <summary>플레이어가 실행할 수 있는 조치. 비용 확인 → 효과 적용 → 결과 메시지 반환의 패턴을 따릅니다.</summary>
public static class Actions
{
    public const double BorderCost = 5;
    public const double MilitaryCost = 15;
    public const double QuarantineCost = 10;
    public const double ReliefCost = 8;
    public const double NukeCost = 30;
    public const double SpeechCost = 5;
    public const int SpeechCooldown = 4;
    public const int MaxResearchLevel = 5;
    public const double BaseResearchRate = 1.1;       // 투자 0단계, 연구 기반 100%일 때 주당 진척(%)
    public const double ResearchBonusPerLevel = 0.15; // 투자 1단계마다 +15%

    /// <summary>level단계 투자에 드는 비용 (1단계 30, 2단계 55, ...)</summary>
    public static double ResearchLevelCost(int level) => 30 + (level - 1) * 25;
    public static double ResearchUpgradeCost(World w) => ResearchLevelCost(w.ResearchLevel + 1);

    public static ActionResult ToggleBorders(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (c.BordersClosed)
        {
            c.BordersClosed = false;
            return Ok($"{c.Name}의 국경을 다시 개방했습니다.");
        }
        if (!TryPay(w, BorderCost)) return NoFunds(BorderCost);

        c.BordersClosed = true;
        w.AddNews($"[조치] {c.Name} 국경 봉쇄", ConsoleColor.Cyan);
        if (c.IsInfected) return Ok($"{c.Name}의 국경을 봉쇄했습니다.");

        w.Trust -= 2;
        return Ok($"{c.Name}의 국경을 봉쇄했습니다. (감염 전 봉쇄에 대한 반발로 신뢰도 -2)");
    }

    public static ActionResult DeployMilitary(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (c.MilitaryTurns > 0) return Fail("이미 군이 파견되어 있습니다.");
        if (!TryPay(w, MilitaryCost)) return NoFunds(MilitaryCost);

        c.MilitaryTurns = 3;
        c.Order += 10;
        w.AddNews($"[조치] {c.Name}에 다국적군 파견", ConsoleColor.Cyan);
        return Ok($"{c.Name}에 3주간 군을 파견합니다. 좀비 처치율이 크게 오릅니다.");
    }

    public static ActionResult Quarantine(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (c.QuarantineTurns > 0) return Fail("이미 격리 구역이 운영 중입니다.");
        if (!TryPay(w, QuarantineCost)) return NoFunds(QuarantineCost);

        c.QuarantineTurns = 3;
        w.AddNews($"[조치] {c.Name} 격리 구역 설정", ConsoleColor.Cyan);
        return Ok($"{c.Name}에 3주간 격리 구역을 운영합니다. 내부 감염 속도가 절반이 됩니다.");
    }

    public static ActionResult Relief(World w, Country c)
    {
        if (c.Nuked) return Fail("폐허가 된 국가입니다.");
        if (!TryPay(w, ReliefCost)) return NoFunds(ReliefCost);

        c.Order += 20;
        w.Trust += 2;
        return Ok($"{c.Name}에 긴급 구호물자를 보냈습니다. 질서 +20, 신뢰도 +2");
    }

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
        c.Order = 5;

        double trustLoss = 15 + (1 - ratio) * 35; // 감염이 적은 나라일수록 비난이 거셈
        w.Trust -= trustLoss;
        foreach (var other in c.Region.Countries) other.Order -= 10;

        w.AddNews($"[핵] {c.Name}에 전술 핵 투하. 국제사회 충격", ConsoleColor.DarkRed);
        return Ok($"{c.Name}의 좀비가 전멸했습니다. 신뢰도 -{trustLoss:0}, 주변국 질서 -10");
    }

    public static ActionResult Speech(World w)
    {
        int wait = w.LastSpeechWeek + SpeechCooldown - w.Week;
        if (wait > 0) return Fail($"아직 담화를 할 수 없습니다. ({wait}주 후 가능)");
        if (!TryPay(w, SpeechCost)) return NoFunds(SpeechCost);

        w.LastSpeechWeek = w.Week;
        w.Trust += 6;
        w.AddNews("[담화] 사령관 대국민 담화: \"인류는 반드시 이겨낸다\"", ConsoleColor.Cyan);
        return Ok("대국민 담화를 발표했습니다. 신뢰도 +6");
    }

    public static ActionResult UpgradeResearch(World w)
    {
        if (w.CureDeployed) return Fail("치료제가 이미 완성되었습니다.");
        if (w.ResearchLevel >= MaxResearchLevel) return Fail("연구 투자가 이미 최대 단계입니다.");
        double cost = ResearchUpgradeCost(w);
        if (!TryPay(w, cost)) return NoFunds(cost);

        w.ResearchLevel++;
        w.AddNews($"[연구] 치료제 연구 투자 {w.ResearchLevel}단계 승인", ConsoleColor.Cyan);
        return Ok($"연구 투자 {w.ResearchLevel}단계. 연구 속도 +{w.ResearchLevel * ResearchBonusPerLevel * 100:0}%");
    }

    private static bool TryPay(World w, double cost)
    {
        if (w.Funds < cost) return false;
        w.Funds -= cost;
        return true;
    }

    private static ActionResult Ok(string msg) => new(true, msg);
    private static ActionResult Fail(string msg) => new(false, msg);
    private static ActionResult NoFunds(double cost) => new(false, $"예산이 부족합니다. (필요: ${cost:0}B)");
}
