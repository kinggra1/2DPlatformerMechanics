using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

/*
 * PlayerController deals primarily with the movement of the player itself. So anything that controls walk/run/jump/climb/swim/etc.
 * It is also used as the central source of delegation for player-related actions. The bottom of this file is a collection of
 * wrapper functions that allow other systems (e.g. InventorySystem) to tell the player to manage an action (e.g. UseWeapon),
 * which will be passed down to PlayerWeaponController. All Controllers with the prefix Player- should only be visible to the PlayerController.
 */
public partial class PlayerController : MonoBehaviour {

    // Controllers to delegate work to (just to keep this file reasonably sized). 
    private PlayerWeaponController weaponController;

    // Publicly settable/customizable fields in UnityInspector.
    [Tooltip("Max horizontal movement speed in m/s")]
    public float hMoveSpeed = 10f;
    [Tooltip("Max fall speed in m/s")]
    public float maxFallspeed = -15f;
    [Tooltip("Upwards force applied to Rigidbody2D to create a jump in N")]
    public float jumpForce = 1000;
    [Tooltip("Movement speed when dash/roll/dodge-ing.")]
    public float dodgeMoveSpeed = 30f;
    [Tooltip("Length of time that the dash/roll/dodge-ing motion takes place. Will also set invulnerability timer.")]
    public float dodgeStateTime = 0.2f;
    [Tooltip("How much time must pass between successive dodges.")]
    public float dodgeCooldown = 2f;
    [Tooltip("A reference to the visual sprite of the actual player")]
    public GameObject playerBody;
    [Tooltip("The amount of time player cannot be hit after sustaining damage.")]
    public float knockbackInvulnerabilityTime = 2f;
    [Tooltip("The amount of time that input has no effect on the player after they've been hit.")]
    public float getHitStunTime = 0.2f;
    [Tooltip("The parent of all player-associated objects. Used to seperate collisions and scale flipping.")]
    public PlayerOrganizer playerOrganizer;

    [Tooltip("Debugging text to see our current movement state.")]
    public TextMesh debugStateText;
    [Tooltip("Debugging text to see our current facing direction (in code).")]
    public TextMesh debugDirectionText;

    // Horizontal deceleration rate when there is no input in the air. m/s^2
    private readonly float AIR_X_DECEL_RATE = 50f;

    // Horizontal deceleration rate when there is no input on a rope. m/s^2
    private readonly float SWING_X_DECEL_RATE = 10f;

    private Vector2 wallJumpDirection = (Vector2.up + Vector2.right).normalized;

    // Cardinal directions in 2D referenced as "Up, Down, Left, Right" and a null state.
    private AI.Direction playerFacing = AI.Direction.RIGHT;
    private AI.Direction wallDirection = AI.Direction.NONE;
    private AI.Direction dodgeDirection = AI.Direction.RIGHT;

    private enum MotionState { IDLE, RUN, DODGE, JUMP, FALL, SWIM, SWING, WALLSLIDE };
    private MotionState motionState = MotionState.IDLE;
    private MotionState prevMotionState = MotionState.IDLE;

    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private int standableRaycastLayers;
    private int wallRaycastLayers;

    private float downRaycastDist = 1.1f;
    private float sideRaycastDist = 0.6f;

    private float invulnTimer = 0f;
    private float stunTimer = 0f;
    private float dodgeCooldownTimer = 0f;

    private int playerLayer;
    private int platformLayer; // by definition, a platform is something we can jump up through. call other things ground.
    private int groundLayer;

    // Updated in updateRaycasts
    private bool onGround;
    private bool inWater;
    private bool platformBelow; // used for determining if we're going to go through platforms or not
    private GameObject objectBelow;
    private bool onWall;
    private GameObject objectOnLeft;
    private GameObject objectOnRight;

    private InventorySystem inventory;
    private LevelBoundaryManager levelBoundaryManager;
    private WaterSpriteController waterSprite;

    private float xInput;
    private float yInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool ePressed;
    private float dodgeInput;

    private float xVel;
    private float yVel;

    private CameraFollow cameraFollowScript;
    private GameController gameController;
    public float currentStateTimer = 0f;

    private static PlayerController instance;

