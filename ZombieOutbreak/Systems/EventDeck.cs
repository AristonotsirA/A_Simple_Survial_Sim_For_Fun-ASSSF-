using ZombieOutbreak.Core;

namespace ZombieOutbreak.Systems;

/// <summary>선택지 하나. Apply는 '나중에 실행할 함수'를 값처럼 저장해 둔 것(델리게이트)입니다.</summary>
public sealed record EventChoice(string Label, string Effect, double Cost, Action<World, Country?> Apply);

public sealed class GameEvent
{
    public required string Title { get; init; }
    public required Func<Country?, string> Body { get; init; }
    public required EventChoice[] Choices { get; init; }

    /// <summary>대상 국가를 고르는 함수. null이면 특정 국가와 무관한 전 세계 이벤트입니다.</summary>
    public Func<World, Country?>? Target { get; init; }

    /// <summary>발생 조건. null이면 언제든 발생 가능.</summary>
    public Func<World, bool>? Condition { get; init; }
}

public sealed record DrawnEvent(GameEvent Event, Country? Target);

/// <summary>무작위 사건 카드 묶음. 매 턴 일정 확률로 한 장을 뽑습니다.</summary>
public static class EventDeck
{
    private const double EventChance = 0.4;
    private const int RepeatCooldown = 8;   // 같은 사건은 8주 안에 다시 나오지 않음

    private static readonly Dictionary<GameEvent, int> LastSeen = new();

    public static DrawnEvent? Draw(World w)
    {
        if (w.Rng.NextDouble() >= EventChance) return null;

        var candidates = new List<DrawnEvent>();
        foreach (var ev in All)
        {
            if (LastSeen.TryGetValue(ev, out int week) && w.Week - week < RepeatCooldown) continue;
            if (ev.Condition != null && !ev.Condition(w)) continue;

            Country? target = ev.Target?.Invoke(w);
            if (ev.Target != null && target == null) continue;
            candidates.Add(new DrawnEvent(ev, target));
        }
        if (candidates.Count == 0) return null;

        var drawn = candidates[w.Rng.Next(candidates.Count)];
        LastSeen[drawn.Event] = w.Week;
        return drawn;
    }

    /// <summary>새 게임을 시작할 때 이전 판의 기록을 지웁니다.</summary>
    public static void Reset() => LastSeen.Clear();

    // ───────────────────────── 대상 국가 선택 도우미 ─────────────────────────

    private static Country? RandomCountry(World w, Func<Country, bool> filter)
    {
        var list = w.Countries.Where(c => !c.Nuked && c.Living > 1_000_000 && filter(c)).ToList();
        return list.Count == 0 ? null : list[w.Rng.Next(list.Count)];
    }

    private static Country? Infected(World w) => RandomCountry(w, c => c.IsInfected);
    private static Country? Developed(World w) => RandomCountry(w, c => c.Wealth != Wealth.Low && !c.Collapsed);

    private static Country? CleanNearInfected(World w) => RandomCountry(w, c =>
        !c.IsInfected && (c.Region.InfectedCount > 0 || c.Region.Neighbors.Any(n => n.InfectedCount > 0)));

    private static Country? MostInfected(World w) =>
        w.Countries.Where(c => c.IsInfected && !c.Collapsed).MaxBy(c => c.Zombies);

    private static void MaybeSeed(World w, Country c, double chance, string reason)
    {
        if (w.Rng.NextDouble() < chance) Simulation.Seed(w, c, 100, reason);
    }

    // ───────────────────────── 사건 목록 ─────────────────────────
    // [ ... ] 는 C# 12의 '컬렉션 식'으로, 배열을 간단히 만드는 문법입니다.

