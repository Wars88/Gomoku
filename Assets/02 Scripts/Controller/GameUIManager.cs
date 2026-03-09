using System;
using UnityEngine;
using static Won.Constants;

namespace Won
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [SerializeField] private SettingPanel _settingPanel;
        [SerializeField] private GameResultPopup _resultPopup;
        [SerializeField] private StoneSelectPopup _stoneSelectPopup;
        [SerializeField] private GomokuPresenter _presenter;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Setting ──────────────────────────────────────
        public void ShowSettingPanel() => _settingPanel.Show();
        public void HideSettingPanel() => _settingPanel.Hide();

        // ── Result ───────────────────────────────────────
        public void ShowResult(GameResult result, PlayerProfile profile)
        {
            _resultPopup.OnRetryClicked -= HandleRetry;
            _resultPopup.OnRetryClicked += HandleRetry;
            _resultPopup.Show(result, profile);
        }

        private void HandleRetry()
        {
            _resultPopup.Hide();
            ShowStoneSelectPopup(_presenter.Restart);
        }

        // ── Stone Select ─────────────────────────────────
        public void ShowStoneSelectPopup(Action<StoneType> onSelected)
        {
            void Handler(StoneType stone)
            {
                _stoneSelectPopup.OnStoneSelected -= Handler;
                onSelected?.Invoke(stone);
            }
            _stoneSelectPopup.OnStoneSelected -= Handler; // 중복 방지
            _stoneSelectPopup.OnStoneSelected += Handler;
            _stoneSelectPopup.Show();
        }
    }
}