    // Use this for initialization
    void Awake() {
        if (instance == null) {
            // Keep this object around between scenes.
            DontDestroyOnLoad(this.transform.parent.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(this.transform.parent.gameObject);
        }

        // This keeps our motion consistent if we ever change the physics timescale.
        jumpForce /= (Time.fixedDeltaTime * 60f);

        rb = this.GetComponent<Rigidbody2D>();
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        standableRaycastLayers = (
            (1 << LayerMask.NameToLayer("Ground"))
            | (1 << LayerMask.NameToLayer("Platform"))
            // | (1 << LayerMask.NameToLayer("NameOfALayerWeCanStandOn")
            // ...
            );

        wallRaycastLayers = (
            (1 << LayerMask.NameToLayer("Wall"))
            // | (1 << LayerMask.NameToLayer("NameOfALayerWeCanWallSlideOn?")
            // ...
            );

        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
        groundLayer = LayerMask.NameToLayer("Ground");

    }

    // Put any singleton instance accessing here
    // This allows any existing singletons in the scene to set themselves up in their own Awake()
    // without us accidentally creating a new one before they have a chance to initalize 
    // Start() calls runs after Awake() calls
    // ( yes this happened and that's why I'm writing this -__- )
    private void Start() {
        FindNestedControllers();
        FindExternalControllers();

        playerOrganizer.SetFacing(playerFacing);
        cameraFollowScript.SetPlayerTurnaroundX(transform.position.x);
    }

    private void FindNestedControllers() {
        weaponController = PlayerWeaponController.GetInstance();
    }

    private void FindExternalControllers() {
        gameController = GameController.GetInstance();
        inventory = InventorySystem.GetInstance();
        cameraFollowScript = Camera.main.GetComponent<CameraFollow>();
        waterSprite = WaterSpriteController.GetInstance();
    }

    public static PlayerController GetInstance() {
        if (instance == null) {
            Debug.LogError("PlayerController not found in scene.");
        }
        return instance;
    }

    public WaterSpriteController GetWaterSprite() {
        return waterSprite;
    }

    public Rigidbody2D GetRigidbody() {
        return rb;
    }

    public AI.Direction PlayerFacing() {
        return playerFacing;
    }

    public void SetInvulnerabilityTime(float time) {

        // Don't decrease invulnerability timer if we are already invulnerable for more time than this.
        if (invulnTimer >= time) {
            return;
        }
        invulnTimer = time;
    }

    public void SetStunTime(float time) {

        // Don't decrease stun timer if we are already invulnerable for more time than this.
        if (stunTimer >= time) {
            return;
        }
        stunTimer = time;
    }

    public void GetHit(Vector2 knockback) {
        GetPushed(knockback, getHitStunTime);
        SetInvulnerabilityTime(knockbackInvulnerabilityTime);
        cameraFollowScript.AddShakeTrauma(0.5f);
    }

    public void GrabRope() {
        SetMotionState(MotionState.SWING);
    }

    public void GetPushed(Vector2 knockback, float stunTime) {
        rb.velocity += knockback;
        SetStunTime(stunTime);
    }

    public bool IsInvulnerable() {
        return invulnTimer > 0f;
    }

    public bool CanDodge() {
        return dodgeCooldownTimer <= 0f;
    }

    // This will likely get more complex if we do a more intricate check
    public bool OnPlantableGround() {
        return onGround;
    }

    // Reset input that is held between Update and FixedUpdate.
    private void ResetInput() {
        jumpPressed = false;
    }

    void Update() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        // boolean guard makes sure FixedUpdate has time to process the input.
        if (!jumpPressed) {
            jumpPressed = Input.GetButtonDown("Jump");
        }
        jumpHeld = Input.GetButton("Jump");

        ePressed = Input.GetKeyDown(KeyCode.E);
        dodgeInput = Input.GetAxisRaw("Dodge");

        CheckSurroundings();
        FindClosestWall();
    }

