using System.Collections.Generic;
using UnityEngine;
using static Won.Constants;

namespace Won
{


    public class GomokuPresenter : MonoBehaviour
    {
        [SerializeField] private GamePanel _gamePanel;
        [SerializeField] private float _turnTimeLimit = 30f;

        private IPlayer _whitePlayer;
        private IPlayer _blackPlayer;

        //private bool _isWaiting;    
        private IGomokuModel _model;
        private Stack<ICommand> _commandStack = new();
        private bool _isTimerRunning;
        private float _turnTimer;
        private int _hoveredX = -1;
        private int _hoveredY = -1;
        private int _lastStonePosX = -1;
        private int _lastStonePosY = -1;
        private StoneType _myStoneColor = StoneType.Black;  // TODO: 모드에 따라 다르게 설정

        private void Start()
        {
            RegisterPanelEvents();
            // 게임 시작 시 돌 색 선택
            GameUIManager.Instance.ShowStoneSelectPopup(SetupGame);
        }

        private void Update()
        {
            if (!_isTimerRunning) return;

            _turnTimer -= Time.deltaTime;
            HandleTimer(_turnTimer);

            if (_turnTimer <= 0f)
            {
                TimeOver();
                _turnTimer = _turnTimeLimit;
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
                _model.OnBoardChanged -= HandleBoardChanged;

            if (_whitePlayer is IHumanPlayer humanWhite)
                humanWhite.OnHoverChanged -= HandleHoverChanged;
            if (_blackPlayer is IHumanPlayer humanBlack)
                humanBlack.OnHoverChanged -= HandleHoverChanged;

            if (_gamePanel != null)
            {
                _gamePanel.OnBoardClicked -= HandleViewClicked;
                _gamePanel.OnUndoClicked -= HandleUndoClicked;
                _gamePanel.OnSettingsClicked -= HandleSettingsClicked;
                _gamePanel.OnBackClicked -= HandleBackButton;
            }
        }

        private void RegisterPanelEvents()
        {
            _gamePanel.Init();
            _gamePanel.OnBoardClicked += HandleViewClicked;
            _gamePanel.OnUndoClicked += HandleUndoClicked;
            _gamePanel.OnSettingsClicked += HandleSettingsClicked;
            _gamePanel.OnBackClicked += HandleBackButton;
        }

        private void SetupGame(StoneType myStone)
        {
            // 이전 모델 이벤트 해제
            if (_model != null)
                _model.OnBoardChanged -= HandleBoardChanged;
            if (_whitePlayer is IHumanPlayer hw) hw.OnHoverChanged -= HandleHoverChanged;
            if (_blackPlayer is IHumanPlayer hb) hb.OnHoverChanged -= HandleHoverChanged;

            // 상태 초기화
            _commandStack.Clear();
            _isTimerRunning = false;
            _hoveredX = _hoveredY = -1;
            _lastStonePosX = _lastStonePosY = -1;
            _myStoneColor = myStone;

            // 새 모델
            _model = new GomokuModel();
            _model.OnBoardChanged += HandleBoardChanged;

            // 플레이어 구성
            _blackPlayer = myStone == StoneType.Black
                ? (IPlayer)new HumanPlayer()
                : new SecondMinMaxPlayer(StoneType.Black);
            _whitePlayer = myStone == StoneType.White
                ? (IPlayer)new HumanPlayer()
                : new SecondMinMaxPlayer(StoneType.White);

            if (_blackPlayer is IHumanPlayer blackHuman) blackHuman.OnHoverChanged += HandleHoverChanged;
            if (_whitePlayer is IHumanPlayer whiteHuman) whiteHuman.OnHoverChanged += HandleHoverChanged;

            _gamePanel.ResetBoard();
            StartNextTurn();
        }

        /// <summary>결과 팝업의 다시하기 → UIManager가 호출</summary>
        public void Restart()
        {
            GameUIManager.Instance.ShowStoneSelectPopup(SetupGame);
        }

        private void StartNextTurn()
        {
            _isTimerRunning = true;
            _turnTimer = _turnTimeLimit;

            // _isWaiting = true;
            _gamePanel.RefreshForbiddenImages(_model.CurrentTurn, _model.GetForbiddenPositions());

            IPlayer currentPlayer = _model.CurrentTurn == StoneType.Black ? _blackPlayer : _whitePlayer;
            currentPlayer.RequestPlay(_model.GetBoardState(), HandleDecisionMade);
        }

        private void RefreshLastMoveMarker()
        {
            if (_lastStonePosX != -1)
            {
                _gamePanel.RefreshMoveMarker(_lastStonePosX, _lastStonePosY, false);
                _lastStonePosX = -1;
                _lastStonePosY = -1;
            }

            if (_commandStack.Count == 0) return;

            if (_commandStack.Peek() is not StoneCommand last) return;

            _lastStonePosX = last.X;
            _lastStonePosY = last.Y;
            _gamePanel.RefreshMoveMarker(_lastStonePosX, _lastStonePosY, true);
        }

        private void HandleGameEnd(StoneType winner)
        {
            _isTimerRunning = false;

            PlayerProfile myProfile = GameManager.Instance.MyProfile;

            // 승리면 누적 승리 수 증가 (계산 프로퍼티인 Rank/PointsInRank는 자동 반영)
            if (winner == _myStoneColor) myProfile.TotalWins++;
            else if (winner != StoneType.None) myProfile.TotalLosses++;

            var result = new GameResult
            {
                Winner = winner,
                MyStoneColor = _myStoneColor,
                TotalMoves = _commandStack.Count
            };

            GameManager.Instance.SaveResult(result);
            GameUIManager.Instance.ShowResult(result, myProfile);
        }

        private void HandleTimer(float timer)
        {
            _gamePanel.UpdateTimer(timer / _turnTimeLimit);
        }

        private void TimeOver()
        {
            List<(int x, int y)> pos = GomokuLogic.GetValidPos(_model.GetBoardState(), _model.CurrentTurn);
            if (pos != null && pos.Count > 0)
            {
                var randomIndex = Random.Range(0, pos.Count);
                HandleDecisionMade(pos[randomIndex].x, pos[randomIndex].y);
            }
        }

        private void HandleBoardChanged(int x, int y, StoneType stoneType)
        {
            _gamePanel.RefreshBoardCell(x, y, stoneType);
        }

        private void HandleDecisionMade(int x, int y)
        {
            if (!_model.CanPlaceStone(x, y))
            {
                Debug.Log("둘 수 없는 위치");
                IPlayer currentPlayer = _model.CurrentTurn == StoneType.Black ? _blackPlayer : _whitePlayer;
                currentPlayer.RequestPlay(_model.GetBoardState(), HandleDecisionMade);
                return;
            }

            _isTimerRunning = false;

            StoneType preStoneType = _model.CurrentTurn;
            ICommand command = new StoneCommand(_model, x, y);
            command.Execute();
            _commandStack.Push(command);


            if (GomokuLogic.CheckWin(_model.GetBoardState(), x, y, preStoneType))
            {
                RefreshLastMoveMarker();
                HandleGameEnd(preStoneType);
                return;
            }

            if (GomokuLogic.IsDraw(_model.GetBoardState()))
            {
                RefreshLastMoveMarker();
                HandleGameEnd(StoneType.None);
                return;
            }

            RefreshLastMoveMarker();
            StartNextTurn();
        }

        private void HandleViewClicked(int x, int y)
        {
            IPlayer current = _model.CurrentTurn == StoneType.Black ? _blackPlayer : _whitePlayer;
            if (current is IHumanPlayer humanPlayer)
                humanPlayer.HandleInput(x, y);
        }

        private void HandleUndoClicked()
        {
            if (_commandStack.Count == 0) return;

            var command = _commandStack.Pop();
            command.Undo();
            RefreshLastMoveMarker();
            StartNextTurn();
        }

        private void HandleHoverChanged(int x, int y)
        {
            if (_hoveredX != -1) _gamePanel.RefreshCellHover(_hoveredX, _hoveredY, false);

            _hoveredX = x;
            _hoveredY = y;
            if (x != -1) _gamePanel.RefreshCellHover(x, y, true);
        }

        private void HandleSettingsClicked()
        {
            GameUIManager.Instance.ShowSettingPanel();
        }

        private void HandleBackButton()
        {
            // TODO: 모드에 따라 다르게 처리
            GameManager.Instance.GotoMainMenu();
        }
    }
}