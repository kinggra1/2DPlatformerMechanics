using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitPlantOrangePhase : MonoBehaviour, IGrowablePhase {
    [Tooltip("The plant parts of this phase that were not in the previous phase")]
    public GameObject[] newPieces;

    private float growthTime = 5f;

    // float 0 to 1 for linear interpolation over growth
    private float growthProgress = 0f;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void AnimatePhaseGrowth() {
        StartCoroutine(GrowthCoroutine());
    }

    private void ResetGrowth() {
        growthProgress = 0f;
        foreach (GameObject newPiece in newPieces) {
            newPiece.transform.localScale = Vector3.zero;
        }
    }

    // Grow the set of leaves specified in the public array
    IEnumerator GrowthCoroutine() {

        ResetGrowth();

        while (growthProgress < 1f) {
            growthProgress += Time.deltaTime / growthTime;

            foreach (GameObject newPiece in newPieces) {
                newPiece.transform.localScale = Vector3.one * growthProgress;
            }
            yield return null;
        }
    }
}
