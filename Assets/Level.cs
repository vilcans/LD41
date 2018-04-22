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

        map = TileMap.Generate(new Vector2Int(width, height));
    }
}
