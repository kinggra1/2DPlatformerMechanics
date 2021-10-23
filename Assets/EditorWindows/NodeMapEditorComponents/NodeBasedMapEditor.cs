using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;

[ExecuteAlways]
public class NodeBasedMapEditor : EditorWindow {
    private List<SceneNode> nodes = new List<SceneNode>();
    private List<RoomConnection> connections = new List<RoomConnection>();
    private Dictionary<Guid, DoorwayHandle> allDoorwaysById = new Dictionary<Guid, DoorwayHandle>();

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle doorwayStyle;
    private GUIStyle outPointStyle;

    private DoorwayHandle selectedDoorway;

    private Vector2 offset;
    private Vector2 drag;

    [MenuItem("Window/Node Based Editor")]
    private static void OpenWindow() {
        NodeBasedMapEditor window = GetWindow<NodeBasedMapEditor>();
        window.titleContent = new GUIContent("Level Connection Editor");
    }

    protected void Awake() {
        Initialize();
    }

    protected void Start() {
        Initialize();
    }

    protected void OnDestroy() {
        Debug.Log("OnDestroy called");
    }

    protected void OnDisable() {
        Debug.Log("OnDisable called");
    }

    protected void OnEnable() {
        Debug.Log("OnEnable called");
#if UNITY_EDITOR
        // Initialize();
        EditorApplication.playModeStateChanged += Initialize;
#endif
    }

