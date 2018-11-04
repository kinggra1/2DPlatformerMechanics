using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackgroundManager : MonoBehaviour {

    [System.Serializable]
    public class ScrollLayer {
        [Tooltip("Distance of this layer relative to player. Positive is forward, negative is backward.")]
        public int distance;
        [Tooltip("The calculated scale of this layer relative to the distance from player.")]
        public float scale;
        [Tooltip("The parent of all graphical objects in the given layer.")]
        public GameObject entityParent;
    }
    [Tooltip("Parallax scrolling layers. Distance represents how far from the camera the layer is in z axis, GameObject is the parent of all scene elements in that layer.")]
    public ScrollLayer[] scrollLayers;

    // Distance at which there will be no more movement
    public int vanishingDistance = 200;

    private GameObject mainCamera;
    private float cameraWidth;
    private float cameraHeight;
    private float minScale;
    private LevelBoundaryManager levelBoundaryManager;
    private Vector2 maxBoundaryDimensions;

    private void InitializeLayers() {
        // Let the rest of this function assume that we have at least 1 background layer to work with.
        if (scrollLayers.Length == 0) {
            return;
        }

        cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        cameraHeight = Camera.main.orthographicSize * 2f;

        levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();
        maxBoundaryDimensions = levelBoundaryManager.GetMaxBoundaryDimensions();

        minScale = Mathf.Max(cameraWidth / maxBoundaryDimensions.x, cameraHeight / maxBoundaryDimensions.y);

        // We go off of the assumption that the background should be the same size as the level boundary (otherwise we
        // wouldn't see portions of it), so let's resize based on the size of the first background layer image. The ratio of
        // this to the smallest dimension we can shink to line up perfectly with the levelBoundary dimensions themselves.
        // I.e. shrink the background layers until they fit on top of the LevelBoundary as defined by tilemap.
        GameObject firstBackground = scrollLayers[0].entityParent;
        SpriteRenderer firstBackgroundSprite = firstBackground.GetComponentInChildren<SpriteRenderer>();
        Vector2 firstBackgroundSize = firstBackgroundSprite.sprite.bounds.size;
        float backgroundRescale = Mathf.Max(maxBoundaryDimensions.x / firstBackgroundSize.x, maxBoundaryDimensions.y / firstBackgroundSize.y);
        this.transform.localScale = Vector3.one * backgroundRescale;

        foreach (ScrollLayer layer in scrollLayers) {
            // Rescale the layer so that when we are centered on a given point, all layers line up as if they were on the same spot
            layer.scale = Mathf.Lerp(minScale, 2f - minScale, ((float)(vanishingDistance - layer.distance) / (2f * vanishingDistance)));
            layer.entityParent.transform.localScale = Vector2.one * layer.scale;
        }
    }

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main.gameObject;

        InitializeLayers();
	}
	
    // We run in FixedUpdate to keep up exactly with the camera velocity, which is also set in FixedUpdate
	void FixedUpdate () {

        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f;

        foreach (ScrollLayer layer in scrollLayers) {

            Vector2 scaledBounds = maxBoundaryDimensions * layer.scale;

            float maxLayerOffsetX = (maxBoundaryDimensions.x - scaledBounds.x) / 2f;
            float maxCameraOffsetX = (maxBoundaryDimensions.x - cameraWidth) / 2f;

            float maxLayerOffsetY = (maxBoundaryDimensions.y - scaledBounds.y) / 2f;
            float maxCameraOffsetY = (maxBoundaryDimensions.y - cameraHeight) / 2f;

            float scaleX = (maxLayerOffsetX / maxCameraOffsetX);
            float scaleY = (maxLayerOffsetY / maxCameraOffsetY);

            Vector2 levelBoundaryCenter = levelBoundaryManager.GetBoundaryCenter();
            float distance = layer.distance;
            Vector2 newPosition = levelBoundaryCenter +
                Vector2.Scale(((Vector2)mainCamera.transform.position - levelBoundaryCenter), new Vector2(scaleX, scaleY));

            layer.entityParent.transform.position = newPosition;
        }
	}
}
