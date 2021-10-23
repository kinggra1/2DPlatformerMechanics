using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Private nested controller for weapon controlling logic.
public class PlayerWeaponController : MonoBehaviour {
    private static PlayerWeaponController instance = null;

    [Tooltip("The object that is the position/scale parent of the weapon itself. May move around with weapon usage.")]
    public GameObject weaponParent;

    [Tooltip("Helper prefab for building in-game rope to swing from.")]
    public GameObject ropeSegmentPrefab;

    private PlayerController player;

    private float animationTimer = 0f;
    private float animationDuration = 0.1f;

    private Rigidbody2D rb;
    private GameObject weaponObject = null;
    private ItemWeapon weapon = null;

    private bool usingWeapon = false;

    // HingeJoint2D in the player object where ropes will be attached when we swing.
    private HingeJoint2D ropeConnectionPoint;
    private GameObject ropeParent;

    // Array to hold Physics collision results for the beginning of each weapon swing (using Rigidbody2D.OverlapCollider)
    private Collider2D[] results = new Collider2D[100];

    // History hold all objects hit on this attack so we don't double collide on a single attack.
    HitObjectHistory hitHistory = new HitObjectHistory();

    private float specialAttackTimer = 0f;
    private readonly float SPECIAL_ATTACK_COOLDOWN = 1f;

    void Awake() {
        if (instance == null) {
            // Keep this object around between scenes.
            DontDestroyOnLoad(this.transform.parent.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(this.transform.gameObject);
        }
    }

    internal static PlayerWeaponController GetInstance() {
        if (instance == null) {
            Debug.LogError("No PlayerWeaponController Intiialized.");
        }
        return instance;
    }

    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        ropeConnectionPoint = player.GetComponentInChildren<HingeJoint2D>();
        ropeConnectionPoint.enabled = false;
    }

    void Update() {
        if (specialAttackTimer < SPECIAL_ATTACK_COOLDOWN) {
            specialAttackTimer += Time.deltaTime;
        }
    }

    // TODO: Make this more memory efficient? Save references to weapons and activate/deactivate them
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

    public bool HasRope() {
        return ropeConnectionPoint.enabled;
    }

    public void BreakRope() {
        // Destroy rope object, and disable HingeJoint2D in player object.
        Destroy(ropeParent);
        ropeConnectionPoint.enabled = false;
    }

    private bool AttemptToMakeRope() {
        // If we already have a rope, fail.
        if (this.HasRope()) {
            return false;
        }

        // How far we can reach out to hit an anchorpoint. May be longer than length of whip.
        float whipReach = 8f;
        // How long the actual whip we're swinging on is.
        float whipLength = 5f;
        // The number of links that whipLength is divided across.
        int linkCount = 8;

        float linkSeperation = whipLength / linkCount;

        Vector2 weaponHandPos = this.transform.position;
        Vector2 diagonal = (AI.DirectionToVector2(player.PlayerFacing()) + Vector2.up).normalized;
        RaycastHit2D hit2D = Physics2D.Linecast(weaponHandPos, weaponHandPos + diagonal * whipReach, AI.StandableRaycastLayers);
        Debug.DrawLine(weaponHandPos, weaponHandPos + diagonal * whipReach, Color.blue);
        if (hit2D) {
            ropeParent = new GameObject();
            Vector2 startPos = hit2D.point;
            Vector2 endPos = this.transform.position;
            // int linkCount = (int)(Vector2.Distance(startPos, endPos) / 1f); // LINK SPACING CONSTANT of 0.5f m
            // linkCount = Mathf.Max(2, linkCount); // Ensure 2 links. Avoid divide by 0.
            // linkCount = 8;
            // float linkSpacing = Vector2.Distance(startPos, endPos) / (linkCount - 1);
            Rigidbody2D previousLinkBody = null;
            for (int i = 0; i < linkCount; i++) {
                Vector2 linkPos = Vector2.Lerp(startPos, endPos, i / (linkCount - 1));
                GameObject link = Instantiate(ropeSegmentPrefab, ropeParent.transform) as GameObject;
                link.transform.position = linkPos;
                Rigidbody2D linkRigidbody = link.GetComponent<Rigidbody2D>();
                HingeJoint2D joint = link.GetComponent<HingeJoint2D>();

                // First attachment. Hook to the wall/ceiling itself.
                if (previousLinkBody == null) {
                    joint.autoConfigureConnectedAnchor = true;
                    joint.connectedBody = hit2D.rigidbody;
                }
                else {
                    joint.connectedBody = previousLinkBody;
                    joint.connectedAnchor = new Vector2(0f, -linkSeperation);

                    // TESTING: Also add a sprint joint between each hinge for finer control over Physics.
                    //GameObject springSpacer = new GameObject("SpringSpacer");
                    //springSpacer.transform.position = linkPos;
                    //Rigidbody2D springBody = springSpacer.AddComponent<Rigidbody2D>();
                    //springBody.mass = 0.01f;
                    //SpringJoint2D springJoint = springSpacer.AddComponent<SpringJoint2D>();

                    //springJoint.connectedBody = previousLinkBody;
                    //joint.connectedBody = springBody;
                    //joint.connectedAnchor = new Vector2(0f, -linkSeperation);
                }
                previousLinkBody = linkRigidbody;
            }
            // Connect the last link to the player's hand.
            ropeConnectionPoint.enabled = true;
            ropeConnectionPoint.connectedBody = previousLinkBody;
            return true;
        }
        return false;
    }

