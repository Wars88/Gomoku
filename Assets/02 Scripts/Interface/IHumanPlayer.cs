using System;

namespace Won
{
    /// <summary>
    /// 인간 플레이어 전용 인터페이스.
    /// IPlayer의 RequestPlay에 더해 입력 처리와 hover 기능을 정의한다.
    /// </summary>
    public interface IHumanPlayer : IPlayer
    {
        void HandleInput(int x, int y);
        void HandleHover(int x, int y);
        event Action<int, int> OnHoverChanged;
    }
}
