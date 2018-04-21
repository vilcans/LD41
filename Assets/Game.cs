using UnityEngine;

public class Game : MonoBehaviour {

    public AudioSource music;
    public AudioSource moveSound;
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

    private int beatNumber;
    private float beatFraction;
    private int nextPossibleStepBeat = 0;
    private int lastMoveDirection = 0;
    private int indexOffset;

    public void Awake() {
        samplesPerBeat = music.clip.frequency * 60 / bpm;
    }

    public void Update() {
        float previousFraction = beatFraction;
        float beatWithFraction = music.timeSamples / samplesPerBeat;
        beatFraction = beatWithFraction - Mathf.Floor(beatWithFraction);
        if(beatFraction < previousFraction) {
            ++beatNumber;
        }

        int roundedBeat = (int)(beatNumber + beatFraction + .1f);
        int movementIndex = (roundedBeat - nextPossibleStepBeat + lastMoveDirection + 4) % 4;

        Direction direction = Direction.directions[movementIndex];

        float r = Mathf.Pow(beatFraction, 7);
        rotator.rotation = Quaternion.AngleAxis((beatNumber - nextPossibleStepBeat + lastMoveDirection + r)  * -90, Vector3.forward);

        bool canMove = roundedBeat >= nextPossibleStepBeat;
        rotator.gameObject.SetActive(beatNumber >= nextPossibleStepBeat);

        if(Input.GetKeyDown(KeyCode.Space) && canMove) {
            playerPosition += direction.deltaPosition;
            moveSound.Play();
            //float diff = beat - beatWithFraction;
            //Debug.LogFormat("diff = {0}", diff);
            nextPossibleStepBeat = roundedBeat + 1;
            lastMoveDirection = movementIndex;
        }

        playerTransform.position = GridToWorldPosition(playerPosition);
        visualTransform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1.0f, 1.0f - 1.0f / (beatFraction + 1));
    }

    private Vector3 GridToWorldPosition(Vector2Int p) {
        return new Vector3(p.x, p.y, 0);
    }
}
