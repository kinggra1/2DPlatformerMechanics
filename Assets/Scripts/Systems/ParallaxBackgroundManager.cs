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
        foreach (ScrollLayer layer in scrollLayers) {
            // Rescale the layer so that when we are centered on a given point, all layers line up as if they were on the same spot
            layer.entityParent.transform.localScale = Vector2.one * ((float)(vanishingDistance - layer.distance) / vanishingDistance);
        }
    }

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main.gameObject;

        // I'm guessing we don't want this, but it was a fun experiment
        CreateScaledLayers();
	}
	
    // We run in FixedUpdate to keep up exactly with the camera velocity, which is also set in FixedUpdate
	void Update () {
        // We assume that the player starts at (0, 0, 0) I guess
        foreach (ScrollLayer layer in scrollLayers) {

            // 90 degree angle, don't move at all, 0 degree angle move 100%

            float distance = layer.distance;
            Vector2 newPosition = this.transform.position + mainCamera.transform.position * (distance/vanishingDistance);
            // newPosition.y = layer.entityParent.transform.position.y;
            layer.entityParent.transform.position = newPosition;
            

            /*
            float factor = (110 - layer.distance);
            factor = factor == 0 ? 1 : factor;
            Vector2 newPosition = this.transform.position + mainCamera.transform.position / factor;
            layer.entityParent.transform.position = newPosition;
            */
        }
	}
}
