using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Kind of a silly organizer script. This makes is a tradeoff to make some of the stuff with handling
 * weapon/player collisions/triggers seperately easier
 */
public class PlayerOrganizer : MonoBehaviour {

    // Entry here for every seperate component of the player under this parent
    public GameObject playerBody;
    public GameObject weaponParent;

    private Vector3 startingBodyScale;
    private Vector3 startingWeaponScale;

    private void Start() {
        startingBodyScale = playerBody.transform.localScale;
        startingWeaponScale = weaponParent.transform.localScale;
    }

    // Update is called once per frame
    void FixedUpdate () {
        // weaponParent.transform.position = playerBody.transform.position;
        this.transform.position = playerBody.transform.position;
        playerBody.transform.localPosition = Vector3.zero;
	}

    public void SetPlayerScale(Vector3 scale) {
        playerBody.transform.localScale = scale;
        weaponParent.transform.localScale = scale;
    }

    internal void SetFacing(AI.Direction playerFacing) {
        switch (playerFacing) {
            case AI.Direction.NONE:
                break;
            case AI.Direction.UP:
                break;
            case AI.Direction.DOWN:
                break;
            case AI.Direction.LEFT:
                playerBody.transform.localScale = startingBodyScale;
                weaponParent.transform.localScale = startingWeaponScale;
                break;
            case AI.Direction.RIGHT:
                // flip our x axis scale to flip the sprite
                playerBody.transform.localScale = new Vector3(-startingBodyScale.x, startingBodyScale.y, startingBodyScale.z);
                weaponParent.transform.localScale = new Vector3(-startingWeaponScale.x, startingWeaponScale.y, startingWeaponScale.z);
                break;
        }
    }
}
