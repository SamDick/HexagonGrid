using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour
{
    public float TileScale = 1;
    public int RingCount = 5;
    public Vector3 OriginPoint;
    public GameObject TilePrefab;
    public MapTile HomeTile;
    public List<Vector3> Points;
    private List<Vector2Int> HexAxialCoords;
    public Dictionary<Vector2Int, MapTile> MapTiles;

    public Vector3 LastPosition;
    private Vector3 delta;

    public void Initialize()
    {
        if (transform.position != LastPosition)
        {
            if (LastPosition == Vector3.zero) { LastPosition = transform.position; return; }
            delta = transform.position - LastPosition;
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i] += delta;
            }
            OriginPoint += delta;
        }
        LastPosition = transform.position;
        var newPos = OriginPoint;
        HomeTile = SpawnHexTile(Vector2Int.zero, new Vector3(newPos.x, 0, newPos.z));
        MapTiles = new Dictionary<Vector2Int, MapTile>();
        HomeTile.AdjacentTiles = new List<MapTile>(new MapTile[6]);
        HomeTile.transform.parent = transform;
        HomeTile.transform.localScale = Vector3.one * TileScale;
        GenHex(HomeTile);
    }

    public void Reset()
    {
        DestroyGrid();
        MapTiles = new Dictionary<Vector2Int, MapTile>();
    }

    private void GenHex(MapTile initTile)
    {
        var handles = Points.ToArray();
        int centerQ = 0;
        int centerR = 0;
        HexAxialCoords = new List<Vector2Int>();

        for (int q = centerQ - RingCount; q <= centerQ + RingCount; q++)
        {
            int r1 = Mathf.Max(centerR - RingCount, -q - RingCount);
            int r2 = Mathf.Min(centerR + RingCount, -q + RingCount);
            for (int r = r1; r <= r2; r++)
            {
                HexAxialCoords.Add(new Vector2Int(q, r));
            }
        }

        foreach (Vector2Int hexCoord in HexAxialCoords)
        {
            float x = 1.5f * hexCoord.x * TileScale;
            float y = -1.732f * (hexCoord.y + ((hexCoord.x%2) / 2f)) * TileScale;
            Vector2 hexPos = new Vector2(x, y);
            Vector3 truePos = new Vector3(hexPos.x, 0f, hexPos.y) + initTile.transform.position;
            if (!HexGridEditor.IsVector3InPolygon(truePos, handles)) { continue; }
            SpawnHexTile(hexCoord, truePos);

        }
    }

    private MapTile SpawnHexTile(Vector2Int key, Vector3 hexPos)
    {
        GameObject newTile = Instantiate(TilePrefab, transform);
        MapTile tile = newTile.GetComponent<MapTile>();
        tile.transform.localScale = Vector3.one * TileScale;
        newTile.transform.position = hexPos;// ;
        tile.Coordinates = $"{key}";
        tile.Initialize(key);
        MapTiles.Add(key, tile);
        return tile;
    }

    public void AssociateTiles()
    {
        foreach (MapTile tile in MapTiles.Values)
        {
            for (int i = 0; i < 6; i++)
            {
                foreach(MapTile t in MapTiles.Values)
                {
                    if (tile.AdjacentTiles.Contains(t)) { continue; }
                    if (t.ID == tile.ID) { continue; }
                    if (Vector3.Distance(tile.position, t.position) <= 2 * TileScale)
                    {
                        tile.AddAdjacent(t);
                        break;
                    }
                }
            }
        }

    }

    private void DestroyGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var x = transform.GetChild(i);
            if(x != null)
                DestroyImmediate(x.gameObject);
        }
    }


}