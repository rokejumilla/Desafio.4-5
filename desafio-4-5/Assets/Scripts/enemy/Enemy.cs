using System;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int contactDamage = 1;
    public float moveSpeed = 1.5f;

    [Header("Patrol (optional)")]
    public Transform patrolPointA;
    public Transform patrolPointB;
    public bool patrolEnabled = false;
    public bool faceDirectionWithSprite = true;

    [Header("Pooling")]
    public GameObject prefabReference; // rellenado por el spawner si usás pool
    [HideInInspector] public bool spawnedFromPool = false;

    private int currentHealth;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 patrolTarget;
    private bool alive = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        alive = true;
        if (patrolEnabled && patrolPointA != null)
            patrolTarget = patrolPointA.position;
    }

    private void FixedUpdate()
    {
        if (!alive) return;

        if (patrolEnabled && patrolPointA != null && patrolPointB != null)
        {
            Vector2 pos = rb.position;
            Vector2 targetPos = patrolTarget;
            Vector2 dir = (targetPos - rb.position).normalized;
            rb.linearVelocity = dir * moveSpeed;

            if (faceDirectionWithSprite && spriteRenderer != null)
            {
                if (rb.linearVelocity.x > 0.05f) spriteRenderer.flipX = false;
                else if (rb.linearVelocity.x < -0.05f) spriteRenderer.flipX = true;
            }

            if (Vector2.Distance(pos, targetPos) < 0.15f)
            {
                if (patrolTarget == (Vector3)patrolPointA.position)
                    patrolTarget = patrolPointB.position;
                else
                    patrolTarget = patrolPointA.position;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (!alive) return;
        currentHealth -= dmg;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        alive = false;
        rb.linearVelocity = Vector2.zero;
        // aquí puedes añadir VFX/SFX
        if (spawnedFromPool && PoolManager.Instance != null && prefabReference != null)
        {
            PoolManager.Instance.Return(prefabReference, this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) daño por contacto al jugador — no referenciamos PlayerHealth directamente
        try
        {
            Component phComp = other.GetComponent("PlayerHealth");
            if (phComp != null)
            {
                // enviamos mensaje dinámico; si PlayerHealth tiene TakeDamage(int) lo recibirá
                phComp.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                // alternativa: si tu Player usa tag "Player" y recibe daño por otro método, podrías:
                // if (other.CompareTag("Player")) other.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Enemy: error al intentar dañar PlayerHealth dinámicamente: {ex.Message}");
        }

        // 2) impacto de proyectil — no referenciamos tipo Projectile (evita CS0246)
        try
        {
            Component projComp = other.GetComponent("Projectile");
            if (projComp != null)
            {
                int dmg = contactDamage;

                // intentamos obtener un campo 'damage' vía reflexión (si existe)
                Type t = projComp.GetType();
                FieldInfo f = t.GetField("damage", BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                {
                    object val = f.GetValue(projComp);
                    if (val != null)
                    {
                        try { dmg = Convert.ToInt32(val); } catch { dmg = contactDamage; }
                    }
                }
                else
                {
                    // intentamos propiedad 'damage'
                    PropertyInfo p = t.GetProperty("damage", BindingFlags.Public | BindingFlags.Instance);
                    if (p != null)
                    {
                        object val = p.GetValue(projComp);
                        if (val != null)
                        {
                            try { dmg = Convert.ToInt32(val); } catch { dmg = contactDamage; }
                        }
                    }
                }

                // aplicamos daño al enemigo
                TakeDamage(dmg);

                // avisamos al proyectil que golpeó (si tiene un método OnHitTarget)
                try { projComp.SendMessage("OnHitTarget", SendMessageOptions.DontRequireReceiver); } catch { }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Enemy: error al manejar proyectil dinámicamente: {ex.Message}");
        }
    }

    public void Init(GameObject prefabRef, bool fromPool)
    {
        prefabReference = prefabRef;
        spawnedFromPool = fromPool;
    }

    public void DeactivateForPool()
    {
        alive = false;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolEnabled)
        {
            Gizmos.color = Color.yellow;
            if (patrolPointA != null) Gizmos.DrawSphere(patrolPointA.position, 0.08f);
            if (patrolPointB != null) Gizmos.DrawSphere(patrolPointB.position, 0.08f);
            if (patrolPointA != null && patrolPointB != null)
                Gizmos.DrawLine(patrolPointA.position, patrolPointB.position);
        }
    }
}
