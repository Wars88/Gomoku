using TMPro;
using UnityEngine;
using System;

namespace Won
{
    public abstract class BasePopup : MonoBehaviour
    {
        public virtual void Show(Action action = null)
        {
            gameObject.SetActive(true);
            action?.Invoke();
        }
        public virtual void Hide(Action action = null)
        {
            gameObject.SetActive(false);
            action?.Invoke();
        }
    }
}