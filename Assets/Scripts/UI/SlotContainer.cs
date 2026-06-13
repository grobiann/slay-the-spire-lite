using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace STSLite.UI
{
    [System.Serializable]
    public class SlotContainer<T> : IEnumerable<T> where T : MonoBehaviour
    {
        [SerializeField] private Transform _parent;
        [SerializeField] private T _prefab;

        public int Count => _slots.Count;

        public T this[int index] => _slots[index];

        public IReadOnlyList<T> Slots => _slots;
        private List<T> _slots = new();
        private Queue<T> _pool = new();
        private bool _initialized;

        public void SetSize(int size)
        {
            Initialize();

            if (_slots.Count > size)
            {
                for (int i = _slots.Count - 1; i >= size; i--)
                {
                    ReturnToPool(_slots[i]);
                }
            }
            else if (_slots.Count < size)
            {
                for (int i = _slots.Count; i < size; i++)
                {
                    GetFromPool();
                }
            }
        }

        public void RemoveSlot(T slot)
        {
            ReturnToPool(slot);
        }

        private T GetFromPool()
        {
            T slot;
            if (_pool.Count > 0)
            {
                slot = _pool.Dequeue();
            }
            else
            {
                slot = Object.Instantiate(_prefab, _parent);
            }

            slot.gameObject.SetActive(true);
            slot.transform.SetAsLastSibling();
            _slots.Add(slot);
            return slot;
        }

        private void ReturnToPool(T slot)
        {
            slot.gameObject.SetActive(false);
            _pool.Enqueue(slot);
            _slots.Remove(slot);
        }

        private void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;

                int childCount = _parent.childCount;
                for (int i = childCount - 1; i >= 0; i--)
                {
                    var child = _parent.GetChild(i);
                    var component = child.GetComponent<T>();
                    if (component && component == _prefab)
                    {
                        _prefab.gameObject.SetActive(false);
                        continue;
                    }

                    Object.Destroy(child.gameObject);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _slots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}