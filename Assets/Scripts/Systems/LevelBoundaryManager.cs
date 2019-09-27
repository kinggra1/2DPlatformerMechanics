using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelBoundaryManager : MonoBehaviour {

    // Collection of possible locations for doors. This is kind of abstract, so it's up to your interpretation as to
    // what qualifies as "TopRight", but hopefully these are useful directional anchors. Can add as many as we need.
    // Used for cross-referencing the answer to the question of "where do we enter these scene when we go through a given door in another scene?"
    public enum DoorLocation { TopLeft, TopCenter, TopRight, RightTop, RightCenter, RightBottom, BottomRight, BottomCenter, BottomLeft, LeftBottom, LeftCenter, LeftTop}

    // This is black magic to make the editor work. We need a non-generic Serialized... you know what... https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html 
    [Serializable]
    public class LevelDoorDictionary : SerializableDictionary<DoorLocation, Doorway> { }

    [SerializeField]
    private LevelDoorDictionary _doorMap = new LevelDoorDictionary();

    public LevelDoorDictionary doorMap { get { return _doorMap; } }

    private Vector2 maxBoundaryDimensions;
    private Vector2 boundaryCenter;

    private CompositeCollider2D groundColliders;

    void Awake() {
        groundColliders = GetComponentInChildren<CompositeCollider2D>();
        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f;

        CalculateBoundaryDimensions();

        Vector2 cameraLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth*2f, maxBoundaryDimensions.y - cameraHeight*2f);
        Vector2 playerLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth, maxBoundaryDimensions.y - cameraHeight);

        Vector2 bottomLeft = boundaryCenter - playerLimit / 2f;
        Vector2 topRight = boundaryCenter + playerLimit / 2f;
        Camera.main.GetComponent<CameraFollow>().SetSceneLimitDims(bottomLeft, topRight);

        // Rescale the bounds so that we run into the edge in the appropriate location for the smaller far background
        // this.transform.localScale = (playerLimit) / maxBoundaryDimensions;
        
    }

    private void CalculateBoundaryDimensions() {
        if (groundColliders && groundColliders.bounds.size.magnitude > 1f) {
            boundaryCenter = groundColliders.bounds.center;
            maxBoundaryDimensions = groundColliders.bounds.size;
        } else {
            Debug.Log("Ya background is fucked.");
            boundaryCenter = new Vector2(0f, 0f);
            maxBoundaryDimensions = new Vector2(500f, 100f);
        }
    }

    public Vector2 GetMaxBoundaryDimensions() {
        return maxBoundaryDimensions;
    }

    public Vector2 GetBoundaryCenter() {
        return boundaryCenter;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
