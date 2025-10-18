// GeneradorObjectAleatorio_Shape.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GeneradorObjectAleatorio : MonoBehaviour
{
    public enum SpawnShape { Point, Rectangle, Circle, Annulus }

    [Header("Prefabs & Pool")]
    public GameObject[] prefabs;
    [Min(1)] public int poolSizePerPrefab = 10;

    [Header("Timing")]
    [Range(0f, 30f)] public float startDelay = 0f;
    [Range(0.01f, 30f)] public float intervalo = 1f;

    [Header("Spawn shape")]
    public SpawnShape spawnShape = SpawnShape.Rectangle;

    [Tooltip("Rect area size in local space (width, height)")]
    public Vector2 spawnAreaSize = new Vector2(4f, 2f);

    [Tooltip("Circle radius (world units)")]
    [Min(0f)] public float circleRadius = 3f;

    [Tooltip("Annulus min radius (inner)")]
    [Min(0f)] public float annulusMinRadius = 1f;
    [Tooltip("Annulus max radius (outer)")]
    [Min(0f)] public float annulusMaxRadius = 3f;

    [Header("Collision avoidance (opcional)")]
    [Tooltip("Evita spawnear encima de colliders usando Physics2D. Overlap radius debe ser pequeño")]
    public bool avoidObstacles = false;
    [Tooltip("Radio para chequear colisiones al spawnear")]
    [Min(0f)] public float avoidanceCheckRadius = 0.2f;
    public LayerMask avoidanceLayerMask = ~0; // por defecto todo

    [Header("Attempts")]
    [Tooltip("Intentos para encontrar una posición válida antes de fallar (si avoidObstacles=true)")]
    [Range(1, 50)] public int maxSpawnAttempts = 8;

    private Coroutine routine;
    private SpriteRenderer spriteRenderer;

    private void Reset()
    {
        intervalo = 1f;
        startDelay = 0.5f;
        poolSizePerPrefab = 10;
        spawnAreaSize = new Vector2(4f, 2f);
        circleRadius = 3f;
        annulusMinRadius = 1f;
        annulusMaxRadius = 3f;
        avoidanceCheckRadius = 0.2f;
        maxSpawnAttempts = 8;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (PoolManager.Instance != null && prefabs != null)
        {
            foreach (var p in prefabs)
                if (p != null) PoolManager.Instance.CreatePool(p, poolSizePerPrefab);
        }
    }

    private void OnBecameVisible()
    {
        if (routine == null) routine = StartCoroutine(SpawnLoop());
    }

    private void OnBecameInvisible()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    private IEnumerator SpawnLoop()
    {
        if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            if (prefabs != null && prefabs.Length > 0)
            {
                int idx = Random.Range(0, prefabs.Length);
                var prefab = prefabs[idx];
                if (prefab != null)
                {
                    Vector3 spawnPos = transform.position;
                    bool found = false;

                    if (!avoidObstacles)
                    {
                        spawnPos = CalculateSpawnPosition();
                        found = true;
                    }
                    else
                    {
                        // intenta varias posiciones hasta encontrar una sin colisión
                        for (int i = 0; i < maxSpawnAttempts; i++)
                        {
                            Vector3 candidate = CalculateSpawnPosition();
                            if (!Physics2D.OverlapCircle(candidate, avoidanceCheckRadius, avoidanceLayerMask))
                            {
                                spawnPos = candidate;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        if (PoolManager.Instance != null)
                            PoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity);
                        else
                            Instantiate(prefab, spawnPos, Quaternion.identity);
                    }
                    // si no encontró, se salta spawn esta vez
                }
            }

            yield return new WaitForSeconds(intervalo);
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        switch (spawnShape)
        {
            case SpawnShape.Point:
                return transform.position;

            case SpawnShape.Rectangle:
                // rectángulo en local space centrado en transform
                float rx = (Random.value - 0.5f) * spawnAreaSize.x;
                float ry = (Random.value - 0.5f) * spawnAreaSize.y;
                return transform.TransformPoint(new Vector3(rx, ry, 0f));

            case SpawnShape.Circle:
                {
                    // sampling uniforme en círculo (radio máximo = circleRadius)
                    float angle = Random.value * Mathf.PI * 2f;
                    float r = Mathf.Sqrt(Random.value) * circleRadius; // sqrt para distribución uniforme
                    Vector3 local = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
                    return transform.TransformPoint(local);
                }

            case SpawnShape.Annulus:
                {
                    // sampling uniforme en disco anular entre annulusMinRadius y annulusMaxRadius
                    float angle = Random.value * Mathf.PI * 2f;
                    float rMin = annulusMinRadius;
                    float rMax = Mathf.Max(annulusMaxRadius, rMin + 0.0001f);
                    // fórmula para uniformidad en área:
                    float r = Mathf.Sqrt(Random.value * (rMax * rMax - rMin * rMin) + rMin * rMin);
                    Vector3 local = new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f);
                    return transform.TransformPoint(local);
                }

            default:
                return transform.position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.7f, 1f, 0.15f);
        Gizmos.matrix = transform.localToWorldMatrix;

        switch (spawnShape)
        {
            case SpawnShape.Rectangle:
                Gizmos.DrawCube(Vector3.zero, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
                Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0f));
                break;

            case SpawnShape.Circle:
                Gizmos.DrawSphere(Vector3.zero, circleRadius);
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
                DrawWireCircle(Vector3.zero, circleRadius);
                break;

            case SpawnShape.Annulus:
                // dibujamos anillo con dos círculos (externo e interno)
                Gizmos.DrawSphere(Vector3.zero, annulusMaxRadius);
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
                DrawWireCircle(Vector3.zero, annulusMaxRadius);
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.15f);
                Gizmos.DrawSphere(Vector3.zero, annulusMinRadius);
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
                DrawWireCircle(Vector3.zero, annulusMinRadius);
                break;

            case SpawnShape.Point:
                Gizmos.color = new Color(0f, 0.7f, 1f, 0.9f);
                Gizmos.DrawSphere(Vector3.zero, 0.1f);
                break;
        }
    }

    // helper simple para dibujar círculo con líneas
    private void DrawWireCircle(Vector3 center, float radius, int segments = 40)
    {
        float step = 2f * Mathf.PI / segments;
        Vector3 prev = center + new Vector3(Mathf.Cos(0f), Mathf.Sin(0f), 0f) * radius;
        for (int i = 1; i <= segments; i++)
        {
            float ang = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
