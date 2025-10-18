// PoolManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PoolManager simple y genérico. Se indexa por el prefab original (GameObject).
/// Usar: PoolManager.Instance.CreatePool(prefab, size);
///      var go = PoolManager.Instance.Get(prefab, pos, rot);
///      PoolManager.Instance.Return(prefab, go);
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    /// <summary> Creates a warm pool for a prefab. Safe to call multiple times. </summary>
    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null) return;
        if (pools.ContainsKey(prefab)) return;

        var q = new Queue<GameObject>();
        var container = new GameObject(prefab.name + "_Pool");
        container.transform.SetParent(transform);

        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(prefab, container.transform);
            go.SetActive(false);
            q.Enqueue(go);
        }

        pools.Add(prefab, q);
    }

    /// <summary> Gets an object from pool (or instantiates if pool empty). </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!pools.ContainsKey(prefab))
        {
            CreatePool(prefab, 5); // warm small default
        }

        var q = pools[prefab];
        GameObject obj;
        if (q.Count == 0)
        {
            obj = Instantiate(prefab);
        }
        else
        {
            obj = q.Dequeue();
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    /// <summary> Returns an object to its pool. If pool doesn't exist, create one. </summary>
    public void Return(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null) return;

        obj.SetActive(false);

        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }
        pools[prefab].Enqueue(obj);

        // Optionally parent to pool container (if created in CreatePool)
        var container = transform.Find(prefab.name + "_Pool");
        if (container != null) obj.transform.SetParent(container, true);
        else obj.transform.SetParent(transform, true);
    }
}
