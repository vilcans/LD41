using System;
using UnityEngine;

public class TileMap {
    public const int width = 80;
    public const int height = 48;

    public Vector2Int entryPoint;

    private int borderThickness;

    [Flags]
    public enum Tile : byte {
        Bedrock = 1,
        Rock = 2,
        Floor = 4,
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
        for(int row = 0; row < height; ++row) {
            for(int column = 0; column < width; ++column) {
                Tile tile;
                if(row < border || row >= height - border || column < border || column >= width - border) {
                    tile = Tile.Bedrock;
                }
                else {
                    tile = (Tile)(1 << UnityEngine.Random.Range(1, 3));
                }
                map.tiles[row, column] = tile;
            }
        }

        TileMap nextGeneration = new TileMap();
        for(int generation = 0; generation < 4; ++generation) {
            for(int row = 0; row < height; ++row) {
                for(int column = 0; column < width; ++column) {
                    Tile tile = map.tiles[row, column];
                    Vector2Int sq = new Vector2Int(column, row);
                    if(tile == Tile.Rock && map.CountNeighbors(sq, Tile.Rock | Tile.Bedrock) < .5f) {
                        nextGeneration.tiles[row, column] = Tile.Floor;
                    }
                    else if(tile == Tile.Floor && map.CountNeighbors(sq, Tile.Floor) < .5f) {
                        if(map.CountNeighbors(sq, Tile.Bedrock) > 0f) {
                            nextGeneration.tiles[row, column] = Tile.Bedrock;
                        }
                        else {
                            nextGeneration.tiles[row, column] = Tile.Rock;
                        }
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

    private float CountNeighbors(Vector2Int square, Tile soughtTilesMask) {
        float weight = 0;
        for(int y = -1; y <= 1; ++y) {
            for(int x = -1; x <= 1; ++x) {
                Vector2Int p = square + new Vector2Int(x, y);
                Tile neighbor = GetTile(p);
                if((neighbor & soughtTilesMask) != 0) {
                    weight += neighborWeights[y + 1, x + 1];
                }
            }
        }
        return weight;
    }
}
