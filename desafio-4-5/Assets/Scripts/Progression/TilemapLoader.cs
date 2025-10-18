using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLoader : MonoBehaviour
{
    [Tooltip("Arrastra aquí el Tilemap que quieres pintar (puede estar en un hijo).")]
    public Tilemap tilemap;

    private void Awake()
    {
        if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();
    }

    /// <summary>
    /// Carga un tilemap desde un CSV (cada celda = índice en palette; -1 = vacío).
    /// Primera fila del CSV corresponde a la fila superior del tilemap.
    /// </summary>
    public void LoadFromCSV(TextAsset csv, TileBase[] palette)
    {
        if (csv == null || tilemap == null || palette == null)
        {
            Debug.LogWarning("TilemapLoader: recursos faltantes (csv/palette/tilemap).");
            return;
        }

        tilemap.ClearAllTiles();

        string[] rows = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        int h = rows.Length;
        for (int y = 0; y < h; y++)
        {
            // invertimos el orden para que la primera fila del CSV sea el tope
            string row = rows[h - 1 - y].Trim();
            string[] cells = row.Split(',');
            for (int x = 0; x < cells.Length; x++)
            {
                if (int.TryParse(cells[x], out int id))
                {
                    if (id >= 0 && id < palette.Length)
                    {
                        tilemap.SetTile(new Vector3Int(x, y, 0), palette[id]);
                    }
                    else
                    {
                        // id == -1 => dejar vacío; fuera de rango => warning opcional
                    }
                }
            }
        }
    }
}
