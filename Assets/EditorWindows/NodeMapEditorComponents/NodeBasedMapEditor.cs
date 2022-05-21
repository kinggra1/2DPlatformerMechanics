using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;

// [ExecuteAlways]
public class NodeBasedMapEditor : EditorWindow, ISerializationCallbackReceiver {
    [SerializeField] private List<SceneNode> nodes = new List<SceneNode>();
    [SerializeField] private List<RoomConnection> connections = new List<RoomConnection>();
    [SerializeField] private SerializableDictionary<Guid, DoorwayHandle> allDoorwaysById = new SerializableDictionary<Guid, DoorwayHandle>();

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle doorwayStyle;
    private GUIStyle outPointStyle;

    [NonSerialized] private DoorwayHandle selectedDoorway;

    private static float zoomLevel = 1f;
    private static Vector2 editorOffset = Vector2.zero;
    private Vector2 drag = Vector2.zero;

    [MenuItem("Window/Level Connection Editor")]
    private static void OpenWindow() {
        NodeBasedMapEditor window = GetWindow<NodeBasedMapEditor>();
        window.titleContent = new GUIContent("Level Connection Editor");
    }

    protected void Awake() {
        ClearEditor();
        Debug.Log("Awake Editor");
        Initialize();
        //EditorApplication.playModeStateChanged -= Initialize;
        //EditorApplication.playModeStateChanged += Initialize;
    }

    private void ClearEditor() {
        nodes.Clear();
        connections.Clear();
        allDoorwaysById.Clear();
        ClearConnectionSelection();
    }

    protected void OnValidate() {
        Debug.Log("OnValidate");
        // Initialize();
    }

    protected void OnDestroy() {
        Debug.Log("OnDestroy");
        EditorApplication.playModeStateChanged -= Initialize;
    }

    protected void OnDisable() {
        Debug.Log("OnDisable");
        EditorApplication.playModeStateChanged -= Initialize;
    }

    protected void OnEnable() {
        Debug.Log("OnEnable");
        EditorApplication.playModeStateChanged -= Initialize;
        EditorApplication.playModeStateChanged += Initialize;
    }

    private void OnInspectorUpdate() {
        // Repaint();
    }

    public void OnBeforeSerialize() {
        Debug.Log("Serialized!");
    }

    public void OnAfterDeserialize() {
        Debug.Log("Deserialized!");
    }

