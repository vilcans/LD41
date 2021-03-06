﻿using System.Collections.Generic;
using UnityEngine;

public class Level {

    public class Creature {
        public Vector2Int square;
        public GameObject gameObject;
        public Transform transform;

        // If the cost for going to the player is less than this, will attack
        public float aggressivity;

        // How old trace this creature will follow
        public int memory;

        public bool inPursuit;

        public string name;

        public int hitPoints = 5;

        public bool GetDamage(out int damage) {
            if(Random.value < .5f) {
                damage = Random.Range(1, 10);
                return true;
            }
            else {
                damage = 0;
                return false;
            }
        }

        public void AddDamage(int damage) {
            hitPoints -= damage;
            if(hitPoints < 0) {
                hitPoints = 0;
            }
        }

        public void Destroy() {
            Object.Destroy(gameObject);
            gameObject = null;
            transform = null;
        }
    }

    public TileMap map;

    public List<Creature> creatures;

    private int dungeonLevel;

    public Level(int depth) {

        //Random.InitState(depth);
        dungeonLevel = depth;

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
        if(depth < 2) {
            numberOfCreatures = 0;
        }
        else if(depth == 2) {
            numberOfCreatures = 1;
        }
        else {
            numberOfCreatures = Random.Range(1 + depth / 8, depth / 2);
        }

        int foodDrops;
        if(depth == 1) {
            foodDrops = 1;
        }
        else {
            foodDrops = 2;
        }

        Debug.LogFormat("Iterations={0}", iterations);
        map = TileMap.Generate(
            new Vector2Int(width, height),
            iterations,
            neighborWeights,
            foodDrops
        );

        creatures = new List<Creature>(numberOfCreatures);
        SpawnCreatures(numberOfCreatures);
        Debug.LogFormat("Spawned {0} out of requested {1}", creatures.Count, numberOfCreatures);
    }

    private void SpawnCreatures(int numberOfCreatures) {
        int retries = 0;
        for(int i = 0; i < numberOfCreatures; ++i) {
            Vector2Int square;
            do {
                if(++retries > 1000) {
                    Debug.Log("Could not spawn all creatures: no space found");
                    return;
                }
                square = map.GetRandomSquare();
            } while(!IsFree(square));

            bool badass = (Random.value < (dungeonLevel - 4) * .10f);
            Creature creature = new Creature {
                square = square,
                aggressivity = badass ? 20 : 10,
                memory = 5,
                name = "Ghost",
                hitPoints = badass ? 15 : 5
            };
            creatures.Add(creature);
        }
    }

    private bool IsFree(Vector2Int square) {
        if(!map.IsWalkable(square)) {
            return false;
        }
        return FindCreature(square) == null;
    }

    public Creature FindCreature(Vector2Int square) {
        for(int i = 0, len = creatures.Count; i < len; ++i) {
            if(creatures[i].square == square) {
                return creatures[i];
            }
        }
        return null;
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
