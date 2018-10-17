using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private static GameController instance = null;

    private PlayerController player;
    private InventorySystem inventory;

    private TimeSystem timeSystem;


    public LevelBoundaryManager.DoorLocation whereToAppear;

    // Use this for initialization
    void Awake () {

        if (instance == null) {
            // Keep this object around between scenes.
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }
	}

    private void Start() {
        FindNeededObjects();
    }

    private void FindNeededObjects() {
        player = PlayerController.GetInstance();
        inventory = InventorySystem.GetInstance();
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

    public void ExitCurrentRoom(LevelBoundaryManager.DoorLocation whereToAppear) {
        Save();
        this.whereToAppear = whereToAppear;
    }

    private void OnLevelWasLoaded(int level) {
        FindNeededObjects();
        Load();
        // TODO: FIX CAUZE THIS IS GOING TO BE BUGGY ON FIRST LOAD OOPS
        LevelBoundaryManager levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();
        Doorway playerEnteredFrom = levelBoundaryManager.doorMap[whereToAppear];
        if (playerEnteredFrom != null) {
            player.TeleportAfterSceneLoad(playerEnteredFrom.playerSpawnMarker.transform.position);
        }
    }





    // These are THE saving and loading functions, which will recursively manage all saving and loading
    private void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        file = File.Create(Application.persistentDataPath + "/player.dat");
        bf.Serialize(file, player.Save());
        file.Close();

        file = File.Create(Application.persistentDataPath + "/inventory.dat");
        bf.Serialize(file, inventory.Save());
        file.Close();
    }

    private void Load() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        if (File.Exists(Application.persistentDataPath + "/player.dat")) {
            file = File.Open(Application.persistentDataPath + "/player.dat", FileMode.Open);
            player.Load((PlayerData)bf.Deserialize(file));
            file.Close();
        }

        if (File.Exists(Application.persistentDataPath + "/inventory.dat")) {
            file = File.Open(Application.persistentDataPath + "/inventory.dat", FileMode.Open);
            inventory.Load((InventoryData)bf.Deserialize(file));
            file.Close();
        }
    }
}
