using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Won.Constants;

namespace Won
{
    public class GamePanel : BasePanel
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _undoButton;

        [SerializeField] private Image _timer;
        [SerializeField] private Image _leftPlayerImage;
        [SerializeField] private Image _rightPlayerImage;

        [SerializeField] private TextMeshProUGUI _leftPlayerNameText;
        [SerializeField] private TextMeshProUGUI _rightPlayerNameText;

        [SerializeField] private CellButton[] _boardCellList;

        public event Action OnBackClicked;
        public event Action OnSettingsClicked;
        public event Action OnUndoClicked;
        public event Action<int, int> OnBoardClicked;

        private void OnDestroy()
        {
            for (int i = 0; i < _boardCellList.Length; i++)
            {
                if (_boardCellList[i] != null)
                    _boardCellList[i].OnCellClicked -= HandleBoardClicked;
            }

            if (_backButton != null) _backButton.onClick.RemoveListener(HandleBackButton);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(HandleSettingsButton);
            if (_undoButton != null) _undoButton.onClick.RemoveListener(HandleUndoButton);
        }

        public void Init()
        {
            for (int i = 0; i < _boardCellList.Length; i++)
            {
                int x = i % BoardSize;
                int y = i / BoardSize;

                _boardCellList[i].SetPosition(x, y);
                _boardCellList[i].OnCellClicked += HandleBoardClicked;
            }

            _backButton.onClick.AddListener(HandleBackButton);
            _settingsButton.onClick.AddListener(HandleSettingsButton);
            _undoButton.onClick.AddListener(HandleUndoButton);

            GameUIManager.Instance.HideSettingPanel();
        }

        /// <summary>보드 시각 초기화 — 돌/hover/마커 이미지 전부 끔</summary>
        public void ResetBoard()
        {
            for (int i = 0; i < _boardCellList.Length; i++)
            {
                int x = i % BoardSize;
                int y = i / BoardSize;
                _boardCellList[i].SetPosition(x, y); // SetPosition 내부에서 이미지 초기화
            }
        }

        public void UpdateTimer(float fillAmount) // 1 -> 0
        {
            _timer.fillAmount = fillAmount;
        }

        public void RefreshBoardCell(int x, int y, StoneType stoneType)
        {
            _boardCellList[y * BoardSize + x].SetStoneImage(stoneType);
        }

        public void RefreshForbiddenImages(StoneType currentTurn, List<(int x, int y)> forbiddenPositions)
        {
            for (int i = 0; i < _boardCellList.Length; i++)
            {
                _boardCellList[i].SetForbiddenImage(false);
            }

            if (currentTurn == StoneType.Black)
            {
                foreach (var pos in forbiddenPositions)
                {
                    _boardCellList[pos.y * BoardSize + pos.x].SetForbiddenImage(true);
                }
            }
        }

        public void RefreshMoveMarker(int x, int y, bool isPrePos)
        {
            _boardCellList[y * BoardSize + x].SetPrePosImage(isPrePos);
        }

        public void RefreshCellHover(int x, int y, bool isHovering)
        {
            _boardCellList[y * BoardSize + x].SetHoverImage(isHovering);
        }

        public void RefreshUserInfo(PlayerProfile leftProfile, PlayerProfile rightProfile)
        {
            _leftPlayerNameText.text = leftProfile.DisplayName;
            _rightPlayerNameText.text = rightProfile.DisplayName;

            _leftPlayerImage.sprite = leftProfile.ProfileImage;
            _rightPlayerImage.sprite = rightProfile.ProfileImage;
        }

        private void HandleBackButton() => OnBackClicked?.Invoke();
        private void HandleSettingsButton() => OnSettingsClicked?.Invoke();
        private void HandleUndoButton() => OnUndoClicked?.Invoke();
        private void HandleBoardClicked(int x, int y) => OnBoardClicked?.Invoke(x, y);
    }
}