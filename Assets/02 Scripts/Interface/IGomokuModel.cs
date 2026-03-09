using System;
using System.Collections.Generic;
using static Won.Constants;

namespace Won
{
    public interface IGomokuModel
    {
        event Action<int, int, StoneType> OnBoardChanged;

        StoneType CurrentTurn { get; }
        StoneType[,] GetBoardState();

        List<(int x, int y)> GetForbiddenPositions();
        bool CanPlaceStone(int x, int y);
        void PlaceStone(int x, int y);
        void UndoStone(int x, int y);

    }
}