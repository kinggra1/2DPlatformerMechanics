using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SceneNode : ISerializationCallbackReceiver {
    public SerializableRect rect;
    public string sceneName;
    public EditorRoomData roomData;
    // Serialize to keep in Editor between Edit/Play
    [SerializeField] private Texture2D image;

    // Rebuilt on Deserialize to avoid circular serialization.
    public List<DoorwayHandle> doorways = new List<DoorwayHandle>();

    public GUIStyle nodeStyle;
    public GUIStyle doorwayStyle;
    public GUIStyle defaultNodeStyle;
    public GUIStyle selectedNodeStyle;

    public Action<DoorwayHandle> OnClickDoorwayHandle;
    public Action<SceneNode> OnRemoveNode;

    [NonSerialized] public bool isDragged;
    [NonSerialized] public bool isSelected;

    public SceneNode(EditorRoomData roomData, string sceneName, Texture2D image, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle doorwayStyle, Action<DoorwayHandle> OnClickDoorwayHandle, Action<SceneNode> OnClickRemoveNode) {
        this.sceneName = sceneName;
        this.roomData = roomData;
        this.rect = roomData.sceneRect;
        // Override width/height with passed in image in case the (unserialized) image has changed.
        this.rect.width = image.width;
        this.rect.height = image.height;
        this.nodeStyle = nodeStyle;
        this.doorwayStyle = doorwayStyle;
        this.image = image;
        this.defaultNodeStyle = nodeStyle;
        this.selectedNodeStyle = selectedStyle;
        this.OnClickDoorwayHandle = OnClickDoorwayHandle;
        this.OnRemoveNode = OnClickRemoveNode;

        Debug.Log("REconstructing");
        ConstructDoorwayHandlesFromData();
    }

    private void ConstructDoorwayHandlesFromData() {
        int doorWidth = 10;
        int doorHeight = 10;
        foreach (EditorDoorwayData doorway in roomData.doorwayData) {

            System.Guid doorwayId = doorway.id;

            int xPos = doorway.gridXPos;
            // Unity Tilemap indexes from bottom-left. Rect is top-left. Invert height.
            int yPos = image.height - doorway.gridYPos;
            Rect localDoorwayRect = new Rect(xPos, yPos, doorWidth, doorHeight);
            doorways.Add(new DoorwayHandle(doorwayId, ((Rect)this.rect).position, localDoorwayRect.position, doorwayStyle, OnClickDoorwayHandle));
        }
    }

    public void Drag(Vector2 delta) {
        Rect r = (Rect)rect;
        r.position += delta;
        foreach (DoorwayHandle handle in doorways) {
            handle.SetParentPosition(r.position); 
        }
        rect = r;
    }

    public void Draw(float zoomLevel) {
        GUI.DrawTexture(
            new Rect(
                rect.x * zoomLevel, 
                rect.y * zoomLevel, 
                image.width * zoomLevel, 
                image.height * zoomLevel
            ), image);
        foreach (DoorwayHandle doorway in doorways) {
            doorway.Draw(zoomLevel);
        }
    }

    public bool ProcessEvents(Event e) {
        Vector2 worldMousePos = NodeBasedMapEditor.ScreenToMapWorld(e.mousePosition);
        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0) {
                    if (((Rect)rect).Contains(worldMousePos)) {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        nodeStyle = selectedNodeStyle;
                    }
                    else {
                        GUI.changed = true;
                        isSelected = false;
                        nodeStyle = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && ((Rect)rect).Contains(worldMousePos)) {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged) {
                    Drag(NodeBasedMapEditor.ScreenToMapWorld(e.delta));
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    private void ProcessContextMenu() {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode() {
        if (OnRemoveNode != null) {
            OnRemoveNode(this);
        }
    }










    [Serializable]
    public class SceneNodeSaveData {

    }

    private SceneNodeSaveData Save() {
        SceneNodeSaveData data = new SceneNodeSaveData();
        return data;
    }

    public EditorRoomData GetSavedState() {
        roomData.sceneRect = rect;
        return roomData;
    }

    public void OnBeforeSerialize() {

    }

    public void OnAfterDeserialize() {
        ConstructDoorwayHandlesFromData();
    }
}





[System.Serializable]
    public class SerializableRect {
        public float x;
        public float y;
        public float width;
        public float height;

        public SerializableRect(float x, float y, float width, float height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        // Implicit converter to use this in place of a Rect
        public static implicit operator Rect(SerializableRect rect) {
            return new Rect(rect.x, rect.y, rect.width, rect.height);
        }

        public static implicit operator SerializableRect(Rect rect) {
            return new SerializableRect(rect.x, rect.y, rect.width, rect.height);
        }
    }