using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Game : MonoBehaviour {

    public AudioSource music;
    public AudioSource moveSound;
    public AudioSource outOfSyncSound;

    public Transform playerTransform;
    public Transform visualTransform;
    public Transform rotator;
    public SpriteRenderer arrowSprite;
    public Gradient arrowColors;
    public Gradient arrowColorsNoMove;

    public Level level;

    private Transform cameraTransform;
    private Vector3 cameraVelocity;
    private Vector3 cameraOffset = new Vector3(0, 0, -10);

    public struct Direction {
        public Vector2Int deltaPosition;

        public static Direction up = new Direction { deltaPosition = new Vector2Int(0, 1) };
        public static Direction right = new Direction { deltaPosition = new Vector2Int(1, 0) };
        public static Direction down = new Direction { deltaPosition = new Vector2Int(0, -1) };
        public static Direction left = new Direction { deltaPosition = new Vector2Int(-1, 0) };

        public static Direction[] directions = { up, right, down, left };

        public Direction GetOpposite() {
            return new Direction { deltaPosition = deltaPosition * -1 };
        }

        public char GetCharacter() {
            if(deltaPosition == up.deltaPosition) {
                return '^';
            }
            if(deltaPosition == right.deltaPosition) {
                return '>';
            }
            if(deltaPosition == down.deltaPosition) {
                return 'v';
            }
            if(deltaPosition == left.deltaPosition) {
                return '<';
            }
            Assert.IsTrue(false, "Unknown Direction: " + this);
            return '?';
        }
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
        cameraTransform = Camera.main.transform;

        samplesPerBeat = music.clip.frequency * 60 / bpm;
        NewLevel();
    }

    public void NewLevel() {
        level.Create();
        playerPosition = level.map.entryPoint;
        cameraTransform.position = GridToWorldPosition(playerPosition) + cameraOffset;
    }

    public void Update() {
#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Backspace)) {
            NewLevel();
        }
#endif

        float previousFraction = beatFraction;
        float beatWithFraction = music.timeSamples / samplesPerBeat;
        beatFraction = beatWithFraction - Mathf.Floor(beatWithFraction);
        if(beatFraction < previousFraction) {
            ++beatNumber;
        }

        int roundedBeat = (int)(beatNumber + beatFraction + .1f);
        int movementIndex = (roundedBeat - nextPossibleStepBeat + lastMoveDirection + 4) % 4;

        Direction direction = Direction.directions[movementIndex];

        if(beatNumber >= nextPossibleStepBeat) {
            float r = Mathf.Pow(beatFraction, 7);
            rotator.rotation = Quaternion.AngleAxis((beatNumber - nextPossibleStepBeat + lastMoveDirection + r) * -90, Vector3.forward);
        }
        else {
            rotator.rotation = Quaternion.AngleAxis(lastMoveDirection * -90, Vector3.forward);
        }

        bool isReady = roundedBeat >= nextPossibleStepBeat;
        if(isReady) {
            arrowSprite.color = arrowColors.Evaluate(beatFraction);
        }
        else {
            arrowSprite.color = arrowColorsNoMove.Evaluate(roundedBeat + 1 == nextPossibleStepBeat ? beatFraction : 0);
        }

        Vector2Int moveDestination = playerPosition + direction.deltaPosition;
        TileMap.Tile destinationTile = level.map.GetTile(moveDestination);
        bool canMove = isReady && destinationTile == TileMap.Tile.Floor;

        if(Input.GetKeyDown(KeyCode.Space) && canMove) {
            playerPosition += direction.deltaPosition;
            moveSound.Play();
            if(beatFraction <= .2f || beatFraction >= .9f) {
                nextPossibleStepBeat = roundedBeat + 1;
            }
            else {
                outOfSyncSound.Play();
                nextPossibleStepBeat = roundedBeat + 2;
            }
            lastMoveDirection = movementIndex;
        }

        Vector3 position = GridToWorldPosition(playerPosition);
        playerTransform.position = position;
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, position + cameraOffset, ref cameraVelocity, 1.0f);
        visualTransform.localScale = Vector3.one * Mathf.Lerp(1.3f, 1.0f, 1.0f - 1.0f / (beatFraction + 1));
    }

    public static Vector3 GridToWorldPosition(Vector2Int p) {
        return new Vector3(p.x, p.y, 0);
    }
}
