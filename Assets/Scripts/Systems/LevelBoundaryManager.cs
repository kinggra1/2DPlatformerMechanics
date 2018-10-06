using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelBoundaryManager : MonoBehaviour {

    BoxCollider2D boundary;

    // Start is called before the first frame update
    void Start() {
        boundary = GetComponent<BoxCollider2D>();
        float cameraWidth = Camera.main.orthographicSize * Camera.main.aspect * 2f * 2f;
        float cameraHeight = Camera.main.orthographicSize * 2f * 2f;
        Vector2 cameraLimit = new Vector2(boundary.size.x - cameraWidth, boundary.size.y - cameraHeight);

        Camera.main.GetComponent<CameraFollow>().SetSceneLimitDims(cameraLimit);

        DestroyImmediate(boundary);
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        gameObject.AddComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update() {
        
    }
}
