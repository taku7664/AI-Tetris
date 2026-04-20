using System.Collections.Generic;
using UnityEngine;

namespace ChatGpt
{
    /// <summary>
    /// Generic object pool that recycles MonoBehaviour components to avoid runtime allocation.
    /// </summary>
    public class ChatGpt_ObjectPool<T> where T : MonoBehaviour
    {
        private readonly Queue<T> _pool = new Queue<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public ChatGpt_ObjectPool(T prefab, Transform parent, int initialSize = 20)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialSize; i++)
                _pool.Enqueue(CreateNew());
        }

        /// <summary>Retrieve an object from the pool (or create a new one if empty).</summary>
        public T Get()
        {
            T obj = (_pool.Count > 0) ? _pool.Dequeue() : CreateNew();
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>Return an object to the pool.</summary>
        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        private T CreateNew()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }
    }
}