    public void UseWeapon(Item.Weapon weaponType) {
        switch (weaponType) {
            case Item.Weapon.None:
            case Item.Weapon.Axe:
            case Item.Weapon.Shovel:
            case Item.Weapon.Sword:
            case Item.Weapon.Whip:
                SwingSword();
                break;
        }
    }

    public void UseTool(Item.Tool toolType) {
        PlantableZone currentPlantableZone = player.TryGetAvailablePlantableZone();
        switch (toolType) {

            case Item.Tool.Axe:
                if (currentPlantableZone) {
                    if (currentPlantableZone.IsPlanted()) {
                        currentPlantableZone.Chop();
                    }
                }
                else {
                    // Swiping at empty space with axe
                }
                break;

            case Item.Tool.Shovel:
                if (currentPlantableZone) {
                    // can only dig up empty dirt patch
                    if (!currentPlantableZone.IsPlanted()) {
                        // Destroy dirt and try to add one to our inventory.
                        Destroy((currentPlantableZone as MonoBehaviour).gameObject);
                        InventorySystem.GetInstance().TryPickupItem(Item.Resource.Dirt);
                    }
                }
                else {
                    // Swiping at empty space with shovel
                }
                break;


                // handle other tools here
        }
    }

    public void UseSpecial(Item.Weapon weaponType) {
        switch (weaponType) {
            case Item.Weapon.None:
            case Item.Weapon.Axe:
            case Item.Weapon.Shovel:
            case Item.Weapon.Sword:
            case Item.Weapon.Whip:

                if (specialAttackTimer >= SPECIAL_ATTACK_COOLDOWN && AttemptToMakeRope()) {
                    specialAttackTimer = 0f;
                    player.GrabRope();
                }
                break;
        }
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

            // If this is on the enemy layer, give us a little knockback when we hit it.
            // TODO: Figure out how to do "pogo" knockback for swinging downwards later
            if ((1 << other.gameObject.layer == AI.EnemyLayermask) && !hitHistory.HitSomethingAlready()) {
                // -1 or 1 exclusively in x, 0 on y, depending on position relative to enemy
                Vector2 hKnockback = (player.transform.position - other.transform.position).x < 0f ? Vector2.left : Vector2.right;
                player.GetPushed(hKnockback * 5f, 0.1f);
            }

            IStrikeable strikeable = other.GetComponent<IStrikeable>();
            if (strikeable != null && !hitHistory.HaveHit(other.gameObject)) {
                strikeable.Strike(this.transform.position, this.weapon);
                hitHistory.MarkObjectAsHit(other.gameObject);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other) {
        HandleWeaponHit(other);
    }





    private class HitObjectHistory {

        private Dictionary<int, GameObject> lookupTable = new Dictionary<int, GameObject>();
        private bool hitThisSwing = false;

        public void MarkObjectAsHit(GameObject obj) {
            lookupTable.Add(obj.GetHashCode(), obj);
            hitThisSwing = true;
        }

        public bool HitSomethingAlready() {
            return hitThisSwing;
        }

        public bool HaveHit(GameObject obj) {
            return lookupTable.ContainsKey(obj.GetHashCode());
        }

        public void Clear() {
            lookupTable.Clear();
            hitThisSwing = false;
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