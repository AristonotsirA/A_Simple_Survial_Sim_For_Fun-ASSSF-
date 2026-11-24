using ZombieOutbreak.Core;

namespace ZombieOutbreak.Data;

public static class WorldFactory
{
    public static World Create(Difficulty difficulty, int seed)
    {
        var countries = CountryTable.Load();
        var map = new WorldMap(MapData.Rows, MapData.Codes, countries.ToDictionary(c => c.Code));
        return new World(countries, map, difficulty, seed);
    }
}
