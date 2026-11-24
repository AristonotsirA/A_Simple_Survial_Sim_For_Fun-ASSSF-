using System.Diagnostics;
using ZombieOutbreak.Core;
using ZombieOutbreak.Data;
using ZombieOutbreak.Systems;
using ZombieOutbreak.UI;

namespace ZombieOutbreak;

/// <summary>
/// 게임 흐름 담당: 타이틀 → 브리핑 → 실시간 게임 루프 → 엔딩.
/// 실시간 루프 = "키 입력 처리 → 시간이 됐으면 하루 진행 → 바뀐 게 있으면 다시 그리기"의 반복.
/// </summary>
public sealed class Game
{
    private const int BlinkMs = 450;
    private const int FrameSleepMs = 15;

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
        var view = new ViewState();
        view.MoveCursorTo(w.PatientZero);
        var buffer = new ScreenBuffer();
        var clock = Stopwatch.StartNew();   // 경과 시간을 밀리초 단위로 재는 타이머
        long nextDayMs = ViewState.DayLengthMs[view.Speed];
        long nextBlinkMs = 0;

        MenuScreens.EnsureWindowSize();
        Console.CursorVisible = false;

        try
        {
            while (true)
            {
                long now = clock.ElapsedMilliseconds;
                bool dirty = false;

                if (Console.WindowWidth <= ScreenBuffer.Width || Console.WindowHeight < ScreenBuffer.Height)
                {
                    MenuScreens.EnsureWindowSize();
                    buffer.Invalidate();
                    dirty = true;
                }

                while (Console.KeyAvailable)
                {
                    if (!Input.Handle(w, view, Console.ReadKey(intercept: true), now)) return GameResult.Abandoned;
                    dirty = true;
                }

                if (view.IsRunning && now >= nextDayMs)
                {
                    Simulation.Step(w);
                    nextDayMs = now + ViewState.DayLengthMs[view.Speed];
                    dirty = true;

                    if (w.Day % 7 == 0 && EventDeck.Draw(w) is { } drawn)   // 매주 한 번 사건 카드 판정
                    {
                        view.Event = drawn;
                        view.EventError = null;
                    }
                }

                if (now >= nextBlinkMs)
                {
                    view.Blink = !view.Blink;
                    nextBlinkMs = now + BlinkMs;
                    dirty = true;
                }

                if (!dirty) { Thread.Sleep(FrameSleepMs); continue; }

                var result = Simulation.CheckResult(w);
                if (result != GameResult.Ongoing) return result;

                GameScreen.Render(w, view, buffer, now);
                buffer.Flush();
            }
        }
        finally   // return으로 빠져나가도 반드시 실행됨
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
    }
}
