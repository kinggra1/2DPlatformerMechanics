using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHand : MonoBehaviour {

    [Tooltip("The object that is the position/scale parent of the weapon itself. May move around with weapon usage.")]
    public GameObject weaponParent;

    private PlayerController player;

    private float animationTimer = 0f;
    private float animationDuration = 0.2f;

    private Rigidbody2D rb;
    private GameObject weaponObject = null;
    private ItemWeapon weapon = null;

    private bool usingWeapon = false;

    // Array to hold Physics collision results for the beginning of each weapon swing (using Rigidbody2D.OverlapCollider)
    private Collider2D[] results = new Collider2D[100];

    // History hold all objects hit on this attack so we don't double collide on a single attack.
    HitObjectHistory hitHistory = new HitObjectHistory();

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
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
        if (!usingWeapon) {
            hitHistory.Clear();
            StartCoroutine(SwordAnimationCoroutine());
        }
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
            if (strikeable != null && !hitHistory.HaveHit(other.gameObject)) {
                strikeable.Strike(this.transform.position, this.weapon);
                hitHistory.MarkObjectAsHit(other.gameObject);
            }

            // If this is on the enemy layer, give us a little knockback when we hit it.
            // TODO: Figure out how to do "pogo" knockback for swinging downwards later
            if (1 << other.gameObject.layer == AI.EnemyLayermask) {
                // -1 or 1 exclusively in x, 0 on y, depending on position relative to enemy
                Vector2 hKnockback = (player.transform.position - other.transform.position).x < 0f ? Vector2.left : Vector2.right;
                Debug.Log(hKnockback);
                player.GetPushed(hKnockback * 10f, 0.1f);

            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other) {
        HandleWeaponHit(other);
    }





    private class HitObjectHistory {

        private Dictionary<int, GameObject> lookupTable = new Dictionary<int, GameObject>();

        public void MarkObjectAsHit(GameObject obj) {
            lookupTable.Add(obj.GetHashCode(), obj);
        }

        public bool HaveHit(GameObject obj) {
            return lookupTable.ContainsKey(obj.GetHashCode());
        }

        public void Clear() {
            lookupTable.Clear();
        }

        public List<GameObject> AllObjects() {
            List<GameObject> objects = new List<GameObject>();
            foreach (GameObject obj in lookupTable.Values) {
                objects.Add(obj);
            }

            return objects;
        }
    }
}