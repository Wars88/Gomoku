using System;
using System.Collections.Generic;
using static Won.Constants;

namespace Won
{
    public class GomokuModel : IGomokuModel
    {
        public event Action<int, int, StoneType> OnBoardChanged;
        
        public StoneType CurrentTurn { get; private set; }
        public StoneType[,] GetBoardState() { return _board; }

        private StoneType[,] _board;

        public GomokuModel()
        {
            _board = new StoneType[BoardSize, BoardSize];
            CurrentTurn = StoneType.Black;
        }

        public bool CanPlaceStone(int x, int y)
        {
            if (GomokuLogic.IsValidPos(x, y) == false)
                return false;

            if (_board[x, y] != StoneType.None)
                return false;

            if (CurrentTurn == StoneType.Black && GomokuLogic.IsForbidden(_board,x, y, StoneType.Black))
                return false;

            return true;
        }

        public void PlaceStone(int x, int y)
        {
            _board[x, y] = CurrentTurn;
            OnBoardChanged?.Invoke(x, y, CurrentTurn);
            CurrentTurn = CurrentTurn == StoneType.Black ? StoneType.White : StoneType.Black;
        }

        public void UndoStone(int x, int y)
        {
            _board[x, y] = StoneType.None;
            OnBoardChanged?.Invoke(x, y, StoneType.None);
            CurrentTurn = CurrentTurn == StoneType.Black ? StoneType.White : StoneType.Black;
        }

        public List<(int x, int y)> GetForbiddenPositions()
        {
            List<(int, int)> forbiddenList = new ();
            if (CurrentTurn == StoneType.White) return forbiddenList;

            for (int y = 0; y < BoardSize; y++)
            {
                for (int x = 0; x < BoardSize; x++)
                {
                    if (_board[x, y] == StoneType.None)
                    {
                        if (GomokuLogic.IsForbidden(_board, x, y, StoneType.Black))
                            forbiddenList.Add((x, y));
                    }
                }
            }

            return forbiddenList;
        }
    }
}