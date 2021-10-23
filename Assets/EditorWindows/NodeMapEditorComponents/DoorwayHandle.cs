using System;
using System.Runtime.Serialization;
using UnityEngine;


public class DoorwayHandle {
    public SceneNode node;
    public Guid id;
    public Rect doorwayRect;
    public Vector2 relativeOffset;
    public GUIStyle style;
    public Action<DoorwayHandle> OnClickConnectionPoint;

    public DoorwayHandle(SceneNode node, Guid id, Vector2 relativeOffset, GUIStyle style, Action<DoorwayHandle> OnClickConnectionPoint) {
        this.node = node;
        this.id = id;
        this.relativeOffset = relativeOffset;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        doorwayRect = new Rect(node.rect.x + relativeOffset.x, node.rect.y + relativeOffset.y, 15f, 15f);
    }

    public void Draw() {
        doorwayRect.x = node.rect.x + relativeOffset.x - doorwayRect.width * 0.5f;
        doorwayRect.y = node.rect.y + relativeOffset.y - doorwayRect.height * 0.5f;

        if (GUI.Button(doorwayRect, "", style)) {
            if (OnClickConnectionPoint != null) {
                OnClickConnectionPoint(this);
            }
        }
    }
}