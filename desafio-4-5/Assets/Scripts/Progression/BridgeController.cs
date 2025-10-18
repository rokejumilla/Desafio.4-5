using UnityEngine;
using UnityEngine.Tilemaps;

public class BridgeController : MonoBehaviour
{
    [Header("Opciones Bridge")]
    public bool useTilemap = false;        // si true usa tilemap, sino activa/desactiva GameObject
    public GameObject bridgeGameObject;    // alternativa: puente como GameObject
    public Tilemap bridgeTilemap;         // alternativa: Tilemap del puente
    public string saveKey = "bridge_state_default"; // valores: 0 = visible, 1 = hidden

    private bool isHidden = false;

    private void Awake()
    {
        // Restaurar estado
        int val = PlayerPrefs.GetInt(saveKey, 0);
        isHidden = val == 1;
        ApplyState();
    }

    public void ToggleBridge()
    {
        isHidden = !isHidden;
        ApplyState();
        PlayerPrefs.SetInt(saveKey, isHidden ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        ApplyState();
        PlayerPrefs.SetInt(saveKey, isHidden ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyState()
    {
        if (useTilemap && bridgeTilemap != null)
        {
            // opción segura: habilitar/ deshabilitar renderer y collider
            var tmRenderer = bridgeTilemap.GetComponent<TilemapRenderer>();
            if (tmRenderer != null) tmRenderer.enabled = !isHidden;

            var tmCollider = bridgeTilemap.GetComponent<TilemapCollider2D>();
            if (tmCollider != null) tmCollider.enabled = !isHidden;

            var composite = bridgeTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null) composite.enabled = !isHidden;
        }
        else if (bridgeGameObject != null)
        {
            bridgeGameObject.SetActive(!isHidden);
        }
    }
}
