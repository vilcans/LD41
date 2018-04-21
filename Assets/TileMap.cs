﻿using UnityEngine;

public class TileMap {
    public const int width = 80;
    public const int height = 48;

    public Vector2Int entryPoint;

    private int borderThickness;

    public enum Tile : byte {
        Rock,
        Floor,
    }

    private Tile[,] tiles;

    private static float[,] neighborWeights = new float[3, 3] {
        { 1, 2, 1 },
        { 2, 0, 2 },
        { 1, 2, 1 },
    };

    static TileMap() {
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

    private TileMap() {
        tiles = new Tile[height, width];
    }

    public Tile GetTile(Vector2Int square) {
        if(square.x < 0 || square.x >= width || square.y < 0 || square.y >= height) {
            return Tile.Rock;
        }
        return tiles[square.y, square.x];
    }

    public static TileMap Generate() {
        TileMap map = new TileMap();

        int border = 1;
        for(int row = border; row < height - border; ++row) {
            for(int column = border; column < width - border; ++column) {
                Tile tile = (Tile)UnityEngine.Random.Range(0, 2);
                map.tiles[row, column] = tile;
            }
        }

        TileMap nextGeneration = new TileMap();
        for(int generation = 0; generation < 4; ++generation) {
            for(int row = 0; row < height; ++row) {
                for(int column = 0; column < width; ++column) {
                    Tile tile = map.tiles[row, column];
                    float w = map.CountNeighbors(new Vector2Int(column, row), tile);
                    if(tile == Tile.Rock && w < .5f) {
                        nextGeneration.tiles[row, column] = Tile.Floor;
                    }
                    else if(tile == Tile.Floor && w < .5f) {
                        nextGeneration.tiles[row, column] = Tile.Rock;
                    }
                    else {
                        nextGeneration.tiles[row, column] = tile;
                    }
                }
            }
            {
                var tmp = nextGeneration;
                nextGeneration = map;
                map = tmp;
            }
        }

        map.entryPoint = map.FindOpenArea(border, border, width / 8, height - border * 2);

        return map;
    }

    private Vector2Int FindOpenArea(int lowCol, int lowRow, int highCol, int highRow) {
        Vector2Int best = Vector2Int.zero;
        float bestScore = -1;
        for(int i = 0; i < 10; ++i) {
            Vector2Int sq = new Vector2Int(
                UnityEngine.Random.Range(lowCol, highCol),
                UnityEngine.Random.Range(lowRow, highRow)
            );
            float w = CountNeighbors(sq, Tile.Floor);
            if(w > bestScore) {
                bestScore = w;
                best = sq;
            }
        }
        return best;
    }

    private float CountNeighbors(Vector2Int square, Tile soughtTile) {
        float weight = 0;
        for(int y = -1; y <= 1; ++y) {
            for(int x = -1; x <= 1; ++x) {
                Vector2Int p = square + new Vector2Int(x, y);
                Tile neighbor = GetTile(p);
                if(neighbor == soughtTile) {
                    weight += neighborWeights[y + 1, x + 1];
                }
            }
        }
        return weight;
    }
}
