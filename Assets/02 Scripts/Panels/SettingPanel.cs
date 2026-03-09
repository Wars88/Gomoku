using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Won
{
    public class SettingPanel : BasePanel
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Slider _bgmSlider;
        [SerializeField] private Slider _sfxSlider;

        public event Action<float> OnBgmVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        
        // TODO: 사운드 매니저와 연동
        private void Awake()
        {
           _closeButton.onClick.AddListener(HandleCloseButton);
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(HandleCloseButton);
        }

        public override void Show()
        {
            base.Show();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }

        public override void Hide()
        {
            transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => base.Hide());
        }

        private void HandleCloseButton()
        {
            Hide();
        }
    }
}