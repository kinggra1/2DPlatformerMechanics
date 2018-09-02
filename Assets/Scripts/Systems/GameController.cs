using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private static GameController instance = null;

    private PlayerController player;
    private TimeSystem timeSystem;

	// Use this for initialization
	void Awake () {
        if (instance != null) {
            return;
        }
        instance = this;

        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        timeSystem = TimeSystem.GetInstance();
	}

    // now if we forget to put a GameController in the scene, we can still
    // call this and one will be dynamically created
    public static GameController GetInstance() {
        if (instance == null) {
            instance = new GameObject().AddComponent<GameController>();
            instance.name = "GameController";
        }
        return instance;
    }

    void Update() {
        bool debugResetLevelPressed = Input.GetKeyDown(KeyCode.R);   
        if (debugResetLevelPressed) {
            ReloadScene();
        }
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