    // We run in FixedUpdate because we are directly messing with the RigidBody2D
    void FixedUpdate() {

        currentStateTimer += Time.fixedDeltaTime;

        if (invulnTimer > 0f) {
            invulnTimer -= Time.fixedDeltaTime;
        }
        if (dodgeCooldownTimer > 0f) {
            dodgeCooldownTimer -= Time.fixedDeltaTime;
        }

        xVel = xInput * hMoveSpeed;
        yVel = rb.velocity.y;

        // Okay, this might be a bad idea, but-
        // This is where we handle various kinds of controller/environment
        // input that was collected above depending on what "motion state" we're
        // in. 
        // 
        // The tradeoff here is either we keep the player in a state machine and
        // then have a lot of redundant transition checks e.g. "if jump pressed
        // then jump" inside of most MotionState cases or we have a lot of 
        // "if jump pressed then check if we're in any of the states where it's 
        // okay to jump and THEN jump". I like it this way just from a visual 
        // organization sense, and also it gives the added benefit of implementing
        // custom behavior for each each state transition. e.g. "inside of the
        // IDLE case we detect jump and do foo, inside of the WALLSLIDE case we
        // detect jump and do bar".
        switch (motionState) {

            // We hangin' out.
            case MotionState.IDLE:

                // DODGE
                if (dodgeInput != 0f && CanDodge()) {
                    SetMotionState(MotionState.DODGE);
                    break;
                }
                // JUMP
                if (jumpHeld && onGround) {
                    rb.AddForce(Vector2.up * jumpForce);
                    SetMotionState(MotionState.JUMP);
                    break;
                }
                // RUNNING
                if (Mathf.Abs(xVel) > 0.01f) {
                    // loloops rb.AddForce(Vector2.up * jumpForce);
                    SetMotionState(MotionState.RUN);
                    break;
                }
                // NOT ON THE GROUND ANYMORE
                if (!onGround) {
                    SetMotionState(MotionState.FALL);
                    break;
                }
                break;

            // Exercising 
            case MotionState.RUN:

                // DODGE
                if (dodgeInput != 0f && CanDodge()) {
                    SetMotionState(MotionState.DODGE);
                    break;
                }
                if (jumpHeld && onGround) {
                    rb.AddForce(Vector2.up * jumpForce);
                    SetMotionState(MotionState.JUMP);
                    break;
                }
                if (Mathf.Approximately(xVel, 0f)) {
                    SetMotionState(MotionState.IDLE);
                    break;
                }
                // NOT ON THE GROUND ANYMORE
                if (!onGround) {
                    SetMotionState(MotionState.FALL);
                    break;
                }
                break;

            // We're dodging attacks
            case MotionState.DODGE:

                xVel = dodgeMoveSpeed * AI.DirectionScalarX(dodgeDirection);
                yVel = 0f;

                // The only way to leave this state is to let the dodge complete
                if (currentStateTimer > dodgeStateTime) {
                    SetMotionState(MotionState.IDLE);
                    dodgeCooldownTimer = dodgeCooldown;
                }
                break;

            // We're going upwards
            case MotionState.JUMP:

                // Player feels gravity more when jumping to feel less floaty
                yVel += Physics.gravity.y * 2f * Time.fixedDeltaTime;

                ApplyAirSpeedModifier();

                // DODGE
                if (dodgeInput != 0f && CanDodge()) {
                    SetMotionState(MotionState.DODGE);
                    break;
                }
                // if the jump key is released, we should start falling
                if (!jumpHeld) {
                    yVel = 0f;
                    SetMotionState(MotionState.FALL);
                    break;
                }

                // we are no longer uh... falling up? Falling down now
                if (yVel <= 0) {
                    SetMotionState(MotionState.FALL);
                    break;
                }
                break;

            // Shit, we're heading towards the ground at a non-zero velocity
            case MotionState.FALL:
                //xVel = rb.velocity.x;
                //break;
                ApplyAirSpeedModifier();

                // More gravity when falling for a "faster" fall
                yVel += Physics.gravity.y * 3f * Time.fixedDeltaTime;
                if (yVel < maxFallspeed) {
                    yVel = maxFallspeed;
                }

                // DODGE
                if (dodgeInput != 0f && CanDodge()) {
                    SetMotionState(MotionState.DODGE);
                    break;
                }
                if (inWater) {
                    SetMotionState(MotionState.SWIM);
                }

                // If there is ground that we're basically touching, then we're 
                // no longer falling
                if (onGround) {
                    SetMotionState(MotionState.IDLE);
                    break;
                }

                // If we started contacting a wall, let's start wallsliding
                switch (wallDirection) {
                    case AI.Direction.LEFT:
                        SetMotionState(MotionState.WALLSLIDE);
                        break;
                    case AI.Direction.RIGHT:
                        SetMotionState(MotionState.WALLSLIDE);
                        break;
                    case AI.Direction.NONE:
                        break;
                }

                break;

            // This cat is okay with getting wet
            case MotionState.SWIM:

                // JUMP
                if (jumpHeld) {
                    rb.AddForce(Vector2.up * jumpForce);
                    SetMotionState(MotionState.JUMP);
                    break;
                }

                // Somehow managed to not be in the water without jumping. Possibly knockback.
                if (!inWater) {
                    SetMotionState(MotionState.IDLE);
                    break;
                }

                break;

            // Swingin' from a rope
            case MotionState.SWING:
                SwingControlModifier();

                // JUMP 
                // jumpPressed is used here, unlike other places, so holding jump doesn't cut off rope. 
                // Need to re-press.
                if (jumpPressed) {
                    rb.AddForce(Vector2.up * jumpForce);
                    SetMotionState(MotionState.JUMP);
                    break;
                }

                if (onGround) {
                    SetMotionState(MotionState.IDLE);
                    break;
                }

                if (inWater) {
                    SetMotionState(MotionState.SWIM);
                }

                break;

            // We. Look. So. Cool.
            case MotionState.WALLSLIDE:
                // My current defnition of "wallslide" involves constantly moving
                // downwards slowly. If we're on a wall going UP, then I think that's 
                // probably still okay to be considered JUMP

                // Constant sliding speed
                yVel = -0.5f;

                // only way to get off is jumping or bottoming out
                xVel = 0f;

                if (onGround) {
                    SetMotionState(MotionState.IDLE);
                    break;
                }

                if (!onWall) {
                    SetMotionState(MotionState.FALL);
                    break;
                }

                // We can jump off of the wall, but in a slightly different arc
                if (jumpHeld) {
                    Vector2 jump = wallJumpDirection * jumpForce;
                    // flip x if wall to the right
                    jump.x = wallDirection == AI.Direction.RIGHT ? -jump.x : jump.x;
                    rb.AddForce(jump);
                    SetMotionState(MotionState.JUMP);
                    break;
                }

                break;
        }

        if (debugDirectionText) {
            debugDirectionText.text = playerFacing.ToString();
        }

        // Not going to feed any input to Rigidbody2D if we're stunned. Physics will still move player around.
        if (stunTimer > 0f) {
            stunTimer -= Time.fixedDeltaTime;
            return;
        }

        // If we're not stunned, we can control which direction we're facing
        UpdatePlayerDirectionFromInput();

        // adjust the velocity on our rigidbody.
        rb.velocity = new Vector2(xVel, yVel);

        ResetInput();

    } // END OF UPDATE FUNCTION

