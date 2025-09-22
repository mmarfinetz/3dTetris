using System.Collections.Generic;
using UnityEngine;

namespace TetrisJenga.Utilities
{
    /// <summary>
    /// Generic object pool for performance optimization
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private Queue<T> pool = new Queue<T>();
        private Transform poolParent;
        private T prefab;
        private int initialSize;
        private int maxSize;
        private int currentCount;

        public ObjectPool(T prefab, int initialSize = 10, int maxSize = 100, Transform parent = null)
        {
            this.prefab = prefab;
            this.initialSize = initialSize;
            this.maxSize = maxSize;

            // Create pool parent
            if (parent == null)
            {
                GameObject poolContainer = new GameObject($"Pool_{typeof(T).Name}");
                poolParent = poolContainer.transform;
            }
            else
            {
                poolParent = parent;
            }

            // Pre-warm pool
            PreWarm();
        }

        private void PreWarm()
        {
            for (int i = 0; i < initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private T CreateNewObject()
        {
            if (currentCount >= maxSize)
            {
                UnityEngine.Debug.LogWarning($"Object pool for {typeof(T).Name} has reached max size of {maxSize}");
                return null;
            }

            T newObject = GameObject.Instantiate(prefab, poolParent);
            newObject.gameObject.SetActive(false);
            currentCount++;
            pool.Enqueue(newObject);
            return newObject;
        }

        /// <summary>
        /// Gets an object from the pool
        /// </summary>
        public T Get()
        {
            T obj = null;

            // Try to get from pool
            while (pool.Count > 0 && obj == null)
            {
                obj = pool.Dequeue();
                if (obj == null)
                {
                    currentCount--;
                }
            }

            // Create new if needed
            if (obj == null)
            {
                obj = CreateNewObject();
                if (obj != null)
                {
                    pool.Dequeue(); // Remove from queue since we just created it
                }
            }

            if (obj != null)
            {
                obj.gameObject.SetActive(true);
            }

            return obj;
        }

        /// <summary>
        /// Returns an object to the pool
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(poolParent);

            // Reset object state
            ResetObject(obj);

            if (pool.Count < maxSize)
            {
                pool.Enqueue(obj);
            }
            else
            {
                GameObject.Destroy(obj.gameObject);
                currentCount--;
            }
        }

        private void ResetObject(T obj)
        {
            // Reset transform
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            // Reset rigidbody if present
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Clears the pool
        /// </summary>
        public void Clear()
        {
            while (pool.Count > 0)
            {
                T obj = pool.Dequeue();
                if (obj != null)
                {
                    GameObject.Destroy(obj.gameObject);
                }
            }
            currentCount = 0;
        }

        /// <summary>
        /// Gets the current pool size
        /// </summary>
        public int GetPoolSize() => pool.Count;

        /// <summary>
        /// Gets the total number of created objects
        /// </summary>
        public int GetTotalCount() => currentCount;
    }

    /// <summary>
    /// Manager for multiple object pools
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager instance;
        public static PoolManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("PoolManager");
                    instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private Dictionary<string, object> pools = new Dictionary<string, object>();

        /// <summary>
        /// Creates or gets a pool for a specific prefab
        /// </summary>
        public ObjectPool<T> GetPool<T>(T prefab, int initialSize = 10, int maxSize = 100) where T : Component
        {
            string key = typeof(T).Name + "_" + prefab.GetInstanceID();

            if (!pools.ContainsKey(key))
            {
                ObjectPool<T> newPool = new ObjectPool<T>(prefab, initialSize, maxSize, transform);
                pools[key] = newPool;
            }

            return pools[key] as ObjectPool<T>;
        }

        /// <summary>
        /// Clears all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                // Use reflection to call Clear on generic pools
                pool.GetType().GetMethod("Clear").Invoke(pool, null);
            }
            pools.Clear();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}