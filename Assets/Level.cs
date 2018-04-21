﻿using UnityEngine;
using System;

public class Level : MonoBehaviour {

    public const int width = 80;
    public const int height = 48;

    public Sprite floorPrefab;
    public Sprite rockPrefab;

    [NonSerialized]
    public Vector2Int entryPoint;

    public enum Tile : byte {
        Floor,
        Rock,
    }

    private Tile[,] tiles;
    private Transform objectParent;

    private static float[,] neighborWeights = new float[3, 3] {
        { 1, 2, 1 },
        { 2, 0, 2 },
        { 1, 2, 1 },
    };

    static Level() {
        float total = 0;
        int size = neighborWeights.GetLength(0);
        for(int row = 0; row < size; ++row) {
            for(int col = 0; col < size; ++col) {
                total += neighborWeights[row, col];
            }
        }
        for(int row = 0; row < size; ++row) {
            for(int col = 0; col < size; ++col) {
                neighborWeights[row, col] /= total;
            }
        }
    }


    public void Create() {
        if(objectParent != null) {
            Destroy();
        }

        Generate();

        objectParent = new GameObject("Level").transform;

        for(int row = 0; row < height; ++row) {
            for(int column = 0; column < width; ++column) {
                Vector2Int square = new Vector2Int(column, row);
                Tile tile = tiles[row, column];
                GameObject obj = CreateObject(tile, square);
            }
        }
    }

    public void Destroy() {
        Destroy(objectParent.gameObject);
        objectParent = null;
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
        Transform t = obj.transform;
        t.SetParent(objectParent);
        t.localPosition = Game.GridToWorldPosition(square) + Vector3.forward;
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

    private void Generate() {
        tiles = new Tile[height, width];

        for(int row = 0; row < height; ++row) {
            for(int column = 0; column < width; ++column) {
                Tile tile = (Tile)UnityEngine.Random.Range(0, 2);
                tiles[row, column] = tile;
                //Debug.LogFormat("tile row {0} col {1} = {2}", row, column, tile);
            }
        }

        Tile[,] nextGeneration = new Tile[height, width];
        for(int generation = 0; generation < 4; ++generation) {
            for(int row = 0; row < height; ++row) {
                for(int column = 0; column < width; ++column) {
                    Tile tile = tiles[row, column];
                    float w = CountNeighbors(tiles, new Vector2Int(column, row), tile);
                    if(tile == Tile.Rock && w < .5f) {
                        nextGeneration[row, column] = Tile.Floor;
                    }
                    else if(tile == Tile.Floor && w < .5f) {
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

        entryPoint = FindOpenArea(2, 2, 20, 10);
    }

    private Vector2Int FindOpenArea(int lowCol, int lowRow, int highCol, int highRow) {
        Vector2Int best = Vector2Int.zero;
        float bestScore = -1;
        for(int i = 0; i < 20; ++i) {
            Vector2Int sq = new Vector2Int(
                UnityEngine.Random.Range(lowRow, highRow),
                UnityEngine.Random.Range(lowCol, highCol)
            );
            float w = CountNeighbors(tiles, sq, Tile.Floor);
            if(w > bestScore) {
                bestScore = w;
                best = sq;
            }
        }
        return best;
    }

    private static float CountNeighbors(Tile[,] tiles, Vector2Int square, Tile soughtTile) {
        float weight = 0;
        for(int y = -1; y <= 1; ++y) {
            for(int x = -1; x <= 1; ++x) {
                Vector2Int p = square + new Vector2Int(x, y);
                if(p.x >= 0 && p.x < width && p.y >= 0 && p.y < height && tiles[p.y, p.x] == soughtTile) {
                    weight += neighborWeights[y + 1, x + 1];
                }
            }
        }
        return weight;
    }
}
