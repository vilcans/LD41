using UnityEngine;
using System;

public class Level : MonoBehaviour {

    public Sprite floorPrefab;
    public Sprite rockPrefab;

    private Transform objectParent;
    public TileMap map;

    public void Create() {
        if(objectParent != null) {
            Destroy();
        }

        map = TileMap.Generate();

        objectParent = new GameObject("Level").transform;

        for(int row = 0; row < TileMap.height; ++row) {
            for(int column = 0; column < TileMap.width; ++column) {
                Vector2Int square = new Vector2Int(column, row);
                TileMap.Tile tile = map.GetTile(square);
                GameObject obj = CreateObject(tile, square);
            }
        }
    }

    public void Destroy() {
        Destroy(objectParent.gameObject);
        objectParent = null;
    }

    private GameObject CreateObject(TileMap.Tile tile, Vector2Int square) {
        GameObject obj = new GameObject("Tile" + square);
        Transform t = obj.transform;
        t.SetParent(objectParent);
        t.localPosition = Game.GridToWorldPosition(square) + Vector3.forward;
        obj.isStatic = true;

        Sprite sprite;
        switch(tile) {
            case TileMap.Tile.Floor:
                sprite = floorPrefab;
                break;
            case TileMap.Tile.Rock:
                sprite = rockPrefab;
                break;
            default:
                throw new ApplicationException("Unhandled tile type: " + tile);
        }
        SpriteRenderer r = obj.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        return obj;
    }
}
