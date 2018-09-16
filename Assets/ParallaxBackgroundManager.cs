using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackgroundManager : MonoBehaviour {

    [System.Serializable]
    public class ScrollLayer {
        public int distance;
        public GameObject entityParent;
    }
    [Tooltip("Parallax scrolling layers. Int represents how far from the player the layer is in z axis, GameObject is the parent of all scene elements in that layer.")]
    public ScrollLayer[] scrollLayers;

    private GameObject mainCamera;

	// Use this for initialization
	void Start () {
        mainCamera = Camera.main.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
        // We assume that the player starts at (0, 0, 0) I guess
        foreach (ScrollLayer layer in scrollLayers) {
            float factor = (110 - layer.distance);
            factor = factor == 0 ? 1 : factor;
            Vector2 newPosition = this.transform.position + mainCamera.transform.position / factor;
            layer.entityParent.transform.position = newPosition;

        }
	}
}
