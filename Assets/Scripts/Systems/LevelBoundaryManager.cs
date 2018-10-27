using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(EdgeCollider2D))]
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

    private EdgeCollider2D boundary;
    private Vector2 maxBoundaryDimensions;

    void Awake() {
        boundary = GetComponent<EdgeCollider2D>();
        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f;

        maxBoundaryDimensions = CalculateBoundaryDimensions();

        Vector2 cameraLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth*2f, maxBoundaryDimensions.y - cameraHeight*2f);
        Vector2 playerLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth, maxBoundaryDimensions.y - cameraHeight);

        Vector2 bottomLeft = (Vector2)transform.position - playerLimit / 2f;
        Vector2 topRight = (Vector2)transform.position + playerLimit / 2f;
        Camera.main.GetComponent<CameraFollow>().SetSceneLimitDims(bottomLeft, topRight);

        // Rescale the bounds so that we run into the edge in the appropriate location for the smaller far background
        // this.transform.localScale = (playerLimit) / maxBoundaryDimensions;
        
    }

    private Vector2 CalculateBoundaryDimensions() {
        float minX = Mathf.Infinity, minY = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity, maxY = Mathf.NegativeInfinity;

        foreach (Vector2 point in boundary.points) {
            if (point.x < minX) {
                minX = point.x;
            }
            if (point.x > maxX) {
                maxX = point.x;
            }
            if (point.y < minY) {
                minY = point.y;
            }
            if (point.y > maxY) {
                maxY = point.y;
            }
        }

        return new Vector2(maxX - minX, maxY - minY);
    }

    public Vector2 GetMaxBoundaryDimensions() {
        return maxBoundaryDimensions;
    }

    // Update is called once per frame
    void Update() {
        
    }
}
