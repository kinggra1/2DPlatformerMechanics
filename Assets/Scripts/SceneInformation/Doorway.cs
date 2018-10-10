using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Doorway : MonoBehaviour {

    [SerializeField]
    public string scenePath;

    [Tooltip("Which door in the target scene to appear in.")]
    [SerializeField]
    public LevelBoundaryManager.DoorLocation whereToAppear;

    public GameObject playerSpawnMarker;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ((1<<collision.gameObject.layer) == AI.PlayerLayermask) {
            GameController.GetInstance().lastDoorLocationUsed = whereToAppear;
            SceneManager.LoadScene(scenePath);
        }
    }
}