    private void SetMotionState(MotionState newMotionState) {
        if (newMotionState == motionState) {
            // this should never happen, but...
            return;
        }

        prevMotionState = motionState;
        motionState = newMotionState;
        currentStateTimer = 0f;

        switch (prevMotionState) {
            case MotionState.SWING:
                if (weaponController.HasRope()) {
                    weaponController.BreakRope();
                }
                break;
        }


        switch (motionState) {

            case MotionState.DODGE:
                dodgeDirection = AI.FloatToHorizontalDirection(dodgeInput);
                // Make us invulnerable for the duration of the dodge.
                SetInvulnerabilityTime(dodgeStateTime);
                break;

            case MotionState.FALL:
                // Set an angle in our motion curve.
                // We move up slowly but as soon as our speed hits zero, shoot us downwards.
                // Increase to decrease feeling of suddenly falling at top of jump.
                yVel = -1f;
                break;
        }

        prevMotionState = motionState;
        motionState = newMotionState;

        if (debugStateText) {
            debugStateText.text = motionState.ToString();
        }
    }

    public PlantableZone TryGetAvailablePlantableZone() {
        PlantableZone closestZone = null;
        float closestZoneDist = float.MaxValue;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        Collider2D[] results = new Collider2D[100];

        int hits = rb.OverlapCollider(filter, results);
        for (int i = 0; i < hits; i++) {
            PlantableZone zone = results[i].GetComponent<PlantableZone>();
            if (zone) {
                float dist = Vector2.Distance(transform.position, zone.transform.position);
                if (dist < closestZoneDist) {
                    closestZone = zone;
                    closestZoneDist = dist;
                }
            }
        }
        return closestZone;
    }