    private void Initialize(PlayModeStateChange state) {
        Debug.Log("MapEditor state processing new State: " + state);
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode) {
            // Initialize();
        }
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode) {
            // Initialize();
        }
    }

    #region Initialization And Subclass Creation Helpers
    private void Initialize() {
        ClearEditor();
        LoadNodesFromFile();
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
    }

    private void InitSceneNode(string directory) {
        string imagePath = directory + "/bounds_image.png";

        if (!File.Exists(imagePath)) {
            return;
        }

        // byte[] imageData = File.ReadAllBytes(imagePath);
        string sceneName = new DirectoryInfo(directory).Name;
        string relativePath = "Assets/_EditorGenerated/Maps/" + sceneName;

        Texture2D image = AssetDatabase.LoadAssetAtPath(relativePath + "/bounds_image.png", typeof(Texture2D)) as Texture2D;
        // Texture2D image = new Texture2D(2, 2); // TODO: Allocate appropriate space.
        image.filterMode = FilterMode.Point;
        // image.LoadImage(imageData);
        // AssetDatabase.CreateAsset(image, "Assets/TESTINGASSETDATABASE/"+sceneName);

        string doorwayDataPath = directory + "/room_data.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        EditorRoomData roomData;
        if (File.Exists(doorwayDataPath)) {
            file = File.Open(doorwayDataPath, FileMode.Open);
            roomData = (EditorRoomData)bf.Deserialize(file);
            AddSceneNode(roomData, image, sceneName);
            file.Close();
        }
    }

    private void AddSceneNode(EditorRoomData roomData, Texture2D image, string sceneName) {
        if (nodes == null) {
            nodes = new List<SceneNode>();
        }
        nodes.Add(new SceneNode(roomData, sceneName, image, nodeStyle, selectedNodeStyle, doorwayStyle, OnClickDoorwayHandle, OnClickRemoveNode));
    }

    private void CreateConnection(DoorwayHandle selectedDoorway, DoorwayHandle secondClickedDoorway) {
        if (connections == null) {
            connections = new List<RoomConnection>();
        }

        connections.Add(new RoomConnection(selectedDoorway, secondClickedDoorway, OnClickRemoveConnection));
    }
    #endregion

    #region OnGUI and Drawing Methods
    protected void OnGUI() {
        float minorTickSpacing = 20f * zoomLevel;
        float majorTickSpacing = 100f * zoomLevel;
        DrawGrid(minorTickSpacing, 0.2f, Color.gray);
        DrawGrid(majorTickSpacing, 0.4f, Color.gray);
        DrawGrid(300f * zoomLevel, 0.4f, Color.red);

        DrawNodes();
        DrawConnections();

        DrawConnectionToMouse(Event.current);

        DrawButtons();

        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void DrawButtons() {
        if (GUI.Button(new Rect(30, 30, 200, 20), "Save To File")) {
            SaveEditorDataToFile();
        }

        if (GUI.Button(new Rect(30, 60, 200, 20), "Reload From File")) {
            ClearEditor();
            LoadNodesFromFile();
        }

        if (GUI.Button(new Rect(30, 90, 200, 20), "Build ALL Scene Data")) {
            ClearEditor();
            GlobalMapWriteSceneData.WriteDataForAllScenesInBuildSettings();
            LoadNodesFromFile();
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing) + 1;
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing) + 1;

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        Vector2 screenSpaceEditorOffset = editorOffset * zoomLevel;

        Vector3 newOffset = new Vector3(
            screenSpaceEditorOffset.x % gridSpacing, 
            screenSpaceEditorOffset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(
                new Vector3(gridSpacing * i + newOffset.x, -gridSpacing, 0),
                new Vector3(gridSpacing * i + newOffset.x , position.height, 0f));
        }

        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(
                new Vector3(-gridSpacing, gridSpacing * j + newOffset.y, 0), 
                new Vector3(position.width, gridSpacing * j + newOffset.y, 0f));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes() {
        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].Draw(zoomLevel);
            }
        }
    }

    private void DrawConnections() {
        if (connections != null) {
            for (int i = 0; i < connections.Count; i++) {
                connections[i].Draw(zoomLevel);
            }
        }
    }

    private void DrawConnectionToMouse(Event e) {
        if (selectedDoorway != null) {
            Rect selectedDoorwayRect = (Rect)selectedDoorway.doorwayRect;
            bool ltr = selectedDoorway.doorwayRect.x < e.mousePosition.x;
            Vector2 handleDirection = ltr ? Vector2.left : Vector2.right;
            // Vector2 handleDirection = new Vector2(secondDoorway.doorwayRect.x - firstDoorway.doorwayRect.x, secondDoorway.doorwayRect.y - firstDoorway.doorwayRect.y).normalized;
            Handles.DrawBezier(
                selectedDoorwayRect.center,
                e.mousePosition,
                selectedDoorwayRect.center - handleDirection * 25f,
                e.mousePosition + handleDirection * 25f,
                RoomConnection.CONNECTION_COLOR,
                null,
                3f
            );

            GUI.changed = true;
        }
    }
    #endregion

    #region Input Handling (Process Events + Button Handlers)
    private void ProcessEvents(Event e) {
        
        // Allow Scene nodes to capture and handle the event first.
        if (nodes != null) {
            for (int i = nodes.Count - 1; i >= 0; i--) {
                bool guiChanged = nodes[i].ProcessEvents(e);
                if (guiChanged) {
                    GUI.changed = true;
                }
            }
        }

        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0) {
                    ClearConnectionSelection();
                }

                if (e.button == 1) {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0) {
                    OnDrag(ScreenToMapWorld(e.delta));
                }
                break;
            case EventType.ScrollWheel:
                OnScroll(e.mousePosition, e.delta);
                break;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition) {
        GenericMenu genericMenu = new GenericMenu();
        // genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddSceneNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    // Drag the entire editor base around. 
    // Input should be scaled to World Space, not Screen Space.
    private void OnDrag(Vector2 mapWorldDelta) {
        // Adjust global offset for drawing grid lines.
        editorOffset += mapWorldDelta;

        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].Drag(mapWorldDelta);
            }
        }

        GUI.changed = true;
    }

    // Mouse Scroll Wheel handler for zooming.
    private void OnScroll(Vector3 mousePosition, Vector2 delta) {

        Vector2 oldMousePos = ScreenToMapWorld(mousePosition);
        zoomLevel = zoomLevel * Mathf.Pow(1.01f, -delta.y);
        zoomLevel = Mathf.Clamp(zoomLevel, 0.5f, 3f);
        Vector2 newMousePos = ScreenToMapWorld(mousePosition); // Scale has changed

        // Adjust where we are scrolled to keep the mouse in the same position. 
        // This functionally zooms relative the mouse when zooming in/out
        OnDrag(newMousePos - oldMousePos);
        GUI.changed = true;
    }

    private void OnClickDoorwayHandle(DoorwayHandle doorway) {

        if (selectedDoorway != null) {
            // This previously checked scene != scene, but we now allow intra-scene connections.
            if (selectedDoorway != doorway) {
                ClearDoorwayHandleConnections(doorway);
                CreateConnection(selectedDoorway, doorway);
                ClearConnectionSelection();
            }
            else {
                ClearConnectionSelection();
            }
        } else {
            ClearDoorwayHandleConnections(doorway);
            selectedDoorway = doorway;
        }
    }

    private void ClearConnectionSelection() {
        selectedDoorway = null;
    }

    private void OnClickRemoveNode(SceneNode node) {
        if (connections != null) {
            List<RoomConnection> connectionsToRemove = new List<RoomConnection>();

            for (int i = 0; i < connections.Count; i++) {
                if (node.doorways.Contains(connections[i].firstDoorway) || node.doorways.Contains(connections[i].secondDoorway)) {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++) {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        nodes.Remove(node);
    }

    private void OnClickRemoveConnection(RoomConnection connection) {
        connections.Remove(connection);
    }

    private void ClearDoorwayHandleConnections(DoorwayHandle handle) {
        if (connections != null) {
            List<RoomConnection> connectionsToRemove = new List<RoomConnection>();
            for (int i = 0; i < connections.Count; i++) {
                if (connections[i].firstDoorway == handle || connections[i].secondDoorway == handle) {
                    connectionsToRemove.Add(connections[i]);
                }
            }
            for (int i = 0; i < connectionsToRemove.Count; i++) {
                connections.Remove(connectionsToRemove[i]);
            }
            connectionsToRemove = null;
        }
    }
    #endregion

    #region Worldspace <-> Screenspace helpers
    public static Vector2 ScreenToMapWorld(Vector2 screenPos) {
        return screenPos / zoomLevel;
    }

    public static Vector2 MapWorldToScreen(Vector2 worldPos) {
        return (worldPos - editorOffset) * zoomLevel;
    }
    #endregion

    #region Saving/Loading Methods
    private void LoadNodesFromFile() {
        string sceneDataPath = GlobalMapManager.MAPS_FILEPATH_ROOTDIR;
        // Initilaize each Scene from serialized data.
        foreach (string directory in Directory.GetDirectories(sceneDataPath)) {
            InitSceneNode(directory);
        }

        // Create a lookup index for all Doorways in all Scenes.
        foreach (SceneNode scene in nodes) {
            foreach (DoorwayHandle doorway in scene.doorways) {
                allDoorwaysById.Add(doorway.id, doorway);
            }
        }

        // Load map of connections between doorways.
        string doorwayConnectionsFilePath = GlobalMapManager.DOOR_CONNECTIONS_FILEPATH;
        if (File.Exists(doorwayConnectionsFilePath)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(doorwayConnectionsFilePath, FileMode.Open);
            if (file.Length > 0) {
                DoorwayConnections doorwayConnections = (DoorwayConnections)bf.Deserialize(file);
                HashSet<Guid> loadedDoorways = new HashSet<Guid>();
                foreach (KeyValuePair<Guid, Guid> connection in doorwayConnections) {
                    // If Initialization of all Scene data found both doorways, add to list of connections. Otherwise
                    // we drop the connection if either doorway has disappeared.
                    if (allDoorwaysById.ContainsKey(connection.Key) && allDoorwaysById.ContainsKey(connection.Value)) {
                        
                        // Skip over this doorway if we've already loaded a connection associted with it.
                        // TODO: Clean up the relationship between file serialized and Editor Doorway connections.
                        if (loadedDoorways.Contains(connection.Key)) {
                            continue;
                        }
                        CreateConnection(allDoorwaysById[connection.Key], allDoorwaysById[connection.Value]);
                        loadedDoorways.Add(connection.Key);
                        loadedDoorways.Add(connection.Value);
                    }
                }
            }
            file.Close();
        }
    }

    void SaveEditorDataToFile() {
        string savePath = GlobalMapManager.MAPS_FILEPATH_ROOTDIR;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = null;
        foreach (SceneNode sceneNode in nodes) {
            string roomDataPath = GlobalMapManager.GetFilePathForSceneName(sceneNode.sceneName) + "room_data.dat";
            try {
                file = File.Create(roomDataPath);
                bf.Serialize(file, sceneNode.GetSavedState());
                file.Close();
            } catch (IOException e) {
                Debug.LogError(String.Format("Error writing SceneNode File: %s", e));
            }
            catch (SerializationException e) {
                Debug.LogError(String.Format("Error Serializing SceneNode: %s", e));
            } finally {
                file.Close();
            }
        }

        // Store a mapping of all GUID to Doorway objects.
        string doorwaysById = GlobalMapManager.DOORS_BY_ID_FILEPATH;
        DoorsById doorsById = new DoorsById();
        foreach (SceneNode scene in nodes) {
            foreach (DoorwayHandle doorway in scene.doorways) {
                doorsById.Add(doorway.id, new SerlializedDoorway(scene.sceneName, doorway.id));
            }
        }
        try {
            file = File.Create(doorwaysById);
            bf.Serialize(file, doorsById);
        }
        catch (IOException e) {
            Debug.LogError(String.Format("Error writing Doorways file: %s", e));
        }
        catch (SerializationException e) {
            Debug.LogError(String.Format("Error Serializing Doorways: %s ", e));
        }
        finally {
            file.Close();
        }

        // Split out the list of DoorwayConnections into a Symmetrical Map of doorway connections.
        string doorwayConnectionsFilePath = GlobalMapManager.DOOR_CONNECTIONS_FILEPATH;
        DoorwayConnections doorwayConnections = new DoorwayConnections();
        foreach (RoomConnection connection in connections) {
            doorwayConnections.Add(connection.firstDoorway.id, connection.secondDoorway.id);
            doorwayConnections.Add(connection.secondDoorway.id, connection.firstDoorway.id);
        }
        try {
            file = File.Create(doorwayConnectionsFilePath);
            bf.Serialize(file, doorwayConnections);
        }
        catch (IOException e) {
            Debug.LogError(String.Format("Error writing Scene Connections file: %s", e));
        }
        catch (SerializationException e) {
            Debug.LogError(String.Format("Error Serializing Scene Connections: %s ", e));
        }
        finally {
            file.Close();
        }
    }
    #endregion

    #region Serialization Data Classes
    [Serializable]
    public class SerlializedDoorway {
        public string sceneNameToLoad;
        public System.Guid doorwayGuid;

        public SerlializedDoorway(string sceneNameToLoad, System.Guid doorwayGuid) {
            this.sceneNameToLoad = sceneNameToLoad;
            this.doorwayGuid = doorwayGuid;
        }
    }

    [Serializable]
    public class DoorsById : SerializableDictionary<System.Guid, SerlializedDoorway> {
        public DoorsById() {}
        public DoorsById(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class DoorwayConnections : SerializableDictionary<System.Guid, System.Guid> {
        public DoorwayConnections() {}
        public DoorwayConnections(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
    #endregion
}