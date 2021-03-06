﻿using System;
using UnityEngine;
using UnityEngine.Assertions;

public class TileMap {
    public readonly int width;
    public readonly int height;

    public const int border = 1;

    public Vector2Int entryPoint;
    public Vector2Int exitPoint;

    public PathMap pathToPlayer;

    [Flags]
    public enum Tile : byte {
        Bedrock = 1,
        Wall = 2,
        Floor = 4,
        Exit = 8,
        Food = 16,
    }

    private Tile[,] tiles;

    public static readonly float[,] standardWeights = NormalizeWeights(new float[3, 3] {
        { 1, 2, 1 },
        { 2, 0, 2 },
        { 1, 2, 1 },
    });
    public static readonly float[,] verticalStripesWeights = NormalizeWeights(new float[3, 3] {
        { 1, 0, 1 },
        { 1, 0, 1 },
        { 1, 0, 1 },
    });
    public static readonly float[,] horizontalStripesWeights = NormalizeWeights(new float[3, 3] {
        { 1, 1, 1 },
        { 0, 0, 0 },
        { 1, 1, 1 },
    });
    public static readonly float[,] horizontalCorridorWeights = NormalizeWeights(new float[5, 5] {
        { 1, 2, 3, 2, 1 },
        { 0, 0, 0, 0, 0 },
        { 1, 2, 3, 2, 1 },
        { 0, 0, 0, 0, 0 },
        { 1, 2, 3, 2, 1 },
    });
    public static readonly float[,] bigHallsWeights = NormalizeWeights(new float[5, 5] {
        { 1, 1, 1, 1, 1 },
        { 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 1 },
        { 1, 0, 0, 0, 1 },
        { 1, 1, 1, 1, 1 },
    });

    private readonly float[,] neighborWeights;

    private TileMap(Vector2Int size, float[,] weights) {
        width = size.x;
        height = size.y;
        tiles = new Tile[height, width];
        neighborWeights = weights;
    }

    public Tile GetTile(Vector2Int square) {
        if(square.x < 0 || square.x >= width || square.y < 0 || square.y >= height) {
            return Tile.Wall;
        }
        return tiles[square.y, square.x];
    }

    public void SetTile(Vector2Int square, Tile tile) {
        Assert.IsFalse(square.x < 0 || square.x >= width || square.y < 0 || square.y >= height);
        tiles[square.y, square.x] = tile;
    }