    private void UpdatePlayerDirectionFromInput() {
        if (Mathf.Approximately(xInput, 0f)) {
            return;
        }

        if (xInput < 0f && playerFacing == AI.Direction.RIGHT ||
            xInput > 0f && playerFacing == AI.Direction.LEFT) {

            playerFacing = AI.OppositeDirection(playerFacing);
            cameraFollowScript.SetPlayerTurnaroundX(transform.position.x);
            playerOrganizer.SetFacing(playerFacing);
        }
    }

    /*
    * Decelerate (and possibly limit) the player's X velocity when they are airbourne
    * NOTE: This function should only be called from FixedUpdate because it uses fixedDeltaTime
    */
    private void ApplyAirSpeedModifier() {
        if (Mathf.Abs(xInput) >= 0.01f) {
            // xVel = Mathf.MoveTowards(rb.velocity.x, xVel, Time.fixedDeltaTime * AIR_X_DECEL_RATE);
        }
        else {
            xVel = Mathf.MoveTowards(rb.velocity.x, 0, Time.fixedDeltaTime * AIR_X_DECEL_RATE);
        }
    }

    private void SwingControlModifier() {
        // If there is input, apply it as a force to the player rigidbody2D.
        if (Mathf.Abs(xInput) >= 0.01f) {
            xVel = rb.velocity.x;
            rb.AddForce(Vector2.right * xInput * 10f);
        }
        else if (Mathf.Abs(rb.velocity.x) > 1f) {
            // decay our velocity if there is no input and we're moving at least a certain speed.
            xVel = Mathf.MoveTowards(rb.velocity.x, 0, Time.fixedDeltaTime * SWING_X_DECEL_RATE);
        }
        else {
            // Let physics move us if we're going less than 1 m/s with no input.
            xVel = rb.velocity.x;
        }
    }

    private void CheckSurroundings() {
        objectBelow = null;
        objectOnRight = null;
        objectOnLeft = null;
        onGround = false;
        inWater = false;
        platformBelow = false;
        onWall = false;
        Vector2 playerCenter = getPlayerCenter();
        Vector2 playerLeftCenter = playerCenter - Vector2.right * 0.5f;
        Vector2 playerRightCenter = playerCenter + Vector2.right * 0.5f;
        float minPlatformDistance = 0.5f;

        // check if we are in the water
        RaycastHit2D hit2D = Physics2D.Linecast(playerCenter, playerCenter + Vector2.down * downRaycastDist * 0.5f, AI.WaterLayermask);
        Debug.DrawLine(playerCenter, playerCenter - Vector2.up * downRaycastDist * 0.5f, Color.blue);
        if (hit2D) {
            inWater = true;
        }

        // center grounded check
        hit2D = Physics2D.Linecast(playerCenter, playerCenter + Vector2.down * downRaycastDist, standableRaycastLayers);
        Debug.DrawLine(playerCenter, playerCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            objectBelow = hit2D.collider.gameObject;
            onGround = true;
            if (hit2D.collider.gameObject.layer.Equals(platformLayer) && hit2D.distance > minPlatformDistance) {
                platformBelow = true;
            }
        }

        // left grounded corner check
        hit2D = Physics2D.Linecast(playerLeftCenter, playerLeftCenter + Vector2.down * downRaycastDist, standableRaycastLayers);
        Debug.DrawLine(playerLeftCenter, playerLeftCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            //objectBelow = hit2D.collider.gameObject;
            onGround = true;
            if (hit2D.collider.gameObject.layer.Equals(platformLayer) && hit2D.distance > minPlatformDistance) {
                platformBelow = true;
            }
        }

        // right grounded corner check
        hit2D = Physics2D.Linecast(playerRightCenter, playerRightCenter + Vector2.down * downRaycastDist, standableRaycastLayers);
        Debug.DrawLine(playerRightCenter, playerRightCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            //objectBelow = hit2D.collider.gameObject;
            onGround = true;
            if (hit2D.collider.gameObject.layer.Equals(platformLayer) && hit2D.distance > minPlatformDistance) {
                platformBelow = true;
            }
        }

        hit2D = Physics2D.Linecast(playerCenter, playerCenter + Vector2.right * sideRaycastDist, wallRaycastLayers);
        Debug.DrawLine(playerCenter, playerCenter + Vector2.right * sideRaycastDist, Color.red);
        if (hit2D) {
            objectOnRight = hit2D.collider.gameObject;
            onWall = true;
        }

        hit2D = Physics2D.Linecast(playerCenter, playerCenter + Vector2.left * sideRaycastDist, wallRaycastLayers);
        Debug.DrawLine(playerCenter, playerCenter - Vector2.right * sideRaycastDist, Color.red);
        if (hit2D) {
            objectOnLeft = hit2D.collider.gameObject;
            onWall = true;
        }
    }

