using System;
using System.Collections.Generic;
using System.Linq;
using static Won.Constants;
// using System.Threading.Tasks;

namespace Won
{
    public enum TTFlag
    {
        Exact,  // 정확한 값
        LowerBound,  // Lower Bound (알파 컷으로 인해 더 낮게 제한됨)
        UpperBound   // Upper Bound (베타 컷으로 인해 더 높게 제한됨)
    }

    public struct TTEntry
    {
        public int Score;
        public int Depth;
        public TTFlag Flag;
    }

    public class SecondMinMaxPlayer : IAIPlayer
    {
        private StoneType _myStone;
        private Random _fixedRand = new(808);
        private Random _rand = new();
        private float _defenseWeight = 1.2f;
        private int _maxDepth = 3;
        private int _maxCandidates = 15;
        private ulong[,,] _zobristTable = new ulong[15, 15, 3];
        private Dictionary<ulong, TTEntry> _transpositionTable = new();

        public SecondMinMaxPlayer(StoneType myStone)
        {
            _myStone = myStone;
            InitZobristTable();
        }

        private void InitZobristTable()
        {
            byte[] buffer = new byte[8];

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    _fixedRand.NextBytes(buffer);
                    _zobristTable[x, y, 1] = BitConverter.ToUInt64(buffer, 0); // Black

                    _fixedRand.NextBytes(buffer);
                    _zobristTable[x, y, 2] = BitConverter.ToUInt64(buffer, 0); // White
                }
            }
        }


        public void RequestPlay(StoneType[,] board, Action<int, int> onDecisionMade)
        {

            // 초기화
            _transpositionTable.Clear();

            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;
            ulong currentHash = ComputeZobristHash(board);

            List<(int x, int y)> validPosList = GomokuLogic.GetValidPos(board, _myStone);
            List<(int x, int y)> bestMoves = new();

            foreach (var pos in validPosList)
            {
                board[pos.x, pos.y] = _myStone;
                ulong nextHash = currentHash ^ _zobristTable[pos.x, pos.y, (_myStone == StoneType.Black) ? 1 : 2];
                int score = MiniMax(board, _maxDepth - 1, alpha, beta, false, pos.x, pos.y, nextHash);
                board[pos.x, pos.y] = StoneType.None;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(pos);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(pos);
                }

                alpha = Math.Max(alpha, bestScore);
            }

            var finalMove = bestMoves[_rand.Next(bestMoves.Count)];
            onDecisionMade?.Invoke(finalMove.x, finalMove.y);
        }

        public void HandleInput(int x, int y) { }

        private int MiniMax(StoneType[,] board, int remainingDepth, int alpha, int beta,
                    bool isMaximizing, int lastX, int lastY, ulong currentHash)
        {
            StoneType lastStone = board[lastX, lastY];
            if (GomokuLogic.CheckWin(board, lastX, lastY, lastStone))
            {
                int winScore = (lastStone == _myStone) ? 10000000 + remainingDepth : -10000000 - remainingDepth;
                return winScore;
            }
            if (remainingDepth == 0)
                return EvaluateBoard(board, _myStone);
            // 2. Transposition Table 조회: depth까지 검증
            int originalAlpha = alpha;
            if (_transpositionTable.TryGetValue(currentHash, out TTEntry entry) && entry.Depth >= remainingDepth)
            {
                if (entry.Flag == TTFlag.Exact) return entry.Score;
                if (entry.Flag == TTFlag.LowerBound) alpha = Math.Max(alpha, entry.Score);
                else if (entry.Flag == TTFlag.UpperBound) beta = Math.Min(beta, entry.Score);
                if (alpha >= beta) return entry.Score;
            }
            // 3. 탐색
            StoneType opponentStone = (_myStone == StoneType.Black) ? StoneType.White : StoneType.Black;
            List<(int x, int y)> validPosList = GetMiniMaxCandidates(board);
            int bestScore;
            if (isMaximizing)
            {
                bestScore = int.MinValue;
                foreach (var pos in validPosList)
                {
                    board[pos.x, pos.y] = _myStone;
                    ulong nextHash = currentHash ^ _zobristTable[pos.x, pos.y, (_myStone == StoneType.Black) ? 1 : 2];
                    int eval = MiniMax(board, remainingDepth - 1, alpha, beta, false, pos.x, pos.y, nextHash);
                    board[pos.x, pos.y] = StoneType.None;
                    bestScore = Math.Max(bestScore, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
            }
            else
            {
                bestScore = int.MaxValue;
                foreach (var pos in validPosList)
                {
                    board[pos.x, pos.y] = opponentStone;
                    ulong nextHash = currentHash ^ _zobristTable[pos.x, pos.y, (opponentStone == StoneType.Black) ? 1 : 2];
                    int eval = MiniMax(board, remainingDepth - 1, alpha, beta, true, pos.x, pos.y, nextHash);
                    board[pos.x, pos.y] = StoneType.None;
                    bestScore = Math.Min(bestScore, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
            }
            // 4. Transposition Table 저장 (depth, flag 포함)
            TTFlag flag;
            if (bestScore <= originalAlpha) flag = TTFlag.UpperBound;
            else if (bestScore >= beta) flag = TTFlag.LowerBound;
            else flag = TTFlag.Exact;
            _transpositionTable[currentHash] = new TTEntry { Score = bestScore, Depth = remainingDepth, Flag = flag };
            return bestScore;
        }

        private int EvaluateBoard(StoneType[,] board, StoneType myStone)
        {
            StoneType opponentStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;

            int myTotalScore = 0;
            int opponentTotalScore = 0;

            for (int i = 0; i < BoardSize; i++)
            {
                // 가로 검사
                myTotalScore += EvaluateLine(board, 0, i, 1, 0, myStone);
                opponentTotalScore += EvaluateLine(board, 0, i, 1, 0, opponentStone);

                // 세로 검사
                myTotalScore += EvaluateLine(board, i, 0, 0, 1, myStone);
                opponentTotalScore += EvaluateLine(board, i, 0, 0, 1, opponentStone);
            }

            for (int i = 0; i < BoardSize; i++)
            {
                // 대각선 ↘ 검사 (왼쪽, 위쪽 모서리에서 시작)
                myTotalScore += EvaluateLine(board, 0, i, 1, 1, myStone);
                opponentTotalScore += EvaluateLine(board, 0, i, 1, 1, opponentStone);
                if (i != 0)
                {
                    myTotalScore += EvaluateLine(board, i, 0, 1, 1, myStone);
                    opponentTotalScore += EvaluateLine(board, i, 0, 1, 1, opponentStone);
                }

                // 대각선 ↙ 검사 (오른쪽, 위쪽 모서리에서 시작)
                myTotalScore += EvaluateLine(board, BoardSize - 1, i, -1, 1, myStone);
                opponentTotalScore += EvaluateLine(board, BoardSize - 1, i, -1, 1, opponentStone);
                if (i != 0)
                {
                    myTotalScore += EvaluateLine(board, BoardSize - 1 - i, 0, -1, 1, myStone);
                    opponentTotalScore += EvaluateLine(board, BoardSize - 1 - i, 0, -1, 1, opponentStone);
                }
            }

            int totalScore = myTotalScore - (int)(opponentTotalScore * _defenseWeight);
            return totalScore;
        }

        private int EvaluateLine(StoneType[,] board, int startX, int startY, int dirX, int dirY, StoneType myStone)
        {
            int totalLineScore = 0;
            StoneType opponentStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;

            // 전체 라인을 쭉 밀고 나가는 방식 (Global Evaluation)
            int currentX = startX;
            int currentY = startY;

            while (GomokuLogic.IsValidPos(currentX, currentY))
            {
                // 5칸 윈도우 스캔
                int count = 0;
                int emptyCount = 0;
                bool isBlocked = false;

                for (int j = 0; j < 5; j++)
                {
                    int nx = currentX + j * dirX;
                    int ny = currentY + j * dirY;

                    if (!GomokuLogic.IsValidPos(nx, ny) || board[nx, ny] == opponentStone)
                    {
                        isBlocked = true;
                        break;
                    }
                    if (board[nx, ny] == myStone) count++;
                    else if (board[nx, ny] == StoneType.None) emptyCount++;
                }

                if (!isBlocked && count > 0)
                {
                    // count==5는 CheckWin이 처리. EvaluateBoard는 리프 노드 평가용이라 도달 불가.
                    // 승리 점수(10,000,000)와 충돌하지 않도록 1에서 100,000 범위 유지
                    if (count == 4 && emptyCount == 1) totalLineScore += 100000;
                    else if (count == 3 && emptyCount == 2) totalLineScore += 10000;
                    else if (count == 2 && emptyCount == 3) totalLineScore += 100;
                }

                currentX += dirX;
                currentY += dirY;
            }

            return totalLineScore;
        }

        private ulong ComputeZobristHash(StoneType[,] board)
        {
            ulong hash = 0;

            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    StoneType stone = board[x, y];
                    if (stone != StoneType.None)
                    {
                        int stoneIndex = (stone == StoneType.Black) ? 1 : 2;
                        hash ^= _zobristTable[x, y, stoneIndex];
                    }
                }
            }

            return hash;
        }

        private List<(int x, int y)> GetMiniMaxCandidates(StoneType[,] board)
        {
            HashSet<(int, int)> candidates = new();
            // 기존 돌 주변 2칸의 빈 자리를 후보로 수집
            for (int x = 0; x < BoardSize; x++)
            {
                for (int y = 0; y < BoardSize; y++)
                {
                    if (board[x, y] == StoneType.None) continue; // 빈 칸은 건너뜀
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;
                            if (GomokuLogic.IsValidPos(nx, ny) && board[nx, ny] == StoneType.None)
                                candidates.Add((nx, ny));
                        }
                    }
                }
            }
            if (candidates.Count == 0)
                return new List<(int, int)> { (BoardSize / 2, BoardSize / 2) };
            // 인접 돌이 많은 자리(위협적인 자리)를 먼저 탐색 → Alpha-Beta 컷오프 극대화
            return candidates
                .OrderByDescending(pos => GomokuLogic.CountAdjacentStones(board, pos.Item1, pos.Item2))
                .Take(_maxCandidates)
                .ToList();
        }
    }
}
