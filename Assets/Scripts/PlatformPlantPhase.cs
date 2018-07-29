using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPlantPhase : MonoBehaviour, Growable {

    public GameObject stemPrefab;
    public GameObject[] platforms;
    public float maxPlatformXOffset = 4f;

    private float growthTime = 5f;
    private GameObject topPlatform;

    private Vector3 platformGrowthStartPos;
    private Vector3 platformGrowthTargetPos;

    // float 0 to 1 for linear interpolation over growth
    private float growthProgress = 0f;

    // Use this for initialization
    void Awake() {
        if (platforms.Length == 0) {
            return;
        }

        topPlatform = platforms[platforms.Length - 1];
        platformGrowthTargetPos = topPlatform.transform.position;

        // If we have more than 1 platform then let's grow out of the last platform
        if (platforms.Length > 1) {
            platformGrowthStartPos = platforms[platforms.Length - 2].transform.position;
        } else {
            platformGrowthStartPos = topPlatform.transform.position;
        }

        GameObject newStemPiece = CreateStem(platforms[0].transform.position + Vector3.down * 1.5f);
        GrowStem(newStemPiece, platforms[0].transform.position);

        // create all but the last stem
        for (int i = 1; i < platforms.Length-1; i++) {
            newStemPiece = CreateStem(platforms[i - 1].transform.position);
            GrowStem(newStemPiece, platforms[i].transform.position);
        }
    }

    private GameObject CreateStem(Vector3 location) {
        GameObject newStemPiece = Instantiate(stemPrefab, this.transform);
        newStemPiece.transform.position = location;

        SpriteRenderer renderer = newStemPiece.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = -1000; // draw behind everything

        return newStemPiece;
    }

    private void GrowStem(GameObject stemPiece, Vector3 targetLocation) {
        Vector3 vectorToNextLeaf = targetLocation - stemPiece.transform.position;
        float stemAngle = Mathf.Atan2(vectorToNextLeaf.y, vectorToNextLeaf.x);
        stemPiece.transform.rotation = Quaternion.EulerAngles(0f, 0f, stemAngle);
        stemPiece.transform.localScale = new Vector3(vectorToNextLeaf.magnitude, 0.1f, 1f);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void Water() {
        throw new System.NotImplementedException("OHP");
    }

    public void Grow() {
        StartCoroutine(GrowthCoroutine());
    }

    // Coroutine to grow highest platform after creation
    // We assume that previous plant portion is the same, and we're just growing the top leaf
    IEnumerator GrowthCoroutine() {

        topPlatform.transform.position = platformGrowthStartPos;
        topPlatform.transform.localScale = Vector3.zero;
        Debug.Log(platformGrowthStartPos + " " + platformGrowthTargetPos);

        GameObject newStemPiece = CreateStem(platformGrowthStartPos);

        //Vector3 topPlatformStartPos
        while (growthProgress < 1f) {
            Debug.Log(growthProgress);
            growthProgress += Time.deltaTime / growthTime;
            topPlatform.transform.position = Vector3.Lerp(platformGrowthStartPos, platformGrowthTargetPos, growthProgress);
            topPlatform.transform.localScale = Vector3.one * growthProgress;

            // Update dynamic stem
            GrowStem(newStemPiece, topPlatform.transform.position);
            yield return null;
        }
    }
}
