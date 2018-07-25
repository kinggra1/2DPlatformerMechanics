using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpriteController : MonoBehaviour {

    private float xSwayMin = -0.2f;
    private float xSwayMax = 0.2f;
    private float ySwayMin = -0.5f;
    private float ySwayMax = -0.5f;

    private PlayerController player;
    private Vector2 target;
    private SpriteRenderer sprite;
    private GameObject body;

    Vector2 velocity = Vector2.zero;

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        body = sprite.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        target = player.transform.position - this.transform.position;
        velocity += target.normalized  + target * Time.deltaTime;
        //velocity *= 0.99f;
        this.transform.position += new Vector3(velocity.x, velocity.y) * Time.deltaTime;
	}
}
