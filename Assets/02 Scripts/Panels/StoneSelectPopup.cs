using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Won.Constants;

namespace Won
{
    public class StoneSelectPopup : BasePopup
    {
        [SerializeField] private Button _blackButton;
        [SerializeField] private Button _whiteButton;

        public event Action<StoneType> OnStoneSelected;

        private void Awake()
        {
            _blackButton.onClick.AddListener(() => Select(StoneType.Black));
            _whiteButton.onClick.AddListener(() => Select(StoneType.White));
        }

        private void OnDestroy()
        {
            _blackButton.onClick.RemoveAllListeners();
            _whiteButton.onClick.RemoveAllListeners();
        }

        private void Select(StoneType stone)
        {
            Hide(() => OnStoneSelected?.Invoke(stone));
        }

        public override void Show(Action onClosed = null)
        {
            gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        public override void Hide(Action onComplete = null)
        {
            transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}
