using UnityEngine;

public class Level {

    public TileMap map;

    public Level(int depth) {

        //Random.InitState(depth);

        Vector2Int size;
        if(depth < 5) {
            size = new Vector2Int(40, 24);
        }
        else {
            size = new Vector2Int(60, 32);
        }
        map = TileMap.Generate(size);
    }
}
