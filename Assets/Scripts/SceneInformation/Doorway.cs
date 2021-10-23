using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[ExecuteAlways]
public class Doorway : MonoBehaviour, ISerializationCallbackReceiver {
    // Fields to create a unique, persistant ID for these Doorways across ALL saves/loads/edits so that we can link between scenes correctly.
    public System.Guid guid = System.Guid.Empty;
    [SerializeField]
    public byte[] serializedGuid;

    [SerializeField]
    public string scenePath;

    [Tooltip("Which door in the target scene to appear in.")]
    [SerializeField]
    public LevelBoundaryManager.DoorLocation whereToAppear;

    public GameObject playerSpawnMarker;

    void Awake() {
        CreateGuid();
        Debug.Log(guid);
#if UNITY_EDITOR
        // This lets us detect if we are a prefab instance or a prefab asset.
        // A prefab asset cannot contain a GUID since it would then be duplicated when instanced.
        PrefabType prefabType = PrefabUtility.GetPrefabType(this);
        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab) {
            serializedGuid = new byte[0];
            guid = System.Guid.Empty;
        }

        // If we are creating a new GUID for a prefab instance of a prefab, allow GUID marking to break from Prefab instance.
        else if (prefabType == PrefabType.PrefabInstance) {
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
#endif 
    }

    // Update is called once per frame
    void Update() {
        
    }

    // When de-serializing or creating this component, we want to either restore our serialized GUID
    // or create a new one.
    void CreateGuid() {
        // if our serialized data is invalid, then we are a new object and need a new GUID
        if (serializedGuid == null || serializedGuid.Length != 16) {
            guid = System.Guid.NewGuid();
            serializedGuid = guid.ToByteArray();

        }
        else if (guid == System.Guid.Empty) {
            // otherwise, we should set our system guid to our serialized guid
            guid = new System.Guid(serializedGuid);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize() {
        if (guid != System.Guid.Empty) {
            serializedGuid = guid.ToByteArray();
        }
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize() {
        if (serializedGuid != null && serializedGuid.Length == 16) {
            guid = new System.Guid(serializedGuid);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ((1<<collision.gameObject.layer) == AI.PlayerLayermask) {
            //GameController.GetInstance().ExitCurrentRoom(whereToAppear);
            //SceneManager.LoadScene(scenePath);

            if (GlobalMapManager.Instance.HasConnectedDoor(guid)) {
                NodeBasedMapEditor.SerlializedDoorway targetDoor = GlobalMapManager.Instance.GetConnectedDoor(guid);
                GameController.GetInstance().ExitCurrentRoom(targetDoor.doorwayGuid);
                SceneManager.LoadScene(targetDoor.sceneNameToLoad);
            }
        }
    }
}