    private static readonly GameEvent[] All =
    [
        new()
        {
            Title = "\"좀비는 가짜뉴스다\"",
            Target = Infected,
            Body = c => $"{c!.Name} 정부가 감염 사태를 부정하며 국제 대응팀의 입국을 거부했습니다. 현지 언론은 철저히 통제되고 있습니다.",
            Choices =
            [
                new("외교적 압박을 가한다", "예산 -10, 해당국 격리 3주", 10, (w, c) => c!.QuarantineTurns = Math.Max(c.QuarantineTurns, 3)),
                new("내정 간섭은 피한다", "해당국 질서 -15", 0, (w, c) => c!.Order -= 15),
            ]
        },
        new()
        {
            Title = "유망한 항체 발견",
            Target = Developed,
            Body = c => $"{c!.Name} 국립연구소에서 좀비 바이러스를 무력화하는 항체 후보 물질을 발견했다는 보고가 들어왔습니다.",
            Choices =
            [
                new("최정예 연구팀을 급파한다", "예산 -15, 연구 +8%", 15, (w, c) => w.Research += 8),
                new("현지 연구진에게 맡긴다", "연구 +3%", 0, (w, c) => w.Research += 3),
            ]
        },
        new()
        {
            Title = "변이 바이러스 출현",
            Condition = w => w.Week >= 6,
            Body = _ => "감염자 표본에서 새로운 변이가 확인되었습니다. 잠복기가 짧아지고 공격성이 크게 높아졌습니다.",
            Choices =
            [
                new("연구 인력을 변이 분석에 돌린다", "연구 -5%, 감염력 소폭 상승", 0, (w, c) => { w.Research -= 5; w.Infectivity += 0.05; }),
                new("치료제 개발에만 집중한다", "감염력 크게 상승", 0, (w, c) => w.Infectivity += 0.15),
            ]
        },
        new()
        {
            Title = "대규모 축제 강행",
            Target = CleanNearInfected,
            Body = c => $"{c!.Name}에서 수백만 명이 모이는 전통 축제가 예정되어 있습니다. 주최 측은 \"공포에 굴복하지 않겠다\"며 강행 의사를 밝혔습니다.",
            Choices =
            [
                new("취소를 강력히 권고한다", "신뢰도 -4", 0, (w, c) => w.Trust -= 4),
                new("개최를 허용한다", "신뢰도 +2, 40% 확률로 감염 발생", 0, (w, c) => { w.Trust += 2; MaybeSeed(w, c!, 0.4, "축제 인파"); }),
            ]
        },
        new()
        {
            Title = "비상착륙 요청",
            Target = CleanNearInfected,
            Body = c => $"감염 의심 승객을 태운 여객기가 {c!.Name} 상공에서 비상착륙을 요청하고 있습니다. 연료가 얼마 남지 않았습니다.",
            Choices =
            [
                new("착륙을 허가하고 격리한다", "예산 -5, 신뢰도 +2, 35% 확률로 감염", 5, (w, c) => { w.Trust += 2; MaybeSeed(w, c!, 0.35, "비상착륙 여객기"); }),
                new("회항을 지시한다", "신뢰도 -3", 0, (w, c) => w.Trust -= 3),
                new("격추를 명령한다", "신뢰도 -15", 0, (w, c) => w.Trust -= 15),
            ]
        },
        new()
        {
            Title = "억만장자의 제안",
            Body = _ => "세계 최고 부호가 막대한 사재 기부를 제안했습니다. 조건은 단 하나, 자신과 가족의 '안전 벙커 우선 배정'입니다.",
            Choices =
            [
                new("제안을 수락한다", "예산 +40, 신뢰도 -8", 0, (w, c) => { w.Funds += 40; w.Trust -= 8; }),
                new("정중히 거절한다", "신뢰도 +3", 0, (w, c) => w.Trust += 3),
            ]
        },
        new()
        {
            Title = "식량 폭동",
            Target = w => RandomCountry(w, c => c.IsInfected && c.Order < 50 && !c.Collapsed),
            Body = c => $"{c!.Name}에서 굶주린 시민들이 식량 창고를 습격했습니다. 치안 병력은 좀비와 시민 사이에서 갈피를 잡지 못하고 있습니다.",
            Choices =
            [
                new("구호물자를 공수한다", "예산 -12, 해당국 질서 +25", 12, (w, c) => c!.Order += 25),
                new("무력 진압을 승인한다", "해당국 질서 +12, 신뢰도 -6", 0, (w, c) => { c!.Order += 12; w.Trust -= 6; }),
            ]
        },
        new()
        {
            Title = "종말론 교단",
            Condition = w => w.Week >= 4,
            Body = _ => "\"좀비는 신의 심판이며 구원이다\"를 외치는 교단이 세계 곳곳에서 대규모 집회를 열고 있습니다.",
            Choices =
            [
                new("집회를 강제 해산한다", "신뢰도 -3", 0, (w, c) => w.Trust -= 3),
                new("표현의 자유로 보고 방치한다", "무작위 3개국 질서 -10", 0, (w, c) =>
                {
                    for (int i = 0; i < 3; i++)
                        if (RandomCountry(w, _ => true) is { } target) target.Order -= 10;
                }),
            ]
        },
        new()
        {
            Title = "의료 자원봉사단",
            Body = _ => "전 세계에서 수천 명의 의료진이 자원봉사를 신청했습니다. 어디에 배치하시겠습니까?",
            Choices =
            [
                new("최전선에 배치한다", "가장 심각한 국가 격리 4주", 0, (w, c) =>
                {
                    var worst = MostInfected(w);
                    if (worst != null) worst.QuarantineTurns = Math.Max(worst.QuarantineTurns, 4);
                }),
                new("연구소를 지원한다", "연구 +4%", 0, (w, c) => w.Research += 4),
            ]
        },
        new()
        {
            Title = "동물 매개 감염",
            Condition = w => w.Week >= 8,
            Body = _ => "쥐와 들개도 바이러스를 옮긴다는 사실이 확인되었습니다. 도시 하수도가 새로운 전파 경로가 되고 있습니다.",
            Choices =
            [
                new("대규모 방역을 실시한다", "예산 -20", 20, (w, c) => { }),
                new("인력이 부족하다. 무시한다", "감염력 상승", 0, (w, c) => w.Infectivity += 0.1),
            ]
        },
        new()
        {
            Title = "연구 데이터 독점",
            Target = Developed,
            Body = c => $"{c!.Name} 정부가 자국 연구 데이터의 공유를 거부했습니다. \"자국민 우선\"이라는 입장입니다.",
            Choices =
            [
                new("지원금을 주고 협상한다", "예산 -15, 연구 +7%", 15, (w, c) => w.Research += 7),
                new("국제 제재를 건다", "신뢰도 -4, 연구 +3%, 해당국 질서 -5", 0, (w, c) => { w.Trust -= 4; w.Research += 3; c!.Order -= 5; }),
            ]
        },
        new()
        {
            Title = "UN 긴급 총회",
            Body = _ => "UN 긴급 총회가 소집되었습니다. 회원국들에게 추가 분담금을 요구할 수 있는 기회입니다.",
            Choices =
            [
                new("강력하게 요구한다", "예산 +35, 신뢰도 -7", 0, (w, c) => { w.Funds += 35; w.Trust -= 7; }),
                new("자발적 기여를 호소한다", "예산 +12", 0, (w, c) => w.Funds += 12),
            ]
        },
        new()
        {
            Title = "가짜 치료제 루머",
            Body = _ => "\"치료제는 이미 있고 부자들만 맞고 있다\"는 소문이 SNS를 뒤덮었습니다.",
            Choices =
            [
                new("공식 반박 자료를 낸다", "예산 -5, 신뢰도 +3", 5, (w, c) => w.Trust += 3),
                new("대응할 가치가 없다", "신뢰도 -6", 0, (w, c) => w.Trust -= 6),
            ]
        },
        new()
        {
            Title = "국경의 난민 행렬",
            Target = CleanNearInfected,
            Body = c => $"감염 지역을 탈출한 수십만 명의 난민이 {c!.Name} 국경에 몰려들고 있습니다.",
            Choices =
            [
                new("인도적 수용을 권고한다", "신뢰도 +5, 30% 확률로 감염", 0, (w, c) => { w.Trust += 5; MaybeSeed(w, c!, 0.3, "난민 유입"); }),
                new("국경 봉쇄를 지지한다", "해당국 국경 봉쇄, 신뢰도 -4", 0, (w, c) => { c!.BordersClosed = true; w.Trust -= 4; }),
            ]
        },
        new()
        {
            Title = "인체 임상시험",
            Condition = w => w.Research is >= 30 and < 95,
            Body = _ => "치료제 후보 물질이 동물 실험을 통과했습니다. 아직 안전성이 검증되지 않았지만 지원자는 넘쳐납니다.",
            Choices =
            [
                new("임상시험을 강행한다", "연구 +12%, 신뢰도 -6", 0, (w, c) => { w.Research += 12; w.Trust -= 6; }),
                new("단계적으로 진행한다", "연구 +5%", 0, (w, c) => w.Research += 5),
            ]
        },
        new()
        {
            Title = "연구소 정전",
            Condition = w => w.Research > 10 && !w.CureDeployed,
            Body = _ => "핵심 연구소의 전력망이 끊겼습니다. 냉동 보관 중인 샘플이 위험합니다.",
            Choices =
            [
                new("비상 발전기를 공수한다", "예산 -10", 10, (w, c) => { }),
                new("일부 샘플을 포기한다", "연구 -6%", 0, (w, c) => w.Research -= 6),
            ]
        },
        new()
        {
            Title = "작은 마을의 기적",
            Target = Infected,
            Body = c => $"{c!.Name}의 한 시골 마을이 3주째 좀비 무리를 막아내고 있다는 소식이 전 세계에 퍼지고 있습니다.",
            Choices =
            [
                new("영웅으로 대대적 홍보한다", "신뢰도 +6", 0, (w, c) => w.Trust += 6),
                new("보급품을 공수한다", "예산 -8, 해당국 질서 +20, 신뢰도 +2", 8, (w, c) => { c!.Order += 20; w.Trust += 2; }),
            ]
        },
        new()
        {
            Title = "파견군 지휘관의 항명",
            Target = w => RandomCountry(w, c => c.MilitaryTurns > 0),
            Body = c => $"{c!.Name}에 파견된 부대 지휘관이 \"병사들이 한계에 달했다\"며 즉각 철수를 요구하고 있습니다.",
            Choices =
            [
                new("위험수당을 지급한다", "예산 -10, 파견 유지", 10, (w, c) => { }),
                new("철수를 허가한다", "파견 종료, 신뢰도 -3", 0, (w, c) => { c!.MilitaryTurns = 0; w.Trust -= 3; }),
            ]
        },
        new()
        {
            Title = "제약회사의 거래",
            Condition = w => !w.CureDeployed,
            Body = _ => "거대 제약회사가 생산 설비를 제공하겠다고 제안했습니다. 대가는 치료제 특허 독점권입니다.",
            Choices =
            [
                new("거래를 수락한다", "연구 +6%, 신뢰도 -5", 0, (w, c) => { w.Research += 6; w.Trust -= 5; }),
                new("거절한다", "변화 없음", 0, (w, c) => { }),
            ]
        },
    ];
}
