using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class HexTile : MonoBehaviour
{
    public HexTileGenerationSettings Settings;
    public HexTileGenerationSettings.TileType TileType;
    public GameObject Tile;
    public Vector2Int OffsetCoordinate;
    public Vector3Int CubeCoordinate;
    public List<HexTile> Neighbors;
    public bool IsDirty = false;

    public void RollTileType()
    {
        TileType = (HexTileGenerationSettings.TileType)Random.Range(0, System.Enum.GetValues(typeof(HexTileGenerationSettings.TileType)).Length);
    }

    public void AddTile()
    {
        Tile = Instantiate(Settings.GetTile(TileType));
        Tile.transform.SetParent(transform, true);
        Tile.transform.position = transform.position;
    }

    private void Update()
    {
        if (IsDirty)
        {
            if (!Application.isPlaying)
            {
                DestroyImmediate(Tile);
            }
            AddTile();
            IsDirty = false;
        }
    }
}
