﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Tooltip("How far 'zoomed-out' the camera is in orthographic view space.")]
    public float cameraDistance = 8;

    private GameObject target;
    private PlayerController player;
    private Rigidbody2D playerRigidbody;
    private Camera cam;

    private Vector3 velocity;
    private Vector3 targetPosition;

    private float cameraLerpPercentage = 0.05f;

    private float playerTurnaroundX;
    private float playerGroundedY;
    private Vector2 bottomLeftLimit;
    private Vector2 topRightLimit;

    private float shakeTime = 0f;
    private float trauma = 0f;
    private float cameraShakeMagnitude = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    private float shakeAngle = 0f;
    private readonly float MAX_SHAKE_ANGLE = 10f;
    private readonly float MAX_TRAUMA = 1f;
    private readonly float SHAKE_DECAY_RATE = 0.5f;
    private readonly float SHAKE_SPEED = 10f;

    private readonly Vector3 SHAKE_SCALE = new Vector3(2f, 1.5f, 0f);
    private readonly Vector3 DEFAULT_OFFSET = new Vector3(0f, 2f, 0f);

    private static CameraFollow instance;

    private void Awake() {

        if (instance == null) {
            // Keep this object around between scenes.
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }


        cam = this.GetComponent<Camera>();
        if (cam == null) {
            Debug.LogError("No Camera attached to CameraFollower");
        }
        cam.orthographicSize = cameraDistance;
    }

    // Use this for initialization
    void Start () {
        // This keeps camera lerp consistent if we change the physics timescale.
        cameraLerpPercentage *= (Time.fixedDeltaTime * 60f);

        // Initial/normal target is the player
        target = GameObject.FindWithTag("Player");
        player = target.GetComponentInChildren<PlayerController>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        // Set camera to start on player
        TeleportAfterSceneLoad(player.transform.position);
    }

    public static CameraFollow GetInstance() {
        if (instance == null) {
            Debug.LogError("No camera found in scene :o");
        }
        return instance;
    }

    public void AddShakeTrauma(float change) {
        trauma += change;
        if (trauma > MAX_TRAUMA) {
            trauma = MAX_TRAUMA;
        }
    }

    public void SetPlayerTurnaroundX(float x) {
        playerTurnaroundX = x;
    }

    // We run in FixedUpdate to keep up with the player velocity, which is also set in FixedUpdate
    void FixedUpdate() {

        shakeTime += Time.fixedDeltaTime * SHAKE_SPEED;
        if (trauma > 0f) {
            trauma -= Time.fixedDeltaTime * SHAKE_DECAY_RATE;
        } else {
            trauma = 0f;
        }

        targetPosition = target.transform.position + DEFAULT_OFFSET;
        targetPosition.z = transform.position.z;
        if (playerRigidbody != null) {
            // targetPosition += new Vector3(Mathf.Min(playerRigidbody.velocity.x, 2f), 0f, 0f);// new Vector3(targetRigidbody.velocity.x, targetRigidbody.velocity.y, 0f).normalized * Mathf.Min(targetRigidbody.velocity.magnitude, 2f);
        }

        float xLerpPercentage = 0f;
        float playerBacktrackXDist = Mathf.Abs(playerRigidbody.transform.position.x - playerTurnaroundX);
        if (Mathf.Abs(playerRigidbody.transform.position.x - playerTurnaroundX) > 4f) {
            xLerpPercentage = cameraLerpPercentage * (playerBacktrackXDist - 4f) / (8f - 4f);
        }

        float yLerpPercentage = cameraLerpPercentage;

        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(transform.position.x, targetPosition.x, xLerpPercentage);
        newPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, yLerpPercentage);

        // Add in random screenshake depending on our current shakeRatio
        // Our camera shakes at trauma^2. Since trauma is [0,1], this creates an increasing slope curve from [0,1]
        // so that a the more trauma there is, the more the camera is affected, exponentially
        cameraShakeMagnitude = Mathf.Pow((trauma / MAX_TRAUMA), 2);

        shakeOffset.x = Mathf.PerlinNoise(shakeTime, 10f) - 0.5f;
        shakeOffset.y = Mathf.PerlinNoise(shakeTime, 20f) - 0.5f;
        // Normalize in [-1, 1]
        shakeOffset = shakeOffset * 2f * cameraShakeMagnitude;
        shakeAngle = (Mathf.PerlinNoise(shakeTime, 50f) * 2f - 1f) * cameraShakeMagnitude * MAX_SHAKE_ANGLE;

        newPosition = CapInSceneBounds(newPosition);

        transform.position = newPosition + Vector3.Scale(shakeOffset, SHAKE_SCALE);
        //newPosition = player.transform.position;
        //newPosition.z = this.transform.position.z;
        //transform.position = transform.position;
        transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
        cam.orthographicSize = cameraDistance;
	}

    private Vector3 CapInSceneBounds(Vector3 pos) {

        if (pos.x > topRightLimit.x) {
            pos.x = topRightLimit.x;
        }
        if (pos.x < bottomLeftLimit.x) {
            pos.x = bottomLeftLimit.x;
        }
        if (pos.y > topRightLimit.y) {
            pos.y = topRightLimit.y;
        }
        if (pos.y < bottomLeftLimit.y) {
            pos.y = bottomLeftLimit.y;
        }

        return pos;
    }

    public void SetSceneLimitDims(Vector2 bottomLeft, Vector2 topRight) {
        bottomLeftLimit = bottomLeft;
        topRightLimit = topRight;
    }

    public void SetTarget(GameObject newTarget) {
        target = newTarget;
        playerRigidbody = target.GetComponent<Rigidbody2D>();
    }

    public void TeleportAfterSceneLoad(Vector3 location) {
        // Retain our current offset relative to the player
        location.z = this.transform.position.z;
        this.transform.position = location; // + (transform.position - target.transform.position);
    }
}
