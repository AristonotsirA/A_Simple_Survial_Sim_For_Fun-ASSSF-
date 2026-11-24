namespace ZombieOutbreak.Core;

/// <summary>세계를 19개 권역으로 나눈 ID. 순서가 곧 화면에 표시되는 번호(1~19)입니다.</summary>
public enum RegionId
{
    NorthAmerica, CentralAmerica, SouthAmericaNorth, SouthAmericaSouth,
    NorthEurope, WestEurope, SouthEurope, EastEurope, RussiaCentralAsia,
    MiddleEast, NorthAfrica, WestAfrica, CentralAfrica, EastAfrica, SouthernAfrica,
    SouthAsia, EastAsia, SoutheastAsia, Oceania
}

/// <summary>권역 하나. 소속 국가 목록과 육로로 이어진 이웃 권역을 가집니다.</summary>
public sealed class Region
{
    public Region(RegionId id, string name, int mapRow, int mapCol)
    {
        Id = id;
        Name = name;
        MapRow = mapRow;
        MapCol = mapCol;
    }

    public RegionId Id { get; }
    public string Name { get; }
    public int MapRow { get; }   // 세계 지도 타일에서의 행
    public int MapCol { get; }   // 세계 지도 타일에서의 열
    public List<Region> Neighbors { get; } = new();
    public List<Country> Countries { get; } = new();

    public int Number => (int)Id + 1;

    public double Living => Countries.Sum(c => c.Living);
    public double Zombies => Countries.Sum(c => c.Zombies);
    public double ZombieRatio => Living < 1 ? 0 : Zombies / Living;
    public int InfectedCount => Countries.Count(c => c.IsInfected);
    public int CollapsedCount => Countries.Count(c => c.Collapsed);
}
