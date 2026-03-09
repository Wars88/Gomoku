using System;
using static Won.Constants;


namespace Won
{
    public class HumanPlayer : IHumanPlayer
    {
        private Action<int, int> _onDecisionMade;
        private int _hoveredX = -1;
        private int _hoveredY = -1;

        public event Action<int, int> OnHoverChanged;

        public void RequestPlay(StoneType[,] board, Action<int, int> onDecisionMade)
        {
            _onDecisionMade = onDecisionMade;
            _hoveredX = -1;
            _hoveredY = -1;
        }

        public void HandleHover(int x, int y)
        {
            _hoveredX = x;
            _hoveredY = y;
            OnHoverChanged?.Invoke(x, y);
        }

        public void HandleInput(int x, int y)
        {
            if (_onDecisionMade == null) return;

            if (_hoveredX == x && _hoveredY == y)
            {
                // 같은 자리 두 번 클릭 → 착수
                var callback = _onDecisionMade;
                _onDecisionMade = null;
                _hoveredX = _hoveredY = -1;
                callback.Invoke(x, y);
                OnHoverChanged?.Invoke(-1, -1);
            }
            else
            {
                HandleHover(x, y);
            }
        }
    }
}