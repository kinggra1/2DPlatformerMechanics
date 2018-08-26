using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHand : MonoBehaviour {

    [Tooltip("The object that is the position/scale parent of the weapon itself. May move around with weapon usage.")]
    public GameObject weaponParent;

    private float animationTimer = 0f;
    private float animationDuration = 0.2f;

    private GameObject weaponObject = null;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // TODO: Make this more memory efficient. Save references to weapons and activate/deactivate them
    public void SetWeaponObject(GameObject obj) {
        if (weaponObject) {
            Destroy(weaponObject);
        }
        weaponObject = Instantiate(obj, transform) as GameObject;
        weaponObject.transform.localPosition = Vector3.zero;
    }

    // TODO: Take in a time to specify speed
    public void SwingSword() {
        StartCoroutine(SwordAnimationCoroutine());
    }







    // Animation Coroutines
    private IEnumerator SwordAnimationCoroutine() {
        animationTimer = 0f;

        float startAngle = -30f;
        float endAngle = 90f;
        this.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle);

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
    }
}
