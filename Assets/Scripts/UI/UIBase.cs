using UnityEngine;

namespace STSLite.UI
{
    public abstract class UIBase : MonoBehaviour
    {
        public virtual void Begin() { }
        public virtual void Resume() { }
        public virtual void Pause() { }
        public virtual void Finish() { }
    }
}
