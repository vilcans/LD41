using System.Collections.Generic;
using UnityEngine;

public class Level {

    public class Creature {
        public Vector2Int square;
        public GameObject gameObject;
    }

    public TileMap map;

    public List<Creature> creatures;

    public Level(int depth) {

        //Random.InitState(depth);

        bool small = depth < 5;

        int width;
        int height;
        if(small) {
            width = Random.Range(30, 50);
            height = (40 * 24) / width;
        }
        else {
            width = Random.Range(50, 70);
            height = (60 * 32) / width;
        }

        int iterations;
        if(depth < 7) {
            iterations = 4;
        }
        else {
            int minIterations = depth < 10 ? 1 : 0;
            iterations = System.Math.Max(Random.Range(minIterations, 5), Random.Range(minIterations, 5));
        }

        if(iterations == 0) {
            // Make labyrinth-like levels easier
            width /= 2;
            height /= 2;
        }

        float[,] neighborWeights = TileMap.standardWeights;
        if(depth > 3 && Random.value < .25f) {
            float random = Random.value;
            Debug.LogFormat("Special level! random={0}", random);
            if((random -= .1f) < 0) {
                neighborWeights = TileMap.verticalStripesWeights;
            }
            else if((random -= .1f) < 0) {
                neighborWeights = TileMap.horizontalStripesWeights;
            }
            else if((random -= .1f) < 0) {
                neighborWeights = TileMap.horizontalCorridorWeights;
            }
            else {
                neighborWeights = TileMap.bigHallsWeights;
            }
        }

        int numberOfCreatures;
        /*if(level < 2) {
            numberOfCreatures = 0;
        }*/
        numberOfCreatures = 1;

        Debug.LogFormat("Iterations={0}", iterations);
        map = TileMap.Generate(
            new Vector2Int(width, height),
            iterations,
            neighborWeights
        );

        creatures = new List<Creature>(numberOfCreatures);
        SpawnCreatures(numberOfCreatures);
    }

    private void SpawnCreatures(int numberOfCreatures) {
        int retries = 0;
        for(int i = 0; i < numberOfCreatures; ++i) {
            Vector2Int square;
            do {
                if(++retries > 10) {
                    Debug.Log("Could not spawn all creatures: no space found");
                    return;
                }
                square = new Vector2Int(Random.Range(TileMap.border, map.width - TileMap.border), Random.Range(TileMap.border, map.height - TileMap.border));
            } while(map.GetTile(square) != TileMap.Tile.Floor);

            Creature creature = new Creature {
                square = square,
            };
            creatures.Add(creature);
        }
    }

    public void TickBeat() {
        for(int i = 0, len = creatures.Count; i < len; ++i) {
            Creature creature = creatures[i];
            PathMap.Node node = map.pathToPlayer.nodes[creature.square.y, creature.square.x];
            Debug.LogFormat("Creature found node with cost {0} in direction {1}", node.cost, node.direction.GetCharacter());
            if(node.cost < Mathf.Infinity) {
                creature.square -= node.direction.deltaPosition;
            }
        }
    }

#if UNITY_EDITOR
    public void DebugDraw() {
        for(int i = 0, len = creatures.Count; i < len; ++i) {
            Creature creature = creatures[i];
            Vector3 worldPos = Game.GridToWorldPosition(creature.square);
            Debug.DrawLine(worldPos + new Vector3(-.5f, -.5f, 0), worldPos + new Vector3(.5f,  .5f, 0), Color.yellow);
            Debug.DrawLine(worldPos + new Vector3(-.5f,  .5f, 0), worldPos + new Vector3(.5f, -.5f, 0), Color.yellow);
        }
        map.pathToPlayer.DebugDraw();
    }
#endif
}
