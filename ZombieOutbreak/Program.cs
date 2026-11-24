using System.Text;
using ZombieOutbreak;

Console.OutputEncoding = Encoding.UTF8;
Console.Title = "LAST LINE - Zombie Outbreak";

// 화면이 120x34보다 작으면 지도가 깨지므로, Windows 기본 콘솔에서는 창 크기를 키워 봅니다.
// (Windows Terminal에서는 창 크기를 프로그램이 바꿀 수 없으므로 실패해도 무시)
if (OperatingSystem.IsWindows())
{
    try
    {
        Console.BufferWidth = Math.Max(Console.BufferWidth, 120);
        Console.SetWindowSize(Math.Min(120, Console.LargestWindowWidth), Math.Min(36, Console.LargestWindowHeight));
    }
    catch (Exception) { }
}

new Game().Run();
