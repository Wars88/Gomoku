using UnityEngine;

namespace Won
{
    public abstract class BasePanel : MonoBehaviour
    {
        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}