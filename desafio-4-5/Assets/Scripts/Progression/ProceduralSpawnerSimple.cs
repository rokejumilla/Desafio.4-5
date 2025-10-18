using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralSpawnerSimple : MonoBehaviour
{
    [System.Serializable]
    public class Entry
    {
        public string id = "entry";
        public GameObject prefab;
        public bool spawnOnStart = true;
        public float initialDelay = 0f;
        [Tooltip("Intervalo aleatorio entre spawns (min,max)")]
        public float minInterval = 1f;
        public float maxInterval = 2f;
        public int poolSize = 8;
        public float weight = 1f;
        public Vector2 spawnOffset = Vector2.zero;
        public bool usePool = true;
    }

    [Header("Entries (inline - no ScriptableObject)")]
    public Entry[] entries;

    [Header("Options")]
    public bool loopForever = true;
    public bool startOnAwake = true;

    [Header("Debug")]
    public bool logSpawns = true;
    public int inspectorTest = 123; // campo simple para comprobar Inspector

    // simple internal pool (by prefab)
    Dictionary<GameObject, Queue<GameObject>> internalPools = new Dictionary<GameObject, Queue<GameObject>>();

    private Coroutine spawnRoutine;

    void OnValidate()
    {
        // Asegurar valores válidos visibles en Inspector
        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (e.minInterval < 0f) e.minInterval = 0f;
                if (e.maxInterval < e.minInterval) e.maxInterval = e.minInterval;
                if (e.poolSize < 0) e.poolSize = 0;
                if (e.weight <= 0f) e.weight = 0.0001f;
            }
        }
    }

    void Awake()
    {
        // preparar pools según entries (si usePool)
        internalPools.Clear();
        if (entries == null) return;
        foreach (var e in entries)
        {
            if (e == null || e.prefab == null) continue;
            if (!e.usePool) continue;
            if (!internalPools.ContainsKey(e.prefab))
            {
                var q = new Queue<GameObject>();
                for (int i = 0; i < Mathf.Max(1, e.poolSize); i++)
                {
                    var go = Instantiate(e.prefab, transform);
                    go.name = e.prefab.name;
                    go.SetActive(false);
                    q.Enqueue(go);
                }
                internalPools[e.prefab] = q;
            }
        }
    }

    void Start()
    {
        if (startOnAwake) StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null) { StopCoroutine(spawnRoutine); spawnRoutine = null; }
    }

    IEnumerator SpawnLoop()
    {
        if (entries == null || entries.Length == 0) yield break;

        // respetar initialDelay de entradas con spawnOnStart
        foreach (var ent in entries)
        {
            if (ent != null && ent.spawnOnStart && ent.initialDelay > 0f)
                yield return new WaitForSeconds(ent.initialDelay);
        }

        while (true)
        {
            var entry = PickRandomEntry(entries);
            if (entry == null) yield break;

            float wait = Random.Range(entry.minInterval, entry.maxInterval);
            if (entry.initialDelay > 0f) wait = Mathf.Max(0f, wait + entry.initialDelay);

            yield return new WaitForSeconds(wait);

            SpawnOne(entry);

            if (!loopForever) break;
            yield return null;
        }
    }

    void SpawnOne(Entry e)
    {
        if (e == null || e.prefab == null) return;

        Vector3 pos = transform.position + (Vector3)e.spawnOffset;
        GameObject spawned = null;

        if (e.usePool && internalPools.TryGetValue(e.prefab, out var q))
        {
            if (q.Count > 0)
            {
                spawned = q.Dequeue();
                spawned.transform.position = pos;
                spawned.transform.rotation = Quaternion.identity;
                spawned.SetActive(true);
            }
            else
            {
                spawned = Instantiate(e.prefab, pos, Quaternion.identity);
                spawned.name = e.prefab.name;
            }
        }
        else
        {
            spawned = Instantiate(e.prefab, pos, Quaternion.identity);
        }

        if (logSpawns) Debug.Log($"Spawned '{e.id}' at {pos}", this);
        // si el objeto tiene un comportamiento que necesita init, hacerlo aquí (OnEnable suele ser suficiente)

        // Si quieres que el objeto vuelva al pool automáticamente (ejemplo simple):
        var pooledReturn = spawned.GetComponent<AutoReturnToPool>();
        if (pooledReturn == null && internalPools.ContainsKey(e.prefab))
        {
            // añadimos un componente auxiliar para que vuelva al pool tras x segundos (solo ejemplo)
            pooledReturn = spawned.AddComponent<AutoReturnToPool>();
            pooledReturn.Setup(this, e.prefab, 5f); // vuelve en 5s por defecto
        }
    }

    // devolver objeto al pool (llamado por AutoReturnToPool)
    public void ReturnToPool(GameObject obj, GameObject originalPrefab)
    {
        if (obj == null || originalPrefab == null) return;
        obj.SetActive(false);
        obj.transform.SetParent(transform, false);

        if (internalPools.TryGetValue(originalPrefab, out var q))
        {
            q.Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    Entry PickRandomEntry(Entry[] list)
    {
        if (list == null || list.Length == 0) return null;
        float total = 0f;
        foreach (var en in list) total += Mathf.Max(0.0001f, en.weight);
        float r = Random.Range(0f, total);
        float acc = 0f;
        foreach (var en in list)
        {
            acc += Mathf.Max(0.0001f, en.weight);
            if (r <= acc) return en;
        }
        return list[0];
    }

    // Componente auxiliar que devuelve objetos al pool automáticamente (demo)
    class AutoReturnToPool : MonoBehaviour
    {
        ProceduralSpawnerSimple owner;
        GameObject originalPrefab;
        float timer;

        public void Setup(ProceduralSpawnerSimple o, GameObject prefab, float life)
        {
            owner = o;
            originalPrefab = prefab;
            timer = life;
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                owner.ReturnToPool(gameObject, originalPrefab);
            }
        }

        void OnDisable()
        {
            // limpiar si es necesario
        }
    }
}
