using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Tiles : MonoBehaviour {

    // Wall sprite for each set of neighbor bits
    public Sprite[] wallSprites;

#if UNITY_EDITOR
    public void OnValidate() {
        wallSprites = new Sprite[256];
        for(int i = 0; i < 256; ++i) {
            int tileNumber = TileNumbers.numbers[i];
            string assetName = "Assets/Walls/Wall_" + tileNumber + ".png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetName);
            if(s == null) {
                Debug.LogWarningFormat("Asset not found: {0}", assetName);
            }
            wallSprites[i] = s;
        }
    }
#endif
}