    private void FindClosestWall() {
        if (objectOnLeft && objectOnRight) {
            // Left object is closer
            if (Vector2.Distance(objectOnLeft.transform.position, transform.position) <
                Vector2.Distance(objectOnRight.transform.position, transform.position)) {
                wallDirection = AI.Direction.LEFT;
            }
            else {
                wallDirection = AI.Direction.RIGHT;
            }
        }
        else if (objectOnLeft) {
            wallDirection = AI.Direction.LEFT;
        }
        else if (objectOnRight) {
            wallDirection = AI.Direction.RIGHT;
        }
        else {
            wallDirection = AI.Direction.NONE;
        }
    }

    // returns null if we're not on anything
    public GameObject GetObjectBelow() {
        return objectBelow;
    }

    private bool canJump() {
        return IsGrounded();
    }

    public bool IsGrounded() {
        return objectBelow != null;
    }

    private Vector2 getPlayerCenter() {
        return transform.position;
    }

    // Moves all components player, the watersprite, and the camera to a location for a smooth teleport experience
    public void TeleportAfterSceneLoad(Vector3 newPlayerLocation) {
        // Reset object references
        FindNestedControllers();
        FindExternalControllers();
        cameraFollowScript.SetPlayerTurnaroundX(transform.position.x);

        playerOrganizer.TeleportAfterSceneLoad(newPlayerLocation);
        cameraFollowScript.TeleportAfterSceneLoad(newPlayerLocation);
        waterSprite.TeleportTo(newPlayerLocation);
    }

    private void OnTriggerEnter2D(Collider2D collider) {
        Collectible collectible = collider.GetComponent<Collectible>();
        if (collectible && collectible.CanCollect()) {
            waterSprite.AddToTargetList(collider.gameObject);
        }

        // If we just landed in some water, then tell the WaterSprite to refill if it isn't already full
        if ((1 << collider.gameObject.layer) == AI.WaterLayermask && !inventory.WaterLevelFull()) {
            waterSprite.AddImmediateToTargetList(collider.gameObject);
        }
    }











    // Wrapper functions to pass behavior down to sub-controllers.
    // Weapon Controller functions.
    public void SetWeapon(ItemWeapon weaponItem) {
        weaponController.SetWeapon(weaponItem);
    }

    public void UseWeapon(Item.Weapon weaponType) {
        weaponController.UseWeapon(weaponType);
    }

    public void UseTool(Item.Tool toolType) {
        weaponController.UseTool(toolType);
    }

    public void UseSpecial(Item.Weapon weaponType) {
        switch (weaponType) {
            // Have character do whatever animation is associated with their sprite
            // Add animation here
            case Item.Weapon.None:
                break;
            case Item.Weapon.Axe:
                break;
            case Item.Weapon.Shovel:
                break;
            case Item.Weapon.Sword:
                break;
            case Item.Weapon.Whip:
                break;
        }

        // And also tell the WeaponController animate
        weaponController.UseSpecial(weaponType);
    }












    // Saving and Loading functions for maintaining stateful information about player.
    public PlayerData Save() {
        PlayerData data = new PlayerData();
        data.facingLeft = (playerFacing == AI.Direction.LEFT);

        return data;
    }

    public void Load(PlayerData data) {
        playerFacing = data.facingLeft ? (AI.Direction.LEFT) : (AI.Direction.RIGHT);
    }
}






// This is the object that will hold data about the player for actual game saving/loading.
// Things that are persistant across play sessions.
[Serializable]
public class PlayerData {
    public bool facingLeft; // otherwise right
}
