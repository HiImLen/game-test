using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using EasyButtons;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int GridSize;
    public float Radius = 1f;
    public bool IsFlatTopped;

    public HexTileGenerationSettings Settings;
    public HexTileGenerationSettings.TileType TileType;

    [Button]
    public void Clear()
    {
        List<GameObject> children = new();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }

        children.ForEach(child => DestroyImmediate(child, true));
    }

    [Button]
    public void LayoutGrid()
    {
        Clear();
        for (int y = 0; y < GridSize.y; y++)
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                GameObject tile = new($"Hex C{x}, R{y}");
                tile.transform.position = GetPositionForHexFromCoordinates(new Vector2Int(x, y));
                HexTile hexTile = tile.AddComponent<HexTile>();
                hexTile.Settings = Settings;
                hexTile.TileType = TileType;
                hexTile.AddTile();

                hexTile.OffsetCoordinate = new Vector2Int(x, y);
                hexTile.CubeCoordinate = OffsetToCube(hexTile.OffsetCoordinate);

                tile.transform.SetParent(transform, true);
            }
        }
    }

    private Vector3 GetPositionForHexFromCoordinates(Vector2Int coordinate)
    {
        int column = coordinate.x;
        int row = coordinate.y;
        float width;
        float height;
        float xPosition = 0f;
        float yPosition = 0f;
        bool shouldOffset;
        float horizontalDistance;
        float verticalDistance;
        float offset;
        float size = Radius;

        if (!IsFlatTopped)
        {
            shouldOffset = row % 2 == 0;
            width = Mathf.Sqrt(3) * size;
            height = 2f * size;

            horizontalDistance = width;
            verticalDistance = height * 0.75f;

            offset = shouldOffset ? width / 2 : 0f;

            xPosition = column * horizontalDistance + offset;
            yPosition = row * verticalDistance;
        }
        else
        {
            shouldOffset = column % 2 == 0;
            width = 2f * size;
            height = Mathf.Sqrt(3) * size;

            horizontalDistance = width * 0.75f;
            verticalDistance = height;

            offset = shouldOffset ? height / 2 : 0f;

            xPosition = column * horizontalDistance;
            yPosition = row * verticalDistance - offset;
        }

        return new Vector3(xPosition, 0f, -yPosition);
    }

    private Vector3Int OffsetToCube(Vector2Int offset)
    {
        var q = offset.x - (offset.y + (offset.y % 2)) / 2;
        var r = offset.y;
        return new Vector3Int(q, r, -q - r);
    }
}
