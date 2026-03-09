using System;
using System.Collections.Generic;
using static Won.Constants;

namespace Won
{
    public class RandomAI : IAIPlayer
    {
        private Random _random;

        public RandomAI()
        {
            _random = new Random();
        }

        public void RequestPlay(StoneType[,] board, Action<int, int> onDecisionMade)
        {
            List<(int x, int y)> emptyCellList = new();

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (board[i, j] == StoneType.None)
                        emptyCellList.Add((i, j));
                }
            }

            if (emptyCellList.Count == 0)
            {
                onDecisionMade?.Invoke(-1, -1);
                return;
            }

            int randomIndex = _random.Next(emptyCellList.Count);
            var randomCell = emptyCellList[randomIndex];
            onDecisionMade?.Invoke(randomCell.x, randomCell.y);
        }
    }
}