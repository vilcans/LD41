using UnityEngine;

public class Level {

    public TileMap map;

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

        Debug.LogFormat("Iterations={0}", iterations);
        map = TileMap.Generate(
            new Vector2Int(width, height),
            iterations,
            neighborWeights
        );
    }
}
