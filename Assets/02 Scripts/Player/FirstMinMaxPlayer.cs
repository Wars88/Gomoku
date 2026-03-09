using System;
using System.Collections.Generic;
using static Won.Constants;

namespace Won
{
    public class FirstMinMaxPlayer : IAIPlayer
    {
        private StoneType _myStone;
        private Random _rand = new();
        private float _defenseWeight = 1.2f;
        private int _maxDepth = 3;

        public FirstMinMaxPlayer(StoneType myStone)
        {
            _myStone = myStone;
        }

        public void RequestPlay(StoneType[,] board, Action<int, int> onDecisionMade)
        {
            int bestScore = int.MinValue;
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            List<(int x, int y)> validPosList = GetValidPos(board, _myStone);
            List<(int x, int y)> bestMoves = new List<(int x, int y)>();

            foreach (var pos in validPosList)
            {
                board[pos.x, pos.y] = _myStone;
                int score = MiniMax(board, 1, alpha, beta, false, pos.x, pos.y);
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

        private int MiniMax(StoneType[,] board, int depth, int alpha, int beta, bool isMaximizing, int lastX, int lastY)
        {
            if (depth == _maxDepth)
            {
                return EvaluateBoard(board, lastX, lastY, _myStone);
            }

            List<(int x, int y)> validPosList = GetValidPos(board, _myStone);
            StoneType opponentStone = (_myStone == StoneType.Black) ? StoneType.White : StoneType.Black;

            if (isMaximizing)
            {
                int maxEval = int.MinValue;
                foreach (var pos in validPosList)
                {
                    board[pos.x, pos.y] = _myStone;
                    int eval = MiniMax(board, depth + 1, alpha, beta, false, pos.x, pos.y);
                    board[pos.x, pos.y] = StoneType.None;
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var pos in validPosList)
                {
                    board[pos.x, pos.y] = opponentStone;
                    int eval = MiniMax(board, depth + 1, alpha, beta, true, pos.x, pos.y);
                    board[pos.x, pos.y] = StoneType.None;
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        private List<(int, int)> GetValidPos(StoneType[,] board, StoneType stoneType)
        {
            HashSet<(int, int)> _validPosList = new();

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    if (board[x, y] != StoneType.None)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            for (int dx = -2; dx <= 2; dx++)
                            {
                                int ny = y + dy;
                                int nx = x + dx;
                                if (GomokuLogic.IsValidPos(nx, ny))
                                {
                                    if (stoneType == StoneType.Black && GomokuLogic.IsForbidden(board, nx, ny, StoneType.Black))
                                    {
                                        continue;
                                    }

                                    if (board[nx, ny] == StoneType.None)
                                        _validPosList.Add((nx, ny));
                                }
                            }
                        }
                    }
                }
            }

            if (_validPosList.Count == 0)
            {
                return new List<(int, int)> { (BoardSize / 2, BoardSize / 2) };
            }

            return new List<(int, int)>(_validPosList);
        }

        private int EvaluateBoard(StoneType[,] board, int x, int y, StoneType myStone)
        {
            StoneType opponentStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;

            int myTotalScore = 0;
            int opponentTotalScore = 0;

            for (int dir = 0; dir < 4; dir++)
            {
                myTotalScore += EvaluateLine(board, x, y, GomokuLogic.Dx[dir], GomokuLogic.Dy[dir], myStone);
                opponentTotalScore += EvaluateLine(board, x, y, GomokuLogic.Dx[dir], GomokuLogic.Dy[dir], opponentStone);
            }

            int totalScore = myTotalScore - (int)(opponentTotalScore * _defenseWeight);


            return totalScore;
        }

        private int EvaluateLine(StoneType[,] board, int startX, int startY, int dirX, int dirY, StoneType myStone)
        {
            int totalLineScore = 0;
            StoneType opponentStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;

            // 1. 방금 놓은 돌을 중심으로 -4칸 ~ +4칸 (총 9칸)을 검사
            for (int i = -4; i <= 0; i++)
            {
                int count = 0;
                int emptyCount = 0;
                bool isBlocked = false;

                for (int j = 0; j < 5; j++)
                {
                    int nx = startX + (i + j) * dirX;
                    int ny = startY + (i + j) * dirY;

                    if (!GomokuLogic.IsValidPos(nx, ny))
                    {
                        isBlocked = true;
                        break;
                    }
                    if (board[nx, ny] == opponentStone)
                    {
                        isBlocked = true;
                        break;
                    }
                    else if (board[nx, ny] == myStone) count++;
                    else if (board[nx, ny] == StoneType.None) emptyCount++;
                }

                if (!isBlocked)
                {
                    if (count == 5) totalLineScore += 10000000;
                    else if (count == 4 && emptyCount == 1) totalLineScore += 1000000; // 4목 (끊긴 4목 포함)
                    else if (count == 3 && emptyCount == 2) totalLineScore += 10000;   // 3목 (끊긴 3목 포함)
                    else if (count == 2 && emptyCount == 3) totalLineScore += 100;     // 2목
                }
            }
            return totalLineScore;
        }
    }

}