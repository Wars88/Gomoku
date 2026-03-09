using System;
using System.Collections.Generic;
using System.Linq;
using static Won.Constants;

namespace Won
{
    public static class GomokuLogic
    {
        public static int[] Dx = { 1, 0, 1, 1 };
        public static int[] Dy = { 0, 1, 1, -1 };

        public static bool IsForbidden(StoneType[,] board, int x, int y, StoneType stone)
        {
            board[x, y] = stone;

            // [렌주룰] 5목을 완성하는 수는 쌍삼/쌍사가 동시에 발생해도 금수가 아니라 승리.
            // 반드시 다른 금수 판단보다 먼저 체크해야 한다.
            for (int i = 0; i < 4; i++)
            {
                int stoneCount = 1 + CountStones(board, x, y, Dx[i], Dy[i], stone)
                                   + CountStones(board, x, y, -Dx[i], -Dy[i], stone);
                if (stoneCount == 5)
                {
                    board[x, y] = StoneType.None;
                    return false;
                }
            }

            bool isForbidden = false;
            int openThreeCount = 0;
            int fourCount = 0;

            for (int i = 0; i < 4; i++)
            {
                int stoneCount = 1 + CountStones(board, x, y, Dx[i], Dy[i], stone)
                                   + CountStones(board, x, y, -Dx[i], -Dy[i], stone);

                if (stoneCount > 5)
                {
                    isForbidden = true;
                    break;
                }
                if (CheckOpenThree(board, x, y, Dx[i], Dy[i], stone))
                    openThreeCount++;
                if (CheckFour(board, x, y, Dx[i], Dy[i], stone))
                    fourCount++;

                if (openThreeCount >= 2 || fourCount >= 2)
                {
                    isForbidden = true;
                    break;
                }
            }

            board[x, y] = StoneType.None;
            return isForbidden;
        }

        public static bool CheckOpenThree(StoneType[,] board, int x, int y, int dx, int dy, StoneType stone)
        {
            for (int i = -5; i <= 0; i++)
            {
                int myCount = 0;
                int emptyCount = 0;
                bool validWindow = true;

                for (int j = 0; j < 6; j++)
                {
                    int nx = x + (i + j) * dx;
                    int ny = y + (i + j) * dy;

                    if (IsValidPos(nx, ny) == false)
                    {
                        validWindow = false;
                        break;
                    }

                    if (board[nx, ny] == stone) myCount++;
                    else if (board[nx, ny] == StoneType.None) emptyCount++;
                    else
                    {
                        validWindow = false;
                        break;
                    }
                }

                if (validWindow && myCount == 3 && emptyCount == 3)
                {
                    int startNx = x + i * dx;
                    int startNy = y + i * dy;
                    int endNx = x + (i + 5) * dx;
                    int endNy = y + (i + 5) * dy;

                    if (board[startNx, startNy] == StoneType.None && board[endNx, endNy] == StoneType.None)
                        return true;
                }
            }
            return false;
        }

        public static bool CheckFour(StoneType[,] board, int x, int y, int dx, int dy, StoneType stone)
        {
            for (int i = -4; i <= 0; i++)
            {
                int myCount = 0;
                int emptyCount = 0;
                bool validWindow = true;

                for (int j = 0; j < 5; j++)
                {
                    int nx = x + (i + j) * dx;
                    int ny = y + (i + j) * dy;

                    if (IsValidPos(nx, ny) == false)
                    {
                        validWindow = false;
                        break;
                    }

                    if (board[nx, ny] == stone) myCount++;
                    else if (board[nx, ny] == StoneType.None) emptyCount++;
                    else
                    {
                        validWindow = false;
                        break;
                    }
                }

                if (validWindow && myCount == 4 && emptyCount == 1)
                    return true;
            }
            return false;
        }

        public static int CountStones(StoneType[,] board, int startX, int startY, int dirX, int dirY, StoneType stoneType)
        {
            int count = 0;
            int nx = startX + dirX;
            int ny = startY + dirY;

            while (IsValidPos(nx, ny) && board[nx, ny] == stoneType)
            {
                count++;
                nx += dirX;
                ny += dirY;
            }
            return count;
        }

        public static bool CheckWin(StoneType[,] board, int x, int y, StoneType stoneType)
        {
            if (IsValidPos(x, y) == false || board[x, y] != stoneType)
                return false;

            for (int i = 0; i < 4; i++)
            {
                int count = 1;

                count += CountStones(board, x, y, Dx[i], Dy[i], stoneType); // 정방향
                count += CountStones(board, x, y, -Dx[i], -Dy[i], stoneType); // 역방향

                // 흑은 정확히 5개만 승리 (육목은 금수). 백은 5개 이상 모두 승리.
                if (stoneType == StoneType.Black && count == 5) return true;
                if (stoneType == StoneType.White && count >= 5) return true;
            }
            return false;
        }

        public static bool IsDraw(StoneType[,] board)
        {
            for (int x = 0; x < BoardSize; x++)
                for (int y = 0; y < BoardSize; y++)
                    if (board[x, y] == StoneType.None)
                        return false; // 빈 칸이 하나라도 있으면 무승부 아님
            return true;
        }

        public static List<(int, int)> GetValidPos(StoneType[,] board, StoneType stoneType)
        {
            HashSet<(int, int)> validPosList = new();

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

                                if (IsValidPos(nx, ny) && board[nx, ny] == StoneType.None)
                                {
                                    // 비어 있는 칸 확인 후, 흑돌일 경우에만 금수 필터 적용
                                    if (stoneType == StoneType.Black && IsForbidden(board, nx, ny, StoneType.Black))
                                        continue;

                                    validPosList.Add((nx, ny));
                                }
                            }
                        }
                    }
                }
            }

            if (validPosList.Count == 0)
            {
                return new List<(int, int)> { (BoardSize / 2, BoardSize / 2) };
            }

            return validPosList.OrderByDescending(
                pos => CountAdjacentStones(board, pos.Item1, pos.Item2))
                .ToList();
        }

        // 주변에 돌이 없는 완전한 빈 공간은 탐색 우선순위 낮춤
        public static int CountAdjacentStones(StoneType[,] board, int x, int y)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; 
                    int nx = x + dx, ny = y + dy;
                    if (IsValidPos(nx, ny) && board[nx, ny] != StoneType.None)
                        count++;
                }
            return count;
        }

        public static bool IsValidPos(int x, int y)
        {
            return x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;
        }
    }
}