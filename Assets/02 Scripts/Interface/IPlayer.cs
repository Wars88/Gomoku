using System;
using static Won.Constants;

namespace Won
{
    public interface IPlayer
    {
        void RequestPlay(StoneType[,] board, Action<int, int> onDecisionMade);
    }
}