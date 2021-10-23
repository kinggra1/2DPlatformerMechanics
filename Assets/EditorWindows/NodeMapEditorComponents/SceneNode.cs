﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SceneNode {
    public SerializableRect rect;
    public string sceneName;
    [NonSerialized] public Texture2D image;

    [NonSerialized] public bool isDragged;
    [NonSerialized] public bool isSelected;

    [NonSerialized] public List<DoorwayHandle> doorways = new List<DoorwayHandle>();

    [NonSerialized] public GUIStyle style;
    [NonSerialized] public GUIStyle defaultNodeStyle;
    [NonSerialized] public GUIStyle selectedNodeStyle;

    [NonSerialized] public Action<SceneNode> OnRemoveNode;

    public SceneNode(string sceneName, Vector2 position, float width, float height, Texture2D image, List<EditorDoorwayData> doorwayData, GUIStyle nodeStyle, GUIStyle selectedStyle, GUIStyle doorwayStyle, Action<DoorwayHandle> OnClickDoorwayHandle, Action<SceneNode> OnClickRemoveNode) {
        this.sceneName = sceneName;
        rect = new SerializableRect(position.x, position.y, width, height);
        style = nodeStyle;
        this.image = image;
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;

        ConstructDoorwayHandles(image, doorwayData, doorwayStyle, OnClickDoorwayHandle);
    }

    private void ConstructDoorwayHandles(Texture2D image, List<EditorDoorwayData> doorwayData, GUIStyle doorwayStyle, Action<DoorwayHandle> OnClickDoorwayHandle) {
        int doorWidth = 10;
        int doorHeight = 10;
        foreach (EditorDoorwayData doorway in doorwayData) {

            System.Guid doorwayId = doorway.id;
            // Load old toggle state from static map.
            // bool toggleState = doorwayGuiToggles.ContainsKey(doorwayId) ? doorwayGuiToggles[doorwayId] : false;
            // bool toggleState = false;

            int xPos = doorway.gridXPos;
            // Unit Tilemap indexes from bottom-left. Rect is top-left. Invert height.
            int yPos = image.height - doorway.gridYPos;

            // Clamp the side to the edge of the image.
            // int xPos = Mathf.Clamp(doorway.gridXPos, 0, image.width - doorWidth);
            // int yPos = Mathf.Clamp(fixedY, 0, image.height - doorHeight);



            Rect localDoorwayRect = new Rect(xPos, yPos, doorWidth, doorHeight);

            doorways.Add(new DoorwayHandle(this, doorwayId, localDoorwayRect.position, doorwayStyle, OnClickDoorwayHandle));


            // Draw the Toggle element itself, and store current value to toggle state dictionary.
            // toggleState = GUI.Toggle(localToggleRect, toggleState, "");
            // doorwayGuiToggles[doorwayId] = toggleState;

            // doorway.editorPosition = new Rect(data.localEditorPosition.x + localToggleRect.x, data.localEditorPosition.y + localToggleRect.y, doorWidth, doorHeight);
            // toggleState is active when the user clicks on this specific Doorway toggle.
            /*
            if (toggleState) {
                // If we already have a Doorway selected, see if we can make a connection to this one
                if (doorwaySelected) {
                    if (doorwayId != activeDoorway.doorwayGuid) {
                        // Attempt to make a connection to this second doorway.
                        TargetDoorway thisSceneTarget = new TargetDoorway(data.name, doorwayId, doorway.editorPosition);

                        doorwayConnections[doorwayId] = activeDoorway;
                        doorwayConnections[activeDoorway.doorwayGuid] = thisSceneTarget;
                        Debug.Log("Connecting doorways: " + activeDoorway + " " + data.localEditorPosition);
                    }
                    ResetDoorwaySelection();
                }
                else {
                    doorwaySelected = true;
                    activeDoorway = new TargetDoorway(data.name, doorwayId, doorway.editorPosition);
                }
            }
            

            // Draw square color based on selection status.
            if (doorwaySelected && activeDoorway.doorwayGuid == doorwayId) {
                EditorGUI.DrawRect(new Rect(xPos, yPos, doorWidth, doorHeight), Color.green);
            }
            else {
                EditorGUI.DrawRect(new Rect(xPos, yPos, doorWidth, doorHeight), Color.red);
            }
            */
        }
    }

    public void Drag(Vector2 delta) {
        Rect r = (Rect)rect;
        r.position += delta;
        rect = r;
    }

    public void Draw() {
        GUI.DrawTexture(new Rect(rect.x, rect.y, image.width, image.height), image);
        foreach (DoorwayHandle doorway in doorways) {
            doorway.Draw();
        }
    }

    public bool ProcessEvents(Event e) {
        switch (e.type) {
            case EventType.MouseDown:
                if (e.button == 0) {
                    if (((Rect)rect).Contains(e.mousePosition)) {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && ((Rect)rect).Contains(e.mousePosition)) {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged) {
                    Drag(e.delta);
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