using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    [Tooltip("Max horizontal movement speed in m/s")]
    public float hMoveSpeed = 10f;
    [Tooltip("Max fall speed in m/s")]
    public float maxFallspeed = -15f;
    [Tooltip("Upwards force applied to Rigidbody2D to create a jump in N")]
    public float jumpForce = 1000;
    [Tooltip("A reference to our best friend.")]
    public WaterSpriteController waterSprite;
    [Tooltip("A reference to the visual sprite of the actual player")]
    public GameObject playerBody;
    [Tooltip("A reference to the hand where we physically hold Weapons/Tools.")]
    public WeaponHand weaponHand;
    [Tooltip("The amount of time player cannot be hit after sustaining damage.")]
    public float invulnerabilityTime = 2f;
    [Tooltip("The amount of time that input has no effect on the player after they've been hit.")]
    public float knockbackStunTime = 1f;
    [Tooltip("The parent of all player-associated objects. Used to seperate collisions and scale flipping.")]
    public PlayerOrganizer playerOrganizer;

    [Tooltip("Debugging text to see our current movement state.")]
    public TextMesh debugStateText;
    [Tooltip("Debugging text to see our current facing direction (in code).")]
    public TextMesh debugDirectionText;

    private Vector2 wallJumpDirection = (Vector2.up + Vector2.right).normalized;

    // Cardinal directions in 2D referenced as "Up, Down, Left, Right" and a null state.
    private AI.Direction playerFacing = AI.Direction.RIGHT;
    private AI.Direction wallDirection = AI.Direction.NONE;

    private enum MotionState { IDLE, RUN, JUMP, FALL, WALLSLIDE };
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

    private int playerLayer;
    private int platformLayer; // by definition, a platform is something we can jump up through. call other things ground.
    private int groundLayer;

    // Updated in updateRaycasts
    private bool onGround;
    private bool platformBelow; // used for determining if we're going to go through platforms or not
    private GameObject objectBelow;
    private bool onWall;
    private GameObject objectOnLeft;
    private GameObject objectOnRight;
    private IPlantableZone currentPlantableZone = null; // usually null

    private InventorySystem inventory;

    private float xInput;
    private float yInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool ePressed;

    private float xVel;
    private float yVel;

    private GameController gameController;

	// Use this for initialization
	void Awake () {
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
        gameController = GameController.GetInstance();
        inventory = InventorySystem.GetInstance();
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

    public IPlantableZone GetAvailablePlantableZone() {
        return this.currentPlantableZone;
    }

    public void SetAvailablePlantableZone(IPlantableZone zone) {
        this.currentPlantableZone = zone;
    }

    public void GetHit(Vector2 knockback) {
        rb.velocity = knockback;
        invulnTimer = 0f;
        stunTimer = 0f;
    }

    public bool IsInvulnerable() {
        return invulnTimer < invulnerabilityTime;
    }

    // This will likely get more complex if we do a more intricate check
    public bool OnPlantableGround() {
        return onGround;
    }

    // Update is called once per frame
    void Update () {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        jumpPressed = Input.GetButtonDown("Jump");
        jumpReleased = Input.GetButtonUp("Jump");

        ePressed = Input.GetKeyDown(KeyCode.E);

        CheckSurroundings();
        FindClosestWall();

        if (invulnTimer < invulnerabilityTime) {
            invulnTimer += Time.deltaTime;
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

                // JUMP
                if (jumpPressed && onGround) {
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

                UpdatePlayerDirectionFromInput();

                if(jumpPressed && onGround) {
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

            // We're going upwards
            case MotionState.JUMP:

                // Player feels gravity more when jumping to feel less floaty
                yVel += Physics.gravity.y * 2f * Time.deltaTime;

                ApplyAirSpeedModifier();
                UpdatePlayerDirectionFromInput();

                // if the jump key is released, we should start falling
                if (jumpReleased) {
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

                ApplyAirSpeedModifier();
                UpdatePlayerDirectionFromInput();

                // More gravity when falling for a "faster" fall
                yVel += Physics.gravity.y * 3f * Time.deltaTime;
                if (yVel < maxFallspeed) {
                    yVel = maxFallspeed;
                }

                // If there is ground that we're basically touching, then we're 
                // no longer falling
                if (onGround) {
                    SetMotionState(MotionState.IDLE); // what about if we're moving sideways?
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
                if (jumpPressed) {
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
        if (stunTimer < knockbackStunTime) {
            stunTimer += Time.deltaTime;
            return;
        }

        // adjust the velocity on our rigidbody.
        rb.velocity = new Vector2(xVel, yVel);

	} // END OF UPDATE FUNCTION

    private void SetMotionState(MotionState newMotionState) {
        if (newMotionState == motionState) {
            // this should never happen, but...
            return;
        }

        switch (newMotionState) {
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

    public void SetWeapon(ItemWeapon weaponItem) {
        weaponHand.SetWeapon(weaponItem);
    }

    public void UseWeapon(Item.Weapon weaponType) {

        // Different animations for different weapons
        switch(weaponType) {

            // Set of "swordlike" animations
            case Item.Weapon.Axe:
            case Item.Weapon.Shovel:
            case Item.Weapon.Sword:
                // Have character do whatever animation is associated with their sprite
                // Add animation here

                // And also tell the WeaponController animate
                weaponHand.SwingSword();
                break;
        }
    }


    private void UpdatePlayerDirectionFromInput() {
        if (Mathf.Approximately(xInput, 0f)) {
            return;
        }

        if (xInput < 0f) {
            playerFacing = AI.Direction.LEFT;
        } else {
            playerFacing = AI.Direction.RIGHT;
        }
        playerOrganizer.SetFacing(playerFacing);
    }

    private void ApplyAirSpeedModifier() {
        if (Mathf.Abs(xInput) >= 0.01f) {
            xVel = Mathf.Lerp(rb.velocity.x, xVel, Time.deltaTime*10);
        }
        else {
            xVel = Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime*10);
        }
    }

    private void CheckSurroundings() {

        objectBelow = null;
        objectOnRight = null;
        objectOnLeft = null;
        onGround = false;
        platformBelow = false;
        onWall = false;
        Vector2 playerCenter = getPlayerCenter();
        Vector2 playerLeftCenter = playerCenter - Vector2.right * 0.5f;
        Vector2 playerRightCenter = playerCenter + Vector2.right * 0.5f;
        float minPlatformDistance = 0.5f;

        // center grounded check
        RaycastHit2D hit2D = Physics2D.Linecast(playerCenter, playerCenter  + Vector2.down * downRaycastDist, standableRaycastLayers);
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
        if (hit2D)
        {
            objectOnLeft = hit2D.collider.gameObject;
            onWall = true;
        }
    }
     
    private void FindClosestWall() {
        if (objectOnLeft && objectOnRight) {
            // Left object is closer
            if (Vector3.Distance(objectOnLeft.transform.position, transform.position) <
                Vector3.Distance(objectOnRight.transform.position, transform.position)) {
                wallDirection = AI.Direction.LEFT;
            } else {
                wallDirection = AI.Direction.RIGHT;
            }
        } else if (objectOnLeft) {
            wallDirection = AI.Direction.LEFT;
        } else if (objectOnRight) {
            wallDirection = AI.Direction.RIGHT;
        } else {
            wallDirection = AI.Direction.NONE;
        }
    }

    // returns null if we're not on anything
    public GameObject GetObjectBelow() {
        return objectBelow;
    }

    private bool canJump() {
        return isGrounded();
    }

    private bool isGrounded() {
        return objectBelow != null;
    }

    private Vector2 getPlayerCenter() {
        return transform.position;
    }    

    private void OnTriggerEnter2D(Collider2D collider) {
        Collectible collectible = collider.GetComponent<Collectible>();
        if (collectible && collectible.CanCollect()) {
            waterSprite.AddToTargetList(collider.gameObject);
        }
    }
}