    private void Initialize(PlayModeStateChange state) {
        Debug.Log("State: " + state);
        if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode) {
            Initialize();
        }
    }

    private void Initialize() {
        Debug.Log("Initializing NodeBasedMapEditor");
        ClearEditor();
        LoadNodesFromFile();
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        doorwayStyle = new GUIStyle();
        doorwayStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        doorwayStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        doorwayStyle.border = new RectOffset(4, 4, 12, 12);
    }

    protected void OnGUI() {
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawNodes();
        DrawConnections();

        DrawConnectionLine(Event.current);

        DrawButtons();

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void DrawButtons() {
        if (GUI.Button(new Rect(0, 0, 100, 20), "Reload From File")) {
            ClearEditor();
            LoadNodesFromFile();
        }

        if (GUI.Button(new Rect(0, 30, 100, 20), "Save To File")) {
            SaveEditorDataToFile();
        }

        if (GUI.Button(new Rect(0, 60, 100, 20), "Reset Editor")) {
            ClearEditor();
            LoadNodesFromFile();
        }

        if (GUI.Button(new Rect(0, 90, 300, 20), "Build ALL Scene Data")) {
            ClearEditor();
            GlobalMapWriteSceneData.WriteDataForAllScenesInBuildSettings();
            LoadNodesFromFile();
        }
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor) {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++) {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++) {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes() {
        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].Draw();
            }
        }
    }

    private void DrawConnections() {
        if (connections != null) {
            for (int i = 0; i < connections.Count; i++) {
                connections[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e) {
        drag = Vector2.zero;

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
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e) {
        if (nodes != null) {
            for (int i = nodes.Count - 1; i >= 0; i--) {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged) {
                    GUI.changed = true;
                }
            }
        }
    }

    private void DrawConnectionLine(Event e) {
        if (selectedDoorway != null) {
            bool ltr = selectedDoorway.doorwayRect.x < e.mousePosition.x;
            Vector2 handleDirection = ltr ? Vector2.left : Vector2.right;
            // Vector2 handleDirection = new Vector2(secondDoorway.doorwayRect.x - firstDoorway.doorwayRect.x, secondDoorway.doorwayRect.y - firstDoorway.doorwayRect.y).normalized;
            Handles.DrawBezier(
                selectedDoorway.doorwayRect.center,
                e.mousePosition,
                selectedDoorway.doorwayRect.center - handleDirection * 25f,
                e.mousePosition + handleDirection * 25f,
                RoomConnection.CONNECTION_COLOR,
                null,
                3f
            );

            GUI.changed = true;
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition) {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddSceneNode(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnDrag(Vector2 delta) {
        drag = delta;

        if (nodes != null) {
            for (int i = 0; i < nodes.Count; i++) {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    private void OnClickAddSceneNode(Vector2 mousePosition) {
        AddSceneNode(mousePosition, 200, 50);
    }

    private void AddSceneNode(Vector2 position, int width, int height) {
        if (nodes == null) {
            nodes = new List<SceneNode>();
        }
        // nodes.Add(new SceneNode(position, 200, 50, nodeStyle, selectedNodeStyle, doorwayStyle, outPointStyle, OnClickDoorwayHandle, OnClickRemoveNode));
    }

    private void AddSceneNode(Vector2 position, int width, int height, Texture2D image, string sceneName, List<EditorDoorwayData> doorwayData) {
        if (nodes == null) {
            nodes = new List<SceneNode>();
        }
        nodes.Add(new SceneNode(sceneName, position, width, height, image, doorwayData, nodeStyle, selectedNodeStyle, doorwayStyle, OnClickDoorwayHandle, OnClickRemoveNode));
    }

    private void OnClickDoorwayHandle(DoorwayHandle doorway) {

        if (selectedDoorway != null) {
            if (selectedDoorway.node != doorway.node) {
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

    private void CreateConnection(DoorwayHandle selectedDoorway, DoorwayHandle secondClickedDoorway) {
        if (connections == null) {
            connections = new List<RoomConnection>();
        }

        connections.Add(new RoomConnection(selectedDoorway, secondClickedDoorway, OnClickRemoveConnection));
    }

    private void ClearConnectionSelection() {
        selectedDoorway = null;
    }






    private void ClearEditor() {
        nodes.Clear();
        connections.Clear();
        allDoorwaysById.Clear();
    }

    private void InitSceneNode(string directory) {
        string imagePath = directory + "/bounds_image.png";
       
        if (!File.Exists(imagePath)) {
            return;
        }
        
        byte[] imageData = File.ReadAllBytes(imagePath);
        string sceneName = new DirectoryInfo(directory).Name;

        Texture2D image = new Texture2D(2, 2); // TODO: Allocate appropriate space.
        image.filterMode = FilterMode.Point;
        image.LoadImage(imageData);

        string doorwayDataPath = directory + "/doorways.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        EditorRoomData roomData = new EditorRoomData(); // Default empty room data.
        if (File.Exists(doorwayDataPath)) {
            file = File.Open(doorwayDataPath, FileMode.Open);
            roomData = (EditorRoomData)bf.Deserialize(file);
            file.Close();
        }

        string editorDataPath = directory + "/editor_tile.dat";
        // Default scene created the first time this editor loads the scene. May be overwritten by file load below.
        SceneNode sceneNode;
        if (File.Exists(editorDataPath)) {
            file = File.Open(editorDataPath, FileMode.Open);
            sceneNode = (SceneNode)bf.Deserialize(file);
            AddSceneNode(new Vector2(sceneNode.rect.x, sceneNode.rect.y), image.width, image.height, image, sceneName, roomData.doorwayData);
            file.Close();
        } else {
            AddSceneNode(new Vector2(200, 200), image.width, image.height, image, sceneName, roomData.doorwayData);
        }

    }

    private void LoadNodesFromFile() {
        string sceneDataPath = GlobalMapManager.MAPS_FILEPATH_ROOTDIR;
        foreach (string directory in Directory.GetDirectories(sceneDataPath)) {
            InitSceneNode(directory);
            // AddSceneNode(Vector2.zero, 200, 50); 
        }

        // Document all Doorways for this Scene
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
                foreach (KeyValuePair<Guid, Guid> connection in doorwayConnections) {
                    // If Initialization of all Scene data found both doorways, add to list of connections. Otherwise
                    // we drop the connection if either doorway has disappeared.
                    if (allDoorwaysById.ContainsKey(connection.Key) && allDoorwaysById.ContainsKey(connection.Value)) {
                        CreateConnection(allDoorwaysById[connection.Key], allDoorwaysById[connection.Value]);
                    }
                }
            }
            file.Close();
        }

    }

    void SaveEditorDataToFile() {
        string savePath = GlobalMapManager.MAPS_FILEPATH_ROOTDIR;
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        foreach (SceneNode sceneNode in nodes) {
            string roomDataPath = GlobalMapManager.GetFilePathForSceneName(sceneNode.sceneName) + "editor_tile.dat";
            file = File.Create(roomDataPath);
            bf.Serialize(file, sceneNode);
            file.Close();
        }

        // Store a mapping of all GUID to Doorway objects.
        string doorwaysById = GlobalMapManager.DOORS_BY_ID_FILEPATH;
        file = File.Create(doorwaysById);
        DoorsById doorsById = new DoorsById();
        foreach (SceneNode scene in nodes) {
            foreach (DoorwayHandle doorway in scene.doorways) {
                doorsById.Add(doorway.id, new SerlializedDoorway(scene.sceneName, doorway.id));
            }
        }
        bf.Serialize(file, doorsById);
        file.Close();

        // Split out the list of DoorwayConnections into a Symmetrical Map of doorway connections.
        string doorwayConnectionsFilePath = GlobalMapManager.DOOR_CONNECTIONS_FILEPATH;
        file = File.Create(doorwayConnectionsFilePath);
        DoorwayConnections doorwayConnections = new DoorwayConnections();
        foreach (RoomConnection connection in connections) {
            doorwayConnections.Add(connection.firstDoorway.id, connection.secondDoorway.id);
            doorwayConnections.Add(connection.secondDoorway.id, connection.firstDoorway.id);
        }
        bf.Serialize(file, doorwayConnections);
        file.Close();
    }










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
}