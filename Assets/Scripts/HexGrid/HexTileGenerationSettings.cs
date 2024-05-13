using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileGen/HexTileGenerationSettings")]
public class HexTileGenerationSettings : ScriptableObject
{
    public enum TileType
    {
        Rubber,
        Wall
    }

    public GameObject Rubber;
    public GameObject Wall;

    public GameObject GetTile(TileType tileType)
    {
        return tileType switch
        {
            TileType.Rubber => Rubber,
            TileType.Wall => Wall,
            _ => null,
        };
    }
}
