using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    public string Coordinates;
    

    public Vector3 position => transform.position + Vector3.down;
    public CapsuleCollider Collider;
    public GameObject Contents;
    public List<MapTile> AdjacentTiles;

    public Vector2Int ID;

    public int rowID;
    public int columnID;

    public float FCost;
    public float HCost;
    public float Cost;

    public void Initialize(Vector2Int key)
    {
        ID = key;
        rowID = ID.x;
        columnID = ID.y;
        name = $"MapTile{Coordinates}";
        var adj = new MapTile[6];
        AdjacentTiles = new List<MapTile>(adj);
    }

    internal void AddAdjacent(MapTile initTile)
    {
        for(int i = 0; i < 6; i++)
        {
            if(AdjacentTiles[i] != null) { continue; }
            AdjacentTiles[i] = initTile;
            break;

        }
        for (int i = 0; i < 6; i++)
        {
            if (initTile.AdjacentTiles[i] != null) { continue; }
            initTile.AdjacentTiles[i] = this;
            return;
        }
    }
}
