using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHand : MonoBehaviour {

    [Tooltip("The object that is the position/scale parent of the weapon itself. May move around with weapon usage.")]
    public GameObject weaponParent;

    private float animationTimer = 0f;
    private float animationDuration = 0.2f;

    private Rigidbody2D rb;
    private GameObject weaponObject = null;
    private ItemWeapon weapon = null;

    private bool usingWeapon = false;

    // Array to hold Physics collision results for the beginning of each weapon swing (using Rigidbody2D.OverlapCollider)
    private Collider2D[] results = new Collider2D[100];

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // TODO: Make this more memory efficient. Save references to weapons and activate/deactivate them
    public void SetWeapon(ItemWeapon weaponItem) {
        if (weaponObject) {
            Destroy(weaponObject);
        }
        if (weaponItem == null) {
            return;
        }

        weaponObject = Instantiate(weaponItem.inGamePrefab, transform) as GameObject;
        weaponObject.transform.localPosition = Vector3.zero;

        weapon = weaponItem;
    }

    // TODO: Take in a time to specify speed
    public void SwingSword() {
        StartCoroutine(SwordAnimationCoroutine());
    }







    // Animation Coroutines
    private IEnumerator SwordAnimationCoroutine() {
        animationTimer = 0f;
        usingWeapon = true;

        float startAngle = -30f;
        float endAngle = 90f;
        this.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle);

        // Check once to see if sword is already "hitting" something before we swing
        // Build up a list of all colliders in the weapon object and check all things they're overlapping with
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Collider2D[] results = new Collider2D[100];

        int hits = rb.OverlapCollider(filter, results);
        for (int i = 0; i < hits; i++) {
            Collider2D other = results[i];
            HandleWeaponHit(other);
        }


        while (animationTimer < animationDuration) {

            animationTimer += Time.deltaTime;

            float lerpPercentage = animationTimer / animationDuration;

            // This flips us left/right depending on which way the player is facing
            float direction = transform.localScale.x;

            float newAngle = Mathf.LerpAngle(startAngle, endAngle, lerpPercentage) * direction;
            this.transform.localRotation = Quaternion.Euler(0f, 0f, newAngle);
            yield return null;
        }

        // Reset to where we started
        this.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle);
        usingWeapon = false;
    }

    private void HandleWeaponHit(Collider2D other) {
        // we can only "hit" something if we're using our weapon, not when it's just sitting idle
        if (usingWeapon) {
            IStrikeable strikeable = other.GetComponent<IStrikeable>();
            if (strikeable != null) {
                strikeable.Strike(this.transform.position, this.weapon);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other) {
        HandleWeaponHit(other);
    }
}
