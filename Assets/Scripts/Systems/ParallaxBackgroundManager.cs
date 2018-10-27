using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackgroundManager : MonoBehaviour {

    [System.Serializable]
    public class ScrollLayer {
        [Tooltip("Distance of this layer relative to player. Positive is forward, negative is backward.")]
        public int distance;
        [Tooltip("The parent of all graphical objects in the given layer.")]
        public GameObject entityParent;
    }
    [Tooltip("Parallax scrolling layers. Distance represents how far from the camera the layer is in z axis, GameObject is the parent of all scene elements in that layer.")]
    public ScrollLayer[] scrollLayers;

    // Distance at which there will be no more movement
    public int vanishingDistance = 200;

    private GameObject mainCamera;

    private void CreateScaledLayers() {
        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f;

        // TODO: Clean this up after done experimenting so that LevelBoundaryManager and EdgeCollider are on ParallaxBoundary object (name it LevelBoundaryManager)
        LevelBoundaryManager levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();
        Vector2 maxBoundaryDimensions = levelBoundaryManager.GetMaxBoundaryDimensions();
        Vector2 playerLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth, maxBoundaryDimensions.y - cameraHeight);
        Vector2 cameraLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth * 2f, maxBoundaryDimensions.y - cameraHeight * 2f);

        float minScale = Mathf.Max(cameraWidth / maxBoundaryDimensions.x, cameraHeight / maxBoundaryDimensions.y);

        foreach (ScrollLayer layer in scrollLayers) {
            // Rescale the layer so that when we are centered on a given point, all layers line up as if they were on the same spot
            float scale = Mathf.Lerp(minScale, 2f - minScale, ((float)(vanishingDistance - layer.distance) / (2f * vanishingDistance)));
            // float scale = minScale + ((float)(vanishingDistance - layer.distance) / vanishingDistance) / (1f - minScale);
            // layer.entityParent.transform.localScale = Vector2.one * ((float)(vanishingDistance - layer.distance) / vanishingDistance);
            layer.entityParent.transform.localScale = Vector2.one * scale;
            // layer.entityParent.transform.localScale = (maxBoundaryDimensions / playerLimit) * ((float)(vanishingDistance - layer.distance) / vanishingDistance);
        }
    }

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main.gameObject;

        // I'm guessing we don't want this, but it was a fun experiment
        CreateScaledLayers();
	}
	
    // We run in FixedUpdate to keep up exactly with the camera velocity, which is also set in FixedUpdate
	void FixedUpdate () {

        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f;

        // TODO: Clean this up after done experimenting so that LevelBoundaryManager and EdgeCollider are on ParallaxBoundary object (name it LevelBoundaryManager)
        LevelBoundaryManager levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();
        Vector2 maxBoundaryDimensions = levelBoundaryManager.GetMaxBoundaryDimensions();
        Vector2 playerLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth, maxBoundaryDimensions.y - cameraHeight);
        Vector2 cameraLimit = new Vector2(maxBoundaryDimensions.x - cameraWidth * 2f, maxBoundaryDimensions.y - cameraHeight * 2f);

        float xScale = cameraWidth / maxBoundaryDimensions.x;
        float yScale = cameraHeight / maxBoundaryDimensions.y;
        float minScale = Mathf.Max(cameraWidth / maxBoundaryDimensions.x, cameraHeight / maxBoundaryDimensions.y);

        foreach (ScrollLayer layer in scrollLayers) {

            //float xScale = maxBoundaryDimensions.x * minScale / maxBoundaryDimensions.x; // (cameraLimit.x / playerLimit.x);// (cameraHeight / cameraLimit.y);
            // float scale = (maxBoundaryDimensions.x - cameraWidth) / maxBoundaryDimensions.x;
            //float scale = (cameraWidth / cameraHeight);
            //float scale = Mathf.Lerp(minScale, 1f, ((float)(vanishingDistance - layer.distance) / vanishingDistance));
            //float index = ((mainCamera.transform.position - this.transform.position).magnitude / )
            // float scale = cameraLimit.y / cameraLimit.x;
            // Debug.Log(scale);
            // scale = 1 - scale;
            // cameraWidth/height goes down, scale should go down.

            float scale = Mathf.Lerp(minScale, 2f - minScale, ((float)(vanishingDistance - layer.distance) / (2f * vanishingDistance)));
            Vector2 scaledBounds = maxBoundaryDimensions * scale;

            float maxLayerOffsetX = (maxBoundaryDimensions.x - scaledBounds.x) / 2f;
            float maxCameraOffsetX = (maxBoundaryDimensions.x - cameraWidth) / 2f;

            float maxLayerOffsetY = (maxBoundaryDimensions.y - scaledBounds.y) / 2f;
            float maxCameraOffsetY = (maxBoundaryDimensions.y - cameraHeight) / 2f;

            float scaleX = (maxLayerOffsetX / maxCameraOffsetX);
            float scaleY = (maxLayerOffsetY / maxCameraOffsetY);
            
            // scale = 1 - (scaledBounds.y / scaledBounds.x);
            // scale = 
            // When distance=200 y translation should be 1:1

            float distance = layer.distance;
            Vector2 newPosition = this.transform.position +
                Vector3.Scale((mainCamera.transform.position - this.transform.position), new Vector3(scaleX, scaleY, 0f));
                // Offset of the main camera from the center of the entire parallax system is used as offset into background location
                // Vector3.Scale((mainCamera.transform.position - this.transform.position), new Vector3(scale, 1f, 0f))
                //  * (distance/vanishingDistance);

            layer.entityParent.transform.position = newPosition;
            
        }
	}
}
