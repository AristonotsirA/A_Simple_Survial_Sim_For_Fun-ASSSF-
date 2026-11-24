using ZombieOutbreak.Core;

namespace ZombieOutbreak.Data;

/// <summary>권역 목록, 지도 위치, 육로로 연결된 이웃 관계를 정의합니다.</summary>
public static class RegionTable
{
    // 지도 배치 (4행 x 6열, 빈칸은 바다)
    //        0          1          2          3          4          5
    // 0   북미                  북유럽      동유럽      러시아권
    // 1   중미카리브            서유럽      남유럽      중동        동아시아
    // 2   남미북부   서아프리카 북아프리카  동아프리카  남아시아    동남아시아
    // 3   남미남부              중앙아프리카 남부아프리카            오세아니아
    public static List<Region> Build()
    {
        // 반드시 RegionId enum 순서와 같아야 합니다 (Number 계산에 쓰임).
        var list = new List<Region>
        {
            new(RegionId.NorthAmerica,      "북미",         0, 0),
            new(RegionId.CentralAmerica,    "중미카리브",   1, 0),
            new(RegionId.SouthAmericaNorth, "남미북부",     2, 0),
            new(RegionId.SouthAmericaSouth, "남미남부",     3, 0),
            new(RegionId.NorthEurope,       "북유럽",       0, 2),
            new(RegionId.WestEurope,        "서유럽",       1, 2),
            new(RegionId.SouthEurope,       "남유럽",       1, 3),
            new(RegionId.EastEurope,        "동유럽",       0, 3),
            new(RegionId.RussiaCentralAsia, "러시아중앙아", 0, 4),
            new(RegionId.MiddleEast,        "중동",         1, 4),
            new(RegionId.NorthAfrica,       "북아프리카",   2, 2),
            new(RegionId.WestAfrica,        "서아프리카",   2, 1),
            new(RegionId.CentralAfrica,     "중앙아프리카", 3, 2),
            new(RegionId.EastAfrica,        "동아프리카",   2, 3),
            new(RegionId.SouthernAfrica,    "남부아프리카", 3, 3),
            new(RegionId.SouthAsia,         "남아시아",     2, 4),
            new(RegionId.EastAsia,          "동아시아",     1, 5),
            new(RegionId.SoutheastAsia,     "동남아시아",   2, 5),
            new(RegionId.Oceania,           "오세아니아",   3, 5),
        };

        var map = list.ToDictionary(r => r.Id);

        // 로컬 함수: 메서드 안에서만 쓰는 작은 함수
        void Link(RegionId a, RegionId b)
        {
            map[a].Neighbors.Add(map[b]);
            map[b].Neighbors.Add(map[a]);
        }

        Link(RegionId.NorthAmerica, RegionId.CentralAmerica);
        Link(RegionId.CentralAmerica, RegionId.SouthAmericaNorth);
        Link(RegionId.SouthAmericaNorth, RegionId.SouthAmericaSouth);
        Link(RegionId.NorthEurope, RegionId.WestEurope);
        Link(RegionId.NorthEurope, RegionId.EastEurope);
        Link(RegionId.NorthEurope, RegionId.RussiaCentralAsia);
        Link(RegionId.WestEurope, RegionId.SouthEurope);
        Link(RegionId.WestEurope, RegionId.EastEurope);
        Link(RegionId.EastEurope, RegionId.SouthEurope);
        Link(RegionId.EastEurope, RegionId.RussiaCentralAsia);
        Link(RegionId.SouthEurope, RegionId.MiddleEast);
        Link(RegionId.SouthEurope, RegionId.NorthAfrica);
        Link(RegionId.RussiaCentralAsia, RegionId.MiddleEast);
        Link(RegionId.RussiaCentralAsia, RegionId.EastAsia);
        Link(RegionId.RussiaCentralAsia, RegionId.SouthAsia);
        Link(RegionId.MiddleEast, RegionId.NorthAfrica);
        Link(RegionId.MiddleEast, RegionId.EastAfrica);
        Link(RegionId.MiddleEast, RegionId.SouthAsia);
        Link(RegionId.NorthAfrica, RegionId.WestAfrica);
        Link(RegionId.NorthAfrica, RegionId.CentralAfrica);
        Link(RegionId.NorthAfrica, RegionId.EastAfrica);
        Link(RegionId.WestAfrica, RegionId.CentralAfrica);
        Link(RegionId.CentralAfrica, RegionId.EastAfrica);
        Link(RegionId.CentralAfrica, RegionId.SouthernAfrica);
        Link(RegionId.EastAfrica, RegionId.SouthernAfrica);
        Link(RegionId.SouthAsia, RegionId.EastAsia);
        Link(RegionId.SouthAsia, RegionId.SoutheastAsia);
        Link(RegionId.EastAsia, RegionId.SoutheastAsia);
        Link(RegionId.SoutheastAsia, RegionId.Oceania);

        return list;
    }
}
