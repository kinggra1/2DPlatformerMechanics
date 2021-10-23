using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour {

    private static GameController instance = null;

    public GameObject mainCameraPrefab;
    public GameObject playerPrefab;
    public GameObject waterSpritePrefab;
    public GameObject canvasPrefab;

    private PlayerController player;
    private InventorySystem inventory;

    private TimeSystem timeSystem;


    public LevelBoundaryManager.DoorLocation whereToAppear;

    // Set before loading a new level, this is the GUID of the Doorway we should find and appear at when loading in.
    private System.Guid targetDoorwayGuid;

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

        // Initialize everything else we need to play the game! 
        // This covers scenes where the only thing we add is a GameController and everything should create itself.
        if (!GameObject.FindGameObjectWithTag("MainCamera")) {
            Instantiate(mainCameraPrefab);
        }
        if (!GameObject.FindObjectOfType<PlayerController>()) {
            Instantiate(playerPrefab);
        }
        if (!GameObject.FindObjectOfType<WaterSpriteController>()) {
            Instantiate(waterSpritePrefab);
        }
        // This includes InventorySystem and TimeSystem, which are children of the Canvas.
        if (!GameObject.FindObjectOfType<Canvas>()) {
            Instantiate(canvasPrefab);
        }


        // Objects that don't have a GameObject prefab manage their own object initialization.
    }

    private void Start() {
        FindNeededObjects();
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += LevelLoadedCallback;
    }
    private void OnDisable() {
        SceneManager.sceneLoaded -= LevelLoadedCallback;
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

    public void ExitCurrentRoom(System.Guid targetDoorwayGuid) {
        Save();
        this.targetDoorwayGuid = targetDoorwayGuid;
        // this.whereToAppear = whereToAppear;
    }

    private void LevelLoadedCallback(Scene scene, LoadSceneMode mode) {
        FindNeededObjects();
        Load();

        LevelBoundaryManager levelBoundaryManager = GameObject.Find("LevelBoundaryManager").GetComponent<LevelBoundaryManager>();

        // Find the Doorway to appear at by raw matching search :( 
        // TODO: Serlize and pass around spawn position efficiently.
        bool foundDoor = false;
        foreach (Doorway doorway in FindObjectsOfType<Doorway>()) {
            if (doorway.guid.Equals(targetDoorwayGuid)) {
                player.TeleportAfterSceneLoad(doorway.playerSpawnMarker.transform.position);
                foundDoor = true;
                break;
            }
        }

        if (!foundDoor) {
            Debug.LogError("Unable to find Doorway to load into level at. DoorwayGuid: " + targetDoorwayGuid);
        }
    }

    private RoomData SaveRoomState() {
        RoomData data = new RoomData();

        foreach (PlantableZone zone in GameObject.FindObjectsOfType<PlantableZone>()) {
            data.plantableZoneNames.Add(zone.name);
            data.plantableZoneData.Add(zone.Save());
        }

        return data;
    }

    private void LoadRoomState(RoomData data) {
        // Load is only called if we already have data for this room, so we should delete default plantable zones
        foreach (PlantableZone zone in GameObject.FindObjectsOfType<PlantableZone>()) {
            Destroy(zone.gameObject);
        }

        // Create all plants that were stored in level data.
        for (int i = 0; i < data.plantableZoneNames.Count; i++) {
            string name = data.plantableZoneNames[i];
            PlantableZoneData zoneData = data.plantableZoneData[i];

            GameObject obj = Instantiate(inventory.plantableZonePrefabMap[zoneData.zoneType]);
            PlantableZone plantableZone = obj.GetComponent<PlantableZone>();

            plantableZone.Load(zoneData);
        }
    }






    // These are THE game state saving and loading functions, which will recursively manage all saving and loading
    private void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        file = File.Create(Application.persistentDataPath + "/player.dat");
        bf.Serialize(file, player.Save());
        file.Close();

        file = File.Create(Application.persistentDataPath + "/inventory.dat");
        bf.Serialize(file, inventory.Save());
        file.Close();

        // Save information about the Scene that we are currently leaving
        // Use the scene name as the file name, because Scene names are unique and immutable.
        string levelPath = Application.persistentDataPath + "/levels/";
        if (!Directory.Exists(levelPath)) {
            Directory.CreateDirectory(levelPath);
        }
        file = File.Create(levelPath + SceneManager.GetActiveScene().name + ".dat");
        bf.Serialize(file, SaveRoomState());
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

        string levelPath = Application.persistentDataPath + "/levels/";
        if (File.Exists(levelPath + SceneManager.GetActiveScene().name + ".dat")) {
            file = File.Open(levelPath + SceneManager.GetActiveScene().name + ".dat", FileMode.Open);
            LoadRoomState((RoomData)bf.Deserialize(file));
            file.Close();
        }
    }
}

[Serializable]
public class RoomData {
    public List<string> plantableZoneNames = new List<string>();
    public List<PlantableZoneData> plantableZoneData = new List<PlantableZoneData>();
}