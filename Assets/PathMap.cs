using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class PathMap {

    public TileMap map;
    public int updatesPerTick = 50;

    private Vector2Int targetSquare = new Vector2Int(-9999, -9999);

    private int height;
    private int width;

    private float maxCost = Mathf.Infinity;

    private struct PathItem {
        public Vector2Int square;
        public Game.Direction fromDirection;
        public float cost;
        public override string ToString() {
            return square + " from " + fromDirection + " cost " + cost;
        }
        public PathItem(Vector2Int square, Game.Direction fromDirection, float cost) {
            this.square = square;
            this.fromDirection = fromDirection;
            this.cost = cost;
        }
    };

    private Queue<PathItem> path = new Queue<PathItem>();

    public struct Node {
        // Best cost between this node and target
        public float cost;

        // In which direction that cost is
        public Game.Direction direction;
    }

    public Node[,] nodes;

    public PathMap(TileMap map) {
        this.map = map;
        height = map.height;
        width = map.width;
        nodes = new Node[height, width];
        Clear();
    }

    public void Clear() {
        for(var row = 0; row < height; ++row) {
            for(var col = 0; col < width; ++col) {
                nodes[row, col] = new Node {
                    cost = Mathf.Infinity,
                    direction = Game.Direction.left,
                };
            }
        }
    }

    public void Tick() {
        if(path == null) {
            Debug.LogWarning("path is null in " + this);
            path = new Queue<PathItem>();
        }
        if(path.Count != 0) {
            //Debug.Log(this + " Path queue " + path.Count);
            for(int i = 0; i < updatesPerTick && path.Count != 0; ++i) {
                UpdateOneStep();
            }
        }
        if(Input.GetKeyDown(KeyCode.P)) {
            Print();
        }
    }

    /*
    public Game.Direction GetDirection(int col, int row) {
        return directions[map.GetIndex(col, row)];
    }

    public float GetCostToGoal(int col, int row) {
        return costs[map.GetIndex(col, row)];
    }*/

    public void TileChanged(Vector2Int square) {
        float bestCost = Mathf.Infinity;
        Game.Direction bestDirection = Game.Direction.left;
        for(int i = 0, len = Game.Direction.directions.Length; i < len; ++i) {
            Game.Direction direction = Game.Direction.directions[i];
            Vector2Int newSq = square + direction.deltaPosition;
            if(!map.IsInBounds(newSq)) {
                continue;
            }
            float costInDirection = nodes[newSq.y, newSq.x].cost;
            if(costInDirection < bestCost) {
                bestCost = costInDirection;
                bestDirection = direction;
            }
        }
        Debug.Log("Best cost from " + square + " is " + bestCost + " in direction " + bestDirection);

        Node node = nodes[square.y, square.x];
        node.cost = bestCost + map.GetCost(square);
        node.direction = bestDirection.GetOpposite();
        nodes[square.y, square.x] = node;

        UpdateAroundTile(square, node.cost);
    }

    public void StartSearch(Vector2Int target, float maxCost) {
        this.maxCost = maxCost;
        if(target == targetSquare) {
            return;
        }
        for(var row = 0; row < height; ++row) {
            for(var col = 0; col < width; ++col) {
                Node node = nodes[row, col];
                node.cost = Mathf.Infinity;
                nodes[row, col] = node;
            }
        }
        path.Clear();
        targetSquare = target;
        path.Enqueue(new PathItem(targetSquare, Game.Direction.left, 0));
    }

    public void UpdateAll() {
        int iterations = 0;
        while(path.Count != 0) {
            UpdateOneStep();
            ++iterations;
            if(iterations > 1000000) {
                throw new System.Exception("Lots of iterations");
            }
        }
        Debug.Log("Direction map updated in " + iterations + " iterations");
    }

    public bool UpdateUntilPathFound(Vector2Int from) {
        int iterations = 0;
        while(nodes[from.y, from.x].cost == Mathf.Infinity) {
            if(path.Count == 0) {
                Debug.Log("No path found after " + iterations + " iterations");
                return false;
            }
            UpdateOneStep();
            ++iterations;
            if(iterations > 1000000) {
                throw new System.Exception("Lots of iterations");
            }
        }
        Debug.Log("Direction map updated in " + iterations + " iterations");
        return true;
    }

    private void UpdateOneStep() {
        if(path.Count == 0) {
            return;
        }

        PathItem item = path.Dequeue();
        float oldCost = nodes[item.square.y, item.square.x].cost;
        if(oldCost <= item.cost) {
            //Debug.Log("didn't beat cost " + oldCost + " with " + item);
            return;
        }
        nodes[item.square.y, item.square.x] = new Node {
            cost = item.cost,
            direction = item.fromDirection,
        };
        UpdateAroundTile(item.square, item.cost);

        return;
    }

    private void UpdateAroundTile(Vector2Int square, float tileCost) {
        for(int i = 0, len = Game.Direction.directions.Length; i < len; ++i) {
            Game.Direction direction = Game.Direction.directions[i];
            Vector2Int newSq = square + direction.deltaPosition;
            if(!map.IsInBounds(newSq)) {
                continue;
            }
            float newCost = tileCost + map.GetCost(newSq);
            if(newCost < maxCost && nodes[newSq.y, newSq.x].cost > newCost) {
                PathItem newItem = new PathItem(newSq, direction, newCost);
                path.Enqueue(newItem);
            }
        }
    }

    public void Print() {
        Debug.Log("Direction map " + this);
        for(int row = 0; row < height; ++row) {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            s.AppendFormat("{0,-3}: ", row);
            for(int col = 0; col < width; ++col) {
                Node node = nodes[row, col];
                float w = node.cost;
                if(w == Mathf.Infinity) {
                    s.Append("  99.9");
                }
                else {
                    s.AppendFormat("{0,5:F1}", w);
                }
                s.Append(node.direction.GetCharacter());
                s.Append(' ');
            }
            Debug.Log(s.ToString());
        }
    }

#if UNITY_EDITOR
    public void DebugDraw() {
        for(int row = 0; row < height; ++row) {
            for(int col = 0; col < width; ++col) {
                StringBuilder s = new StringBuilder();
                Node node = nodes[row, col];
                float w = node.cost;
                if(w >= 99.9f) {
                    s.Append("  99.9");
                }
                else {
                    s.AppendFormat("{0,5:F1}", w);
                }
                s.Append(node.direction.GetCharacter());

                Vector3 worldPos = Game.GridToWorldPosition(new Vector2Int(col, row));
                //Handles.Label(worldPos, s.ToString());
                Vector3 deltaPosition = new Vector3(
                    node.direction.deltaPosition.x * -.45f,
                    node.direction.deltaPosition.y * -.45f,
                    0
                );
                Debug.DrawLine(worldPos, worldPos + deltaPosition);
            }
        }
    }
#endif
}