    public static TileMap Generate(
        Vector2Int size,
        int refinementIterations,
        float[,] weights,
        int foodDrops
    ) {
        int width = size.x;
        int height = size.y;

        TileMap map = new TileMap(size, weights);

        for(int row = 0; row < map.height; ++row) {
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

        TileMap nextGeneration = new TileMap(size, weights);
        for(int generation = 0; generation < refinementIterations; ++generation) {
            for(int row = 0; row < height; ++row) {
                for(int column = 0; column < width; ++column) {
                    Tile tile = map.tiles[row, column];
                    Vector2Int sq = new Vector2Int(column, row);
                    if(tile == Tile.Wall && map.CountNeighbors(sq, Tile.Wall | Tile.Bedrock) < .5f) {
                        nextGeneration.tiles[row, column] = Tile.Floor;
                    }
                    else if(tile == Tile.Floor && map.CountNeighbors(sq, Tile.Floor) < .5f) {
                        if(map.CountNeighbors(sq, Tile.Bedrock) > 0f) {
                            nextGeneration.tiles[row, column] = Tile.Bedrock;
                        }
                        else {
                            nextGeneration.tiles[row, column] = Tile.Wall;
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

        map.CreateEntryAndExit();

        // Drop food
        {
            int tries = foodDrops * 20;
            while(tries > 0 && foodDrops > 0) {
                Vector2Int square = map.GetRandomSquare();
                if(map.GetTile(square) == Tile.Floor) {
                    map.SetTile(square, Tile.Food);
                    --foodDrops;
                }
                else {
                    --tries;
                }
            }
        }

        return map;
    }

    public Vector2Int GetRandomSquare() {
        return new Vector2Int(UnityEngine.Random.Range(border, width - border), UnityEngine.Random.Range(border, height - border));
    }

    public bool IsWalkable(Vector2Int square) {
        if(!IsInBounds(square)) {
            return false;
        }
        return IsWalkable(GetTile(square));
    }

    public static bool IsWalkable(Tile tile) {
        return (tile & (Tile.Food | Tile.Floor | Tile.Exit)) != 0;
    }

    public float GetCost(Vector2Int square) {
        Tile t = GetTile(square);
        switch(t) {
            case Tile.Bedrock:
                return 1e9f;
            case Tile.Wall:
                return 1e6f;
            default:
                return 1;
        }
    }

    public bool IsInBounds(Vector2Int square) {
        return square.x >= 0 && square.x < width && square.y >= 0 && square.y < height;
    }

    private void CreateEntryAndExit() {
        Vector2Int p1 = FindOpenArea(border, border, width / 4, height - border);
        Vector2Int p2 = FindOpenArea(width - border - width / 4, border, width - width / 8, height - border);

        if(UnityEngine.Random.value < .5) {
            entryPoint = p1;
            exitPoint = p2;
        }
        else {
            entryPoint = p2;
            exitPoint = p1;
        }

        // Pathfind from exit to entry
        {
            PathMap path = new PathMap(this);
            path.StartSearch(entryPoint, Mathf.Infinity);
            bool found = path.UpdateUntilPathFound(exitPoint);
            //path.UpdateAll(); bool found = true;
            if(found) {
                //path.Print();
                Vector2Int sq = exitPoint;
                //Debug.LogFormat("Pathfinding from {0}", sq);
                while(sq != entryPoint) {
                    PathMap.Node node = path.nodes[sq.y, sq.x];
                    //Debug.LogFormat("At {0}: cost {1} direction {2}", sq, node.cost, node.direction.GetCharacter());
                    Vector3 worldPos = Game.GridToWorldPosition(sq);
                    if(GetTile(sq) != Tile.Floor) {
                        //Debug.LogFormat("Changing {0} from {1} to floor", sq, GetTile(sq));
#if UNITY_EDITOR
                        Debug.DrawLine(worldPos + new Vector3(-.5f, -.5f, 0), worldPos + new Vector3(.5f, .5f, 0), Color.red, 5);
#endif
                        SetTile(sq, Tile.Floor);
                    }
                    Assert.IsTrue(node.direction.deltaPosition.sqrMagnitude != 0);
                    sq += node.direction.GetOpposite().deltaPosition;
#if UNITY_EDITOR
                    Debug.DrawLine(worldPos, Game.GridToWorldPosition(sq), Color.green, 5);
#endif
                }
            }
            else {
                Debug.LogWarning("No path found from entry to exit - creating a corridor");
                MakeCorridor(entryPoint, exitPoint, Tile.Bedrock);
            }

            pathToPlayer = path;
            pathToPlayer.Clear();
        }


        SetTile(entryPoint, Tile.Floor);
        SetTile(exitPoint, Tile.Exit);
    }

    private void MakeCorridor(Vector2Int a, Vector2Int b, Tile tile) {
        if(a.x > b.x) {
            var tmp = a;
            a = b;
            b = tmp;
        }
        for(int x = a.x; x <= b.x; ++x) {
            tiles[a.y, x] = tile;
        }
        int endY = Math.Max(a.y, b.y);
        for(int y = Math.Min(a.y, b.y); y <= endY; ++y) {
            tiles[y, b.x] = tile;
        }
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
        int size = neighborWeights.GetLength(0);
        int r = size / 2;
        for(int y = -r; y <= r; ++y) {
            for(int x = -r; x <= r; ++x) {
                Vector2Int p = square + new Vector2Int(x, y);
                Tile neighbor = GetTile(p);
                if((neighbor & soughtTilesMask) != 0) {
                    weight += neighborWeights[y + r, x + r];
                }
            }
        }
        return weight;
    }

    private static float[,] NormalizeWeights(float[,] weights) {
        float total = 0;
        int size = weights.GetLength(0);
        for(int row = 0; row < size; ++row) {
            for(int col = 0; col < size; ++col) {
                total += weights[row, col];
            }
        }
        float[,] result = new float[size, size];
        for(int row = 0; row < size; ++row) {
            for(int col = 0; col < size; ++col) {
                result[row, col] = weights[row, col] / total;
            }
        }
        return result;
    }
}
