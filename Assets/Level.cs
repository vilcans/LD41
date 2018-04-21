using UnityEngine;
using System;

public class Level : MonoBehaviour {

    public const int width = 40;
    public const int height = 24;

    public Sprite floorPrefab;
    public Sprite rockPrefab;

    public enum Tile : byte {
        Floor,
        Rock,
    }

    private Tile[,] tiles;

    public void Create() {
        tiles = GenerateTiles();

        for(int row = 0; row < height; ++row) {
            for(int column = 0; column < width; ++column) {
                Vector2Int square = new Vector2Int(column, row);
                Tile tile = tiles[row, column];
                GameObject obj = CreateObject(tile, square);
            }
        }
    }

    public bool IsWalkable(Vector2Int square) {
        if(square.x < 0 || square.x >= width || square.y < 0 || square.y >= height) {
            return false;
        }
        Tile t = tiles[square.y, square.x];
        return t == Tile.Floor;
    }

    private GameObject CreateObject(Tile tile, Vector2Int square) {
        GameObject obj = new GameObject("Tile" + square);
        obj.transform.localPosition = Game.GridToWorldPosition(square) + Vector3.forward;
        obj.isStatic = true;

        Sprite sprite;
        switch(tile) {
            case Tile.Floor:
                sprite = floorPrefab;
                break;
            case Tile.Rock:
                sprite = rockPrefab;
                break;
            default:
                throw new ApplicationException("Unhandled tile type: " + tile);
        }
        SpriteRenderer r = obj.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        return obj;
    }

    private static Tile[,] GenerateTiles() {
        Tile[,] tiles = new Tile[height, width];

        for(int row = 0; row < height; ++row) {
            for(int column = 0; column < width; ++column) {
                Tile tile = (Tile)UnityEngine.Random.Range(0, 2);
                tiles[row, column] = tile;
                //Debug.LogFormat("tile row {0} col {1} = {2}", row, column, tile);
            }
        }

        Tile[,] nextGeneration = new Tile[height, width];
        for(int generation = 0; generation < 3000; ++generation) {
            for(int row = 0; row < height; ++row) {
                for(int column = 0; column < width; ++column) {
                    Tile tile = tiles[row, column];
                    int neighbors = CountNeighbors(tiles, new Vector2Int(column, row), tile);
                    if(tile == Tile.Rock && neighbors < 2) {
                        nextGeneration[row, column] = Tile.Floor;
                    }
                    else if(tile == Tile.Floor && neighbors < 2) {
                        nextGeneration[row, column] = Tile.Rock;
                    }
                    else {
                        nextGeneration[row, column] = tile;
                    }
                }
            }
            {
                var tmp = nextGeneration;
                nextGeneration = tiles;
                tiles = tmp;
            }
        }

        return tiles;
    }

    private static int CountNeighbors(Tile[,] tiles, Vector2Int square, Tile soughtTile) {
        int neighbors = 0;
        if(square.x >= 1 && tiles[square.y, square.x - 1] == soughtTile) { ++neighbors; }
        if(square.x < width - 1 && tiles[square.y, square.x + 1] == soughtTile) { ++neighbors; }
        if(square.y >= 1 && tiles[square.y - 1, square.x] == soughtTile) { ++neighbors; }
        if(square.y < height - 1 && tiles[square.y + 1, square.x] == soughtTile) { ++neighbors; }
        return neighbors;
    }
}
