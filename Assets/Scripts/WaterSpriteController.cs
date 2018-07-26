using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpriteController : MonoBehaviour {

    private float xSwayRadius = 0.1f;
    private float ySwayRadius = 0.1f;
    private Vector3 floatingOffset = new Vector3(1.2f, 1f, 0f);

    private PlayerController player;
    private Rigidbody2D playerRigidbody;
    private Vector2 target;
    private Vector3 localOffset = Vector3.zero;
    private SpriteRenderer sprite;
    private GameObject body;
    private ParticleSystem drippingParticles;

    Vector2 velocity = Vector2.zero;

    private float randomSwayFactor;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        sprite = this.GetComponentInChildren<SpriteRenderer>();
        body = sprite.gameObject;

        drippingParticles = this.GetComponentInChildren<ParticleSystem>();

        randomSwayFactor = Random.Range(0.1f, 0.8f);
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 targetOffset = floatingOffset;
        // Mirror the floating offset depending on the direction the player is facing
        if (player.PlayerFacing() == PlayerController.Direction.RIGHT) {
            targetOffset.x = targetOffset.x * -1;
        }

        // "Random" bobbing around
        if (true) {
            localOffset.x = Mathf.Cos(Time.time) * xSwayRadius;
            localOffset.y = Mathf.Cos(Time.time * 2.2f) * ySwayRadius;
        }
        target = player.transform.position + (targetOffset + localOffset) - this.transform.position;
        velocity += target;

        // the closer we are to the player, the more we dampen velocity change
        // Dividing by velocity.magnitude means we'll slow down if we're going faster than
        // [distance-to-player]/second, with stronger decreases the closer we get to the player
        velocity *= Mathf.Clamp(target.magnitude/velocity.magnitude, 0.8f, 1.0f);
        this.transform.position += new Vector3(velocity.x, velocity.y) * Time.deltaTime;

        // point us in the direction of velocity
        sprite.gameObject.transform.localRotation = Quaternion.EulerAngles(0f, 0f, Mathf.Atan2(velocity.y, velocity.x));

        // and stretch based on current velocity
        float scaleFactor = velocity.magnitude / 8f;
        sprite.gameObject.transform.localScale = new Vector3(Mathf.Clamp(scaleFactor, 1f, 2f), 1f-Mathf.Clamp(scaleFactor, 0f, 0.25f), 1f);

        drippingParticles.transform.rotation = Quaternion.Inverse(transform.rotation);
    }
}
