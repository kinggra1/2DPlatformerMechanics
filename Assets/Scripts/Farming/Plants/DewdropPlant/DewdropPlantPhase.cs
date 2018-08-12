using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DewdropPlantPhase : MonoBehaviour, IGrowablePhase {

    public GameObject[] leavesToGrow;

    private float growthTime = 5f;

    // float 0 to 1 for linear interpolation over growth
    private float growthProgress = 0f;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AnimatePhaseGrowth() {
        StartCoroutine(GrowthCoroutine());
    }

    // Grow the set of leaves specified in the public array
    IEnumerator GrowthCoroutine() {

        foreach (GameObject leaf in leavesToGrow) {
            leaf.transform.localScale = Vector3.zero;
        }

        while (growthProgress < 1f) {
            growthProgress += Time.deltaTime / growthTime;

            foreach (GameObject leaf in leavesToGrow) {
                leaf.transform.localScale = Vector3.one * growthProgress;
            }
            yield return null;
        }
    }
}
