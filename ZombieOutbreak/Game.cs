using ZombieOutbreak.Core;
using ZombieOutbreak.Data;
using ZombieOutbreak.Systems;
using ZombieOutbreak.UI;

namespace ZombieOutbreak;

/// <summary>게임 흐름 담당: 타이틀 → 브리핑 → (플레이어 조치 ↔ 턴 진행) 반복 → 엔딩.</summary>
public sealed class Game
{
    public void Run()
    {
        while (true)
        {
            var difficulty = MenuScreens.Title();
            if (difficulty == null) return;

            var world = WorldFactory.Create(difficulty, Environment.TickCount);
            EventDeck.Reset();
            Simulation.StartOutbreak(world);
            MenuScreens.Briefing(world);

            var result = Play(world);
            if (!MenuScreens.Ending(world, result)) return;
        }
    }

    private static GameResult Play(World w)
    {
        string? message = null;
        bool success = true;

        while (true)
        {
            HudScreen.Draw(w, message, success);
            message = null;

            var key = Ui.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Simulation.Step(w);
                    var result = Simulation.CheckResult(w);
                    if (result != GameResult.Ongoing) return result;

                    if (EventDeck.Draw(w) is { } drawn)   // 패턴 매칭: null이 아니면 drawn에 담음
                    {
                        EventScreen.Show(w, drawn);
                        result = Simulation.CheckResult(w);   // 사건 선택으로 신뢰도가 0이 될 수도 있음
                        if (result != GameResult.Ongoing) return result;
                    }
                    break;

                case ConsoleKey.Escape:
                    if (Ui.Confirm("정말 작전을 포기하시겠습니까?")) return GameResult.Abandoned;
                    break;
            }

            switch (key.KeyChar)
            {
                case '1':
                    CountryScreens.ShowList(w);
                    break;
                case '2':
                    if (MenuScreens.SelectRegion(w) is { } region) CountryScreens.ShowList(w, region);
                    break;
                case '3':
                    MenuScreens.Research(w);
                    break;
                case '4':
                    var speech = Actions.Speech(w);
                    (message, success) = (speech.Message, speech.Success);   // 튜플로 두 변수 동시 대입
                    break;
                case '5':
                    MenuScreens.Statistics(w);
                    break;
            }
        }
    }
}
