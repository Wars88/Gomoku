using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Won
{
    public class Blink : MonoBehaviour
    {
        private Tween _tween;
        private TextMeshProUGUI _text;
        [SerializeField] private float _blinkDuration = 1f;
        [SerializeField] private float _moveYAmount = 20f;
        [SerializeField] private float _moveYDuration = 1f;


        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            if (_text == null)
            {
                Debug.LogError("Blink 컴포넌트가 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
                return;
            }
            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(_text.DOFade(0f, _blinkDuration))
            .Join(_text.transform.DOLocalMoveY(_text.transform.localPosition.y + _moveYAmount, _moveYDuration)
            .SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Yoyo);

            _tween = mySequence;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }
    }
}