namespace ZombieOutbreak.Core;

/// <summary>난이도별 수치. record는 '값을 담는 용도'의 클래스를 한 줄로 선언하는 문법입니다.</summary>
public sealed record Difficulty(string Name, double Infectivity, double StartFunds, double ResearchMult, double IncomeMult)
{
    public static readonly Difficulty Easy = new("쉬움", 1.6, 150, 1.25, 1.2);
    public static readonly Difficulty Normal = new("보통", 1.85, 100, 1.0, 1.0);
    public static readonly Difficulty Hard = new("어려움", 2.1, 70, 0.85, 0.85);
}
