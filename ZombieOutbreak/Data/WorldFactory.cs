using ZombieOutbreak.Core;

namespace ZombieOutbreak.Data;

public static class WorldFactory
{
    public static World Create(Difficulty difficulty, int seed)
    {
        var regions = RegionTable.Build();
        CountryTable.Populate(regions);
        return new World(regions, difficulty, seed);
    }
}
