using static Won.Constants;

namespace Won
{
    public interface IGomokuView
    {
        void RefreshBoardUI(int x, int y, StoneType stoneType);
        void ShowGameResult(StoneType winner);
    }
}