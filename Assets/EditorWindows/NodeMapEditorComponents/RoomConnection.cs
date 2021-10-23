using System;
using UnityEditor;
using UnityEngine;

public class RoomConnection {
    public static readonly Color CONNECTION_COLOR = new Color(0.2f, 0.8f, 0.2f);
    public DoorwayHandle firstDoorway;
    public DoorwayHandle secondDoorway;
    public Action<RoomConnection> OnClickRemoveConnection;

    public RoomConnection(DoorwayHandle firstDoorway, DoorwayHandle secondDoorway, Action<RoomConnection> OnClickRemoveConnection) {
        this.firstDoorway = firstDoorway;
        this.secondDoorway = secondDoorway;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw() {
        bool ltr = firstDoorway.doorwayRect.x < secondDoorway.doorwayRect.x;
        Vector2 handleDirection = ltr ? Vector2.left : Vector2.right;
        // Vector2 handleDirection = new Vector2(secondDoorway.doorwayRect.x - firstDoorway.doorwayRect.x, secondDoorway.doorwayRect.y - firstDoorway.doorwayRect.y).normalized;
        Handles.DrawBezier(
            firstDoorway.doorwayRect.center,
            secondDoorway.doorwayRect.center,
            firstDoorway.doorwayRect.center - handleDirection * 25f,
            secondDoorway.doorwayRect.center + handleDirection * 25f,
            CONNECTION_COLOR,
            null,
            3f
        );

        if (Handles.Button((firstDoorway.doorwayRect.center + secondDoorway.doorwayRect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap)) {
            if (OnClickRemoveConnection != null) {
                OnClickRemoveConnection(this);
            }
        }
    }




}