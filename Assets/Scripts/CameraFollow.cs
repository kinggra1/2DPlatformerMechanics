using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public float cameraDistance = -5f;

    private GameObject target;
    private Rigidbody2D targetRigidbody;
    private bool trackAhead = false;
    private Camera cam;

    private Vector3 velocity;

	// Use this for initialization
	void Start () {
        // Initial/normal target is the player
        target = GameObject.FindWithTag("Player");
        targetRigidbody = target.GetComponent<Rigidbody2D>();

        cam = this.GetComponent<Camera>();
        if (cam == null) {
            Debug.LogError("No Camera attached to CameraFollower");
        }
	}
	
	// Update is called once per frame
    void FixedUpdate () {

        Vector3 targetPosition = target.transform.position;
        targetPosition.z = transform.position.z;
        if (targetRigidbody != null) {
            targetPosition += new Vector3(targetRigidbody.velocity.x, targetRigidbody.velocity.y, 0f).normalized * Mathf.Min(targetRigidbody.velocity.magnitude, 2f);
        }

        //float lerpPercentage = 0.1f;
        //Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, lerpPercentage);
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.3f);

        transform.position = newPosition;
        cam.orthographicSize = cameraDistance;
	}

    public void setTarget(GameObject newTarget) {
        target = newTarget;
        targetRigidbody = target.GetComponent<Rigidbody2D>();
    }
}
