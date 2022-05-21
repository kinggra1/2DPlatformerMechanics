using System;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

[Serializable]
public class DoorwayHandle {
    public Guid id;
    public SerializableRect doorwayRect;
    public Vector2 parentPosition;
    public Vector2 relativePosition;
    public GUIStyle style;
    public Action<DoorwayHandle> OnClickDoorwayHandle;

    public DoorwayHandle(Guid id, Vector2 parentPosition, Vector2 relativePosition, GUIStyle style, Action<DoorwayHandle> OnClickDoorwayHandle) {
        this.id = id;
        this.parentPosition = parentPosition;
        this.relativePosition = relativePosition;
        CreateStyle();
        this.OnClickDoorwayHandle = OnClickDoorwayHandle;
        this.doorwayRect = new Rect(parentPosition.x + relativePosition.x, parentPosition.y + relativePosition.y, 20f, 20f);
    }

    private void CreateStyle() {
        style = new GUIStyle();
        style.normal.background = EditorGUIUtility.Load("Assets/EditorWindows/UISprites/DoorwayHandle.png") as Texture2D;
        style.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        // style.border = new RectOffset(4, 4, 12, 12);
    }

    public void SetParentPosition(Vector2 parentPosition) {
        this.parentPosition = parentPosition;
    }

    public void Draw(float zoomLevel) {
        if (style == null) {
            CreateStyle();
        }

        doorwayRect.x = parentPosition.x * zoomLevel + relativePosition.x * zoomLevel - doorwayRect.width * 0.5f;
        doorwayRect.y = parentPosition.y * zoomLevel + relativePosition.y * zoomLevel - doorwayRect.height * 0.5f;

        Color currentGUIColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;
        if (GUI.Button(doorwayRect, "", style)) {
            if (OnClickDoorwayHandle != null) {
                OnClickDoorwayHandle(this);
            }
        }
        GUI.backgroundColor = currentGUIColor;
    }

    #region Serialization Callbacks and Helpers
    public void OnBeforeSerialize() {

    }

    public void OnAfterDeserialize() {
        CreateStyle();
    }
    #endregion
}