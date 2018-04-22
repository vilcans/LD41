using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public Text statusText;
    public Text messageText;

    public Sprite bedrockPrefab;
    public Sprite floorPrefab;
    public Sprite rockPrefab;
    public Sprite exitPrefab;

    private Transform tilesParent;

    private Level level;

    private enum State {
        EnteringLevel,
        Playing,
        Dead,
    }
    private State state;

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

    private int dungeonLevel = 0;
    private int hunger = 0;
    private int maxHunger = 10;
    private int maxHitpoints = 20;
    private int hitPoints = 20;

    public void Awake() {
        cameraTransform = Camera.main.transform;

        samplesPerBeat = music.clip.frequency * 60 / bpm;
        NewLevel();
    }

    public void NewLevel() {
        ++dungeonLevel;
        level = new Level(dungeonLevel);

        UpdateStatusText();

        int numberOfTiles = level.map.width * level.map.height;
        if(tilesParent != null) {
            Destroy(tilesParent.gameObject);
            tilesParent = null;
        }
        tilesParent = new GameObject("Level").transform;
        tilesParent.hierarchyCapacity = Math.Max(tilesParent.hierarchyCapacity, numberOfTiles + 10);

        for(int row = 0; row < level.map.height; ++row) {
            for(int column = 0; column < level.map.width; ++column) {
                Vector2Int square = new Vector2Int(column, row);
                TileMap.Tile tile = level.map.GetTile(square);
                CreateTile(tile, square);
            }
        }

        playerPosition = level.map.entryPoint;
        cameraTransform.position = GridToWorldPosition(playerPosition) + cameraOffset;
        state = State.EnteringLevel;
    }

    private GameObject CreateTile(TileMap.Tile tile, Vector2Int square) {
        GameObject obj = new GameObject("Tile" + square);
        Transform t = obj.transform;
        t.SetParent(tilesParent);
        t.localPosition = Game.GridToWorldPosition(square) + Vector3.forward;
        obj.isStatic = true;

        Sprite sprite;
        switch(tile) {
            case TileMap.Tile.Bedrock:
                sprite = bedrockPrefab;
                break;
            case TileMap.Tile.Floor:
                sprite = floorPrefab;
                break;
            case TileMap.Tile.Rock:
                sprite = rockPrefab;
                break;
            case TileMap.Tile.Exit:
                sprite = exitPrefab;
                break;
            default:
                throw new ApplicationException("Unhandled tile type: " + tile);
        }
        SpriteRenderer r = obj.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        return obj;
    }

    public void Update() {
        if(Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene("Main");
        }

        if(state == State.EnteringLevel) {
            state = State.Playing;
            return;
        }
        if(state != State.Playing) {
            return;
        }

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

        if(isReady && level.map.GetTile(playerPosition) == TileMap.Tile.Exit) {
            NewLevel();
            return;
        }

        Vector2Int moveDestination = playerPosition + direction.deltaPosition;
        TileMap.Tile destinationTile = level.map.GetTile(moveDestination);
        bool canMove = isReady && ((destinationTile & (TileMap.Tile.Floor | TileMap.Tile.Exit)) != 0);

#if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.RightArrow)) {
            playerPosition += new Vector2Int(1, 0);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow)) {
            playerPosition += new Vector2Int(-1, 0);
        }
        if(Input.GetKeyDown(KeyCode.UpArrow)) {
            playerPosition += new Vector2Int(0, 1);
        }
        if(Input.GetKeyDown(KeyCode.DownArrow)) {
            playerPosition += new Vector2Int(0, -1);
        }
#endif

        if(Input.GetKeyDown(KeyCode.Space) && canMove) {
            messageText.text = "";
            playerPosition += direction.deltaPosition;
            moveSound.Play();
            if(beatFraction <= .2f || beatFraction >= .9f) {
                AddHunger(1);
                nextPossibleStepBeat = roundedBeat + 1;
            }
            else {
                outOfSyncSound.Play();
                AddHunger(5);
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

    private void AddHunger(int points) {
        hunger += points;
        if(hunger > maxHunger) {
            AddDamage(hunger - maxHunger, "starvation");
            hunger = maxHunger;
        }
        UpdateStatusText();
    }

    private void AddDamage(int hit, string cause) {
        hitPoints -= hit;
        if(hitPoints <= 0) {
            hitPoints = 0;
            UpdateStatusText();
            state = State.Dead;
            AddMessage("You were killed by " + cause);
            AddMessage("Press R to restart");
            music.Stop();
        } 
    }
    private void AddMessage(string text) {
        messageText.text += text + "\n";
    }

    private void UpdateStatusText() {
        statusText.text = "Dlvl:" + dungeonLevel + " HP:" + hitPoints + "(" + maxHitpoints + ") Hunger:" + hunger + "(" + maxHunger + ")";
    }
}
