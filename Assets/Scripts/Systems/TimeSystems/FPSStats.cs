using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSStats : MonoBehaviour {
    private float lastFrameTime;
    private float[] lastFrames = new float[10];
    private int i = 0;
    public Text fpsDisplay;

    // Start is called before the first frame update
    void Start() {
        lastFrameTime = Time.time;
    }

    // Update is called once per frame
    void Update() {
        float fps = (1f / Time.deltaTime);
        lastFrames[i] = fps;
        i = (i + 1) % lastFrames.Length;

        float average = 0f;
        foreach (int val in lastFrames) {
            average += val;
        }
        average /= lastFrames.Length;

        fpsDisplay.text = average.ToString();
        lastFrameTime = Time.time;
    }
}
