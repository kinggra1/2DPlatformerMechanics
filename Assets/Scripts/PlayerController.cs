﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    
    public float hMoveSpeed = 10f;
    public float maxFallspeed = -15f;
    public float jumpForce = 600f;
    public TextMesh debugStateText;
    public TextMesh debugDirectionText;

    private Vector2 wallJumpDirection = (Vector2.up + Vector2.right).normalized;

    public enum Direction { NONE, UP, DOWN, LEFT, RIGHT };
    private Direction playerFacing = Direction.RIGHT;
    private Direction wallDirection = Direction.NONE;

    private enum MotionState { IDLE, RUN, JUMP, FALL, WALLSLIDE };
    private MotionState motionState = MotionState.IDLE;
    private MotionState prevMotionState = MotionState.IDLE;

    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private int ignoredLayers;

    private float downRaycastDist = 0.6f;
    private float sideRaycastDist = 0.6f;

    // Updated in updateRaycasts
    private bool onGround;
    private GameObject objectBelow;
    private bool onWall;
    private GameObject objectOnLeft;
    private GameObject objectOnRight;

    private float xInput;
    private float yInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private float xVel;
    private float yVel;

    private GameController gameController;

	// Use this for initialization
	void Awake () {
        rb = this.GetComponent<Rigidbody2D>();
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        ignoredLayers = ~(1 << LayerMask.NameToLayer("Player"));

        gameController = GameController.GetInstance();
	}

    public Direction PlayerFacing() {
        return playerFacing;
    }
	
	// Update is called once per frame
	void Update () {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        jumpPressed = Input.GetButtonDown("Jump");
        jumpReleased = Input.GetButtonUp("Jump");

        updateRaycasts();
        checkWalls();

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

                UpdatePlayerDirectionFromVelocity();

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

                ApplyAirSpeedModifier();
                UpdatePlayerDirectionFromVelocity();

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
                UpdatePlayerDirectionFromVelocity();

                // More gravity when falling for a "faster" fall
                yVel += Physics.gravity.y * 2f * Time.deltaTime;
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
                    case Direction.LEFT:
                        SetMotionState(MotionState.WALLSLIDE);
                        break;
                    case Direction.RIGHT:
                        SetMotionState(MotionState.WALLSLIDE);
                        break;
                    case Direction.NONE:
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
                    jump.x = wallDirection == Direction.RIGHT ? -jump.x : jump.x; 
                    rb.AddForce(jump);
                    SetMotionState(MotionState.JUMP);
                    break;
                }

                break;
        }

        if (debugDirectionText) {
            debugDirectionText.text = playerFacing.ToString();
        }

        // adjust the velocity on our rigidbody.
        rb.velocity = new Vector2(xVel, yVel);

	}

    private void SetMotionState(MotionState newMotionState) {
        if (newMotionState == motionState) {
            // this should never happen, but...
            return;
        }

        prevMotionState = motionState;
        motionState = newMotionState;

        if (debugStateText) {
            debugStateText.text = motionState.ToString();
        }
    }

    private void UpdatePlayerDirectionFromVelocity() {
        if (Mathf.Approximately(xVel, 0f)) {
            return;
        }

        if (xVel < 0f) {
            playerFacing = Direction.LEFT;
        }
        else {
            playerFacing = Direction.RIGHT;
        }
    }

    private void ApplyAirSpeedModifier() {
        if (Mathf.Abs(xInput) >= 0.01f) {
            xVel = Mathf.Lerp(rb.velocity.x, xVel, Time.deltaTime*10);
        }
        else {
            xVel = Mathf.Lerp(rb.velocity.x, 0, Time.deltaTime*10);
        }
    }

    private void updateRaycasts() {

        objectBelow = null;
        objectOnRight = null;
        objectOnLeft = null;
        onGround = false;
        onWall = false;
        Vector2 playerCenter = getPlayerCenter();
        Vector2 playerLeftCenter = playerCenter - Vector2.right * 0.5f;
        Vector2 playerRightCenter = playerCenter + Vector2.right * 0.5f;

        // center grounded check
        RaycastHit2D hit2D = Physics2D.Linecast(playerCenter, playerCenter - Vector2.up * downRaycastDist, ignoredLayers);
        Debug.DrawLine(playerCenter, playerCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            objectBelow = hit2D.collider.gameObject;
            onGround = true;
        }

        // left grounded corner check
        hit2D = Physics2D.Linecast(playerLeftCenter, playerLeftCenter - Vector2.up * downRaycastDist, ignoredLayers);
        Debug.DrawLine(playerLeftCenter, playerLeftCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            //objectBelow = hit2D.collider.gameObject;
            onGround = true;
        }

        // right grounded corner check
        hit2D = Physics2D.Linecast(playerRightCenter, playerRightCenter - Vector2.up * downRaycastDist, ignoredLayers);
        Debug.DrawLine(playerRightCenter, playerRightCenter - Vector2.up * downRaycastDist, Color.red);
        if (hit2D) {
            //objectBelow = hit2D.collider.gameObject;
            onGround = true;
        }

        hit2D = Physics2D.Linecast(playerCenter, playerCenter + Vector2.right * sideRaycastDist, ignoredLayers);
        Debug.DrawLine(playerCenter, playerCenter + Vector2.right * sideRaycastDist, Color.red);
        if (hit2D) {
            objectOnRight = hit2D.collider.gameObject;
            onWall = true;
        }

        hit2D = Physics2D.Linecast(playerCenter, playerCenter - Vector2.right * sideRaycastDist, ignoredLayers);
        Debug.DrawLine(playerCenter, playerCenter - Vector2.right * sideRaycastDist, Color.red);
        if (hit2D)
        {
            objectOnLeft = hit2D.collider.gameObject;
            onWall = true;
        }
    }
     
    private void checkWalls() {
        if (objectOnLeft && objectOnRight) {
            // Left object is closer
            if (Vector3.Distance(objectOnLeft.transform.position, transform.position) <
                Vector3.Distance(objectOnRight.transform.position, transform.position)) {
                wallDirection = Direction.LEFT;
            } else {
                wallDirection = Direction.RIGHT;
            }
        } else if (objectOnLeft) {
            wallDirection = Direction.LEFT;
        } else if (objectOnRight) {
            wallDirection = Direction.RIGHT;
        } else {
            wallDirection = Direction.NONE;
        }
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
}
