using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Lever : MonoBehaviour
{
    public BridgeController targetBridge;
    public string saveKey = "lever_default"; // opcional persistencia por palanca id

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (targetBridge == null) { Debug.LogWarning("Lever: targetBridge no asignado"); return; }
        targetBridge.ToggleBridge();
        // opcional: anim/sonido
    }
}
