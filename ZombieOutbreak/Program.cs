using System.Text;
using ZombieOutbreak;

Console.OutputEncoding = Encoding.UTF8;
Console.Title = "LAST LINE - Zombie Outbreak";

// 세계지도를 그리려면 120x37 이상이 필요하므로, Windows 기본 콘솔에서는 창 크기를 키워 봅니다.
// (Windows Terminal에서는 창 크기를 프로그램이 바꿀 수 없으므로 실패해도 무시)
if (OperatingSystem.IsWindows())
{
    try
    {
        Console.BufferWidth = Math.Max(Console.BufferWidth, 120);
        Console.BufferHeight = Math.Max(Console.BufferHeight, 37);
        Console.SetWindowSize(Math.Min(120, Console.LargestWindowWidth), Math.Min(37, Console.LargestWindowHeight));
    }
    catch (Exception) { }
}

new Game().Run();
