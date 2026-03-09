using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Won
{
    public class MainPanel : BasePanel
    {
        [SerializeField] private Button _gameStartButton;
        [SerializeField] private Button _recordButton;
        [SerializeField] private Button _rankingsButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private TextMeshProUGUI _profileNameText;

        public event Action OnGameStartClicked;
        public event Action OnRecordClicked;
        public event Action OnRankingsClicked;
        public event Action OnSettingsClicked;
        
        private void Awake()
        {
            _gameStartButton.onClick.AddListener(() => HandleGameStart());
            _recordButton.onClick.AddListener(() => HandleRecord());
            _rankingsButton.onClick.AddListener(() => HandleRankings());
            _settingsButton.onClick.AddListener(() => HandleSettings());
        }

        private void OnDestroy()
        {
            if (_gameStartButton != null) _gameStartButton.onClick.RemoveListener(() => HandleGameStart());
            if (_recordButton != null) _recordButton.onClick.RemoveListener(() => HandleRecord());
            if (_rankingsButton != null) _rankingsButton.onClick.RemoveListener(() => HandleRankings());
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(() => HandleSettings());
        }

        public override void Show()
        {
            base.Show();
            // 추가적인 Show 로직이 필요한 경우 여기에 작성
        }

        public override void Hide()
        {
            base.Hide();
            // 추가적인 Hide 로직이 필요한 경우 여기에 작성
        }

        private void HandleGameStart() => OnGameStartClicked?.Invoke();
        private void HandleRecord() => OnRecordClicked?.Invoke();
        private void HandleRankings() => OnRankingsClicked?.Invoke();
        private void HandleSettings() => OnSettingsClicked?.Invoke();
    }
}