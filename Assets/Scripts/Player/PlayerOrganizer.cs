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
	
	// Update is called once per frame
	void Update () {
        // weaponParent.transform.position = playerBody.transform.position;
        this.transform.position = playerBody.transform.position;
        playerBody.transform.localPosition = Vector3.zero;
	}

    public void SetPlayerScale(Vector3 scale) {
        playerBody.transform.localScale = scale;
        weaponParent.transform.localScale = scale;
    }
}
