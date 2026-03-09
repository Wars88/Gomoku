using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Won.Constants;

namespace Won
{
    public class CellButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _stoneImage;
        [SerializeField] private Image _forbiddenImage;
        [SerializeField] private Image _hoverImage;
        [SerializeField] private Image _prePosImage;

        [SerializeField] private Sprite _blackStone;
        [SerializeField] private Sprite _whiteStone;

        private int _x;
        private int _y;
        public event Action<int, int> OnCellClicked;

        public void SetPosition(int x, int y)
        {
            _x = x;
            _y = y;

            SetStoneImage(StoneType.None);
            SetForbiddenImage(false);
            SetHoverImage(false);
            SetPrePosImage(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCellClicked?.Invoke(_x, _y);
        }

        public void SetStoneImage(StoneType stoneType)
        {
            if (stoneType == StoneType.None)
            {
                _stoneImage.gameObject.SetActive(false);
                return;
            }

            _stoneImage.gameObject.SetActive(true);
            _stoneImage.sprite = stoneType == StoneType.Black ? _blackStone : _whiteStone;
        }

        public void SetForbiddenImage(bool isForbidden)
        {
            _forbiddenImage.gameObject.SetActive(isForbidden);
        }

        public void SetHoverImage(bool isHovering)
        {
            _hoverImage.gameObject.SetActive(isHovering);
        }

        public void SetPrePosImage(bool isPrePos)
        {
            _prePosImage.gameObject.SetActive(isPrePos);
        }
    }
}