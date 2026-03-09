using UnityEngine;

namespace Won
{
    public static class Constants
    {
        public const int BoardSize = 15;
        public const int WinCondition = 5;
        public const int PointsPerRank = 4;   // 급수당 필요 승리 수
        public const int MaxRank = 9;          // 최고 급수 (9급이 최상위)

        public enum StoneType { None, Black, White }
        public enum GameMode { Single, Dual, Multi }
        public enum UIElement { SettingsPopup, GamePanel }
    }

    /// <summary>
    /// 저장 필드: DisplayName, UserId, TotalWins, TotalLosses
    /// 나머지는 계산 프로퍼티 — 중복 저장 없음
    /// </summary>
    public class PlayerProfile
    {
        // ── 저장할 최소 데이터 ──────────────────────────
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }

        // ── 계산 프로퍼티 (저장 불필요) ──────────────────
        public int TotalGames => TotalWins + TotalLosses;
        public float WinRate => TotalGames == 0 ? 0f : (float)TotalWins / TotalGames;

        /// <summary>9급(초보) → 1급(고수). 4승마다 1급 상승.</summary>
        public int Rank => Mathf.Max(1, Constants.MaxRank - TotalWins / Constants.PointsPerRank);

        /// <summary>현재 급수 내 누적 승리 수 (0~3)</summary>
        public int PointsInRank => TotalWins % Constants.PointsPerRank;

        /// <summary>다음 급수까지 남은 승리 수</summary>
        public int PointsToNextRank => Constants.PointsPerRank - PointsInRank;
    }

    /// <summary>
    /// 저장 필드: Winner, MyStoneColor, TotalMoves
    /// 판정/점수는 계산 프로퍼티
    /// </summary>
    public class GameResult
    {
        // ── 저장할 최소 데이터 ──────────────────────────
        public Constants.StoneType Winner { get; set; }
        public Constants.StoneType MyStoneColor { get; set; }
        public int TotalMoves { get; set; }

        // ── 계산 프로퍼티 ─────────────────────────────
        public bool IsWin => Winner != Constants.StoneType.None && Winner == MyStoneColor;
        public bool IsDraw => Winner == Constants.StoneType.None;
        public bool IsLose => !IsWin && !IsDraw;

        /// <summary>승: +1, 무승부: 0, 패: -1</summary>
        public int ScoreGained => IsWin ? 1 : IsDraw ? 0 : -1;

        public string ResultLabel => IsWin ? "승\n리" : IsDraw ? "무\n승\n부" : "패\n배";
    }
}
