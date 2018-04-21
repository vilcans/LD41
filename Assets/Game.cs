using UnityEngine;

public class Game : MonoBehaviour {

    public AudioSource music;
    public Transform playerTransform;
    public Transform visualTransform;
    public Transform rotator;

    private struct Direction {
        public Vector2Int deltaPosition;

        public static Direction up = new Direction { deltaPosition = new Vector2Int(0, 1) };
        public static Direction right = new Direction { deltaPosition = new Vector2Int(1, 0) };
        public static Direction down = new Direction { deltaPosition = new Vector2Int(0, -1) };
        public static Direction left = new Direction { deltaPosition = new Vector2Int(-1, 0) };
        public static Direction[] directions = { up, right, down, left };
    }

    private float bpm = 120;
    private float samplesPerBeat;

    private Vector2Int playerPosition;

    public void Awake() {
        samplesPerBeat = music.clip.frequency * 60 / bpm;
    }

    public void Update() {
        int time = music.timeSamples;
        float beat = time / samplesPerBeat;

        int index = (int)(beat + .5) % 4;

        Direction direction = Direction.directions[index];

        rotator.rotation = Quaternion.AngleAxis(-(int)(beat) * 90, Vector3.forward);

        if(Input.anyKeyDown) {
            playerPosition += direction.deltaPosition;
            float diff = (int)(beat) - beat;
            Debug.LogFormat("diff = {0}", diff);
        }

        playerTransform.position = GridToWorldPosition(playerPosition);
        visualTransform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1.0f, 1.0f - 1.0f / (beat - (int)beat + 1));
    }

    private Vector3 GridToWorldPosition(Vector2Int p) {
        return new Vector3(p.x, p.y, 0);
    }
}
