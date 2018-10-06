using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [Tooltip("How far 'zoomed-out' the camera is in orthographic view space.")]
    public float cameraDistance = 8;

    private GameObject target;
    private PlayerController player;
    private Rigidbody2D playerRigidbody;
    private bool trackAhead = false;
    private Camera cam;

    private Vector3 velocity;
    private Vector3 targetPosition;

    private float cameraLerpPercentage = 0.05f;

    private float playerTurnaroundX;
    private float playerGroundedY;

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

    // Use this for initialization
    void Start () {
        // This keeps camera lerp consistent if we change the physics timescale.
        cameraLerpPercentage *= (Time.fixedDeltaTime * 60f);

        // Initial/normal target is the player
        target = GameObject.FindWithTag("Player");
        player = target.GetComponentInChildren<PlayerController>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        cam = this.GetComponent<Camera>();
        if (cam == null) {
            Debug.LogError("No Camera attached to CameraFollower");
        }
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

        targetPosition = target.transform.position;
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
        /*
        if (player.IsGrounded()) {
            yLerpPercentage = cameraLerpPercentage;
        }
        */
        /*
        float playerBacktrackYDist = Mathf.Abs(playerRigidbody.transform.position.y - lastPlayerPosition.y);
        if (Mathf.Abs(playerRigidbody.transform.position.y - lastPlayerPosition.y) > 4f) {
            yLerpPercentage = cameraLerpPercentage * (playerBacktrackYDist - 4f) / (8f - 4f);
        }
        */

        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(transform.position.x, targetPosition.x, xLerpPercentage);
        newPosition.y = Mathf.Lerp(transform.position.y, targetPosition.y, yLerpPercentage);
        //Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Pow(1f - 0.05f, Time.fixedDeltaTime * 30f));
        //Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.3f);

        // Add in random screenshake depending on our current shakeRatio
        // Our camera shakes at trauma^2. Since trauma is [0,1], this creates an increasing slope curve from [0,1]
        // so that a the more trauma there is, the more the camera is affected, exponentially
        cameraShakeMagnitude = Mathf.Pow((trauma / MAX_TRAUMA), 2);

        shakeOffset.x = Mathf.PerlinNoise(shakeTime, 10f) - 0.5f;
        shakeOffset.y = Mathf.PerlinNoise(shakeTime, 20f) - 0.5f;
        // Normalize in [-1, 1]
        shakeOffset = shakeOffset * 2f * cameraShakeMagnitude;
        shakeAngle = (Mathf.PerlinNoise(shakeTime, 50f) * 2f - 0.5f) * cameraShakeMagnitude;

        transform.position = newPosition + Vector3.Scale(shakeOffset, SHAKE_SCALE);
        //newPosition = player.transform.position;
        //newPosition.z = this.transform.position.z;
        //transform.position = transform.position;
        transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
        cam.orthographicSize = cameraDistance;
	}

    public void setTarget(GameObject newTarget) {
        target = newTarget;
        playerRigidbody = target.GetComponent<Rigidbody2D>();
    }
}
