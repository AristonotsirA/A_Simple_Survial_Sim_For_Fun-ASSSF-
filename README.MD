# A Simple Survival Simulator (ASSSF)

간단하게 즐기는 세계 시뮬레이션. 지구 전체(196개국)를 무대로 시나리오별로 진행됩니다.
C# 콘솔(.NET 8)로 제작했습니다.

## 시나리오
| 시나리오 | 상태 | 설명 |
|---|---|---|
| LAST LINE: 좀비 아웃브레이크 | ✅ | UN 산하 '세계 대응 사령부' 사령관이 되어 좀비 확산을 막고 치료제를 완성하세요. |

## 실행 방법
- **Visual Studio**: `ZombieOutbreak.sln` 열기 → F5
- **터미널**: `dotnet run --project ZombieOutbreak`
- .NET 8 SDK가 필요합니다.
- 창 크기 **120x36 이상**을 권장합니다 (Windows Terminal에서 창 최대화).
- 글자 명령(n, p, f, Y, R, Q)은 **영문 입력 상태**에서 누르는 게 가장 확실합니다.

## 플레이 요약
- 한 턴 = 1주. 예산 안에서 조치를 내리고 `Enter`로 시간을 진행합니다.
- 조치: 국경 봉쇄 · 군 파견 · 격리 구역 · 긴급 구호 · 전술 핵 · 연구 투자 · 대국민 담화
- 무작위 사건 카드 19종이 선택을 요구합니다.
- 승리: 치료제 100% 완성 후 좀비 소탕
- 패배: 생존자가 세계 인구의 40% 미만 → 문명 붕괴 / 신뢰도 0 → 사령관 해임

## 코드 구조
```
ZombieOutbreak/
├─ Core/      데이터 모델 (Country, Region, World, Difficulty)  ─ 무엇이 있는가
├─ Data/      196개국·19개 권역 데이터와 World 생성기           ─ 처음 세계를 어떻게 만드는가
├─ Systems/   규칙 엔진 (Simulation, Actions, EventDeck)        ─ 세계가 어떻게 변하는가
├─ UI/        콘솔 화면 (Hud, CountryScreens, EventScreen...)   ─ 어떻게 보여주는가
├─ Game.cs    게임 흐름 (타이틀 → 플레이 루프 → 엔딩)
└─ Program.cs 진입점
```
UI는 Systems를 호출만 하고, Systems는 UI를 전혀 모릅니다.
덕분에 같은 규칙 엔진을 화면 없이 테스트하거나 Unity·서버로 옮길 수 있습니다.

## 데이터 출처
국가별 인구는 2024년 기준 근사치이며, 감염 모델은 게임용으로 단순화한 것입니다.
