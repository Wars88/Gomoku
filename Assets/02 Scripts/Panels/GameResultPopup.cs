using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Won.Constants;

namespace Won
{
    public class GameResultPopup : BasePopup
    {
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;

        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _progressText;

        public event Action OnRetryClicked;

        private void Awake()
        {
            _retryButton?.onClick.AddListener(() => OnRetryClicked?.Invoke());
            _mainMenuButton?.onClick.AddListener(() => GameManager.Instance.GotoMainMenu());
        }

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveAllListeners();
            _mainMenuButton?.onClick.RemoveAllListeners();
        }

        public void Show(GameResult result, PlayerProfile profile)
        {
            _resultText.text = result.ResultLabel;
            _scoreText.text = $"점수 {(result.ScoreGained >= 0 ? "+" : "")}{result.ScoreGained}점";
            _rankText.text = $"{profile.Rank}급";
            _progressText.text = $"다음 급수까지\n{profile.PointsInRank} / {PointsPerRank}";

            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        public override void Hide(Action onComplete = null)
        {
            transform.DOScale(Vector3.zero, 0.25f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}
