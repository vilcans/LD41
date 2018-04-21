using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    public AudioSource music;
    public Transform rotator;

    private float bpm = 120;
    private float samplesPerBeat;

    public void Awake() {
        samplesPerBeat = music.clip.frequency * 60 / bpm;
    }

    public void Update() {
        int time = music.timeSamples;
        float beat = time / samplesPerBeat;
        beat = Mathf.Floor(beat);
        float angle = beat * 90;

        rotator.rotation = Quaternion.AngleAxis(-angle, Vector3.forward);
    }
}
