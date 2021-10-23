using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalMapManager : MonoBehaviour {

    public static GlobalMapManager Instance;

    private NodeBasedMapEditor.DoorsById doorsById;
    private NodeBasedMapEditor.DoorwayConnections doorwayConnections;
    public static string MAPS_FILEPATH_ROOTDIR { 
        get { return Application.dataPath + "/_EditorGenerated/Maps/"; } 
        set { MAPS_FILEPATH_ROOTDIR = value; }
    }
    public static string DOORS_BY_ID_FILEPATH {
        get { return MAPS_FILEPATH_ROOTDIR + "doorways_by_id.dat"; }
        set { MAPS_FILEPATH_ROOTDIR = value; }
    }
    public static string DOOR_CONNECTIONS_FILEPATH {
        get { return MAPS_FILEPATH_ROOTDIR + "doorway_mapping.dat"; }
        set { MAPS_FILEPATH_ROOTDIR = value; }
    }

    protected void Awake() {
        if (Instance != null) {
            Destroy(this.gameObject);
        }

        Instance = this;
    }

    public static string GetFilePathForScene(Scene scene) {
        return MAPS_FILEPATH_ROOTDIR + scene.name + '/';
    }

    public static string GetFilePathForSceneName(string sceneName) {
        return MAPS_FILEPATH_ROOTDIR + sceneName + '/';
    }

    public bool HasConnectedDoor(System.Guid doorGuid) {
        return doorwayConnections.ContainsKey(doorGuid);
    }

    public NodeBasedMapEditor.SerlializedDoorway GetConnectedDoor(System.Guid doorGuid) {
        if (!HasConnectedDoor(doorGuid)) {
            Debug.LogError("Unable to find connected door for: " + doorGuid);
            return null;
        }

        if (!doorsById.ContainsKey(doorwayConnections[doorGuid])) {
            Debug.LogError("Connected Door in Connections map, but not DoorsById: " + doorwayConnections[doorGuid]);
            return null;
        }

        return doorsById[doorwayConnections[doorGuid]];
    }

    public NodeBasedMapEditor.SerlializedDoorway GetTargetDoorway(System.Guid doorGuid) {
        return doorsById[doorGuid];
    }

    void Start() {
        string doorwayConnectionsFilePath = DOOR_CONNECTIONS_FILEPATH;
        if (File.Exists(doorwayConnectionsFilePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(doorwayConnectionsFilePath, FileMode.Open);
            doorwayConnections = (NodeBasedMapEditor.DoorwayConnections)bf.Deserialize(file);
            file.Close();
        }

        string doorsByIdFilePath = DOORS_BY_ID_FILEPATH;
        if (File.Exists(doorsByIdFilePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(doorsByIdFilePath, FileMode.Open);
            doorsById = (NodeBasedMapEditor.DoorsById)bf.Deserialize(file);
            file.Close();
        }
    }

    void Update() {
        
    }
}
