using System;
using System.Runtime.Serialization;
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
        this.style = style;
        this.OnClickDoorwayHandle = OnClickDoorwayHandle;
        this.doorwayRect = new Rect(parentPosition.x + relativePosition.x, parentPosition.y + relativePosition.y, 15f, 15f);
    }

    public void SetParentPosition(Vector2 parentPosition) {
        this.parentPosition = parentPosition;
    }

    public void Draw(float zoomLevel) {
        doorwayRect.x = parentPosition.x * zoomLevel + relativePosition.x * zoomLevel - doorwayRect.width * 0.5f;
        doorwayRect.y = parentPosition.y * zoomLevel + relativePosition.y * zoomLevel - doorwayRect.height * 0.5f;

        if (GUI.Button(doorwayRect, "", style)) {
            if (OnClickDoorwayHandle != null) {
                OnClickDoorwayHandle(this);
            }
        }
    }
}