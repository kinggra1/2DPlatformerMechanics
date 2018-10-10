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

        FindNeededObjects();

        // Keep this object around between scenes.
        DontDestroyOnLoad(this.gameObject);
	}

    private void FindNeededObjects() {
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        timeSystem = TimeSystem.GetInstance();
    }

    public LevelBoundaryManager.DoorLocation lastDoorLocationUsed;

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

        bool escapeGamePressed = Input.GetKeyDown(KeyCode.Escape);
        if (escapeGamePressed) {
            QuitGame();
        }
    }

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void QuitGame() {
        Application.Quit();
    }

    private void OnLevelWasLoaded(int level) {
        FindNeededObjects();
        // TODO: FIX CAUZE THIS IS GOING TO BE BUGGY ON FIRST LOAD OOPS
        LevelBoundaryManager levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();
        Doorway playerEnteredFrom = levelBoundaryManager.doorMap[lastDoorLocationUsed];
        if (playerEnteredFrom != null) {
            player.TeleportAfterSceneLoad(playerEnteredFrom.playerSpawnMarker.transform.position);
        }
    }
}
