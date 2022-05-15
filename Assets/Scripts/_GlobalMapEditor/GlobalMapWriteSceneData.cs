using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[InitializeOnLoad]
public static class GlobalMapWriteSceneData {

    static GlobalMapWriteSceneData() {
        EditorSceneManager.sceneSaved += OnEditorSceneSaved;
    }

    // This loads ALL scenes in Build Settings one at a time and saves them.
    public static void WriteDataForAllScenesInBuildSettings() {
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
            EditorSceneManager.OpenScene(scene.path);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.Refresh();
    }

    public static void WriteDataForScene(Scene scene) {
        OnEditorSceneSaved(scene);
    }

    // Callback for sceneSaved that will create an image and metadata about this scene that we can use in GlobalMapManagerEditor.cs
    private static void OnEditorSceneSaved(Scene savedScene) {
        Tilemap tilemap = GameObject.FindObjectOfType<Tilemap>();

        // Trim excess space off the top, bottom, and sides, to find the tightest rectangle bound we can.
        tilemap.CompressBounds();

        Vector3Int tilemapSize = tilemap.size;

        Texture2D image = new Texture2D(tilemapSize.x, tilemapSize.y);
        int xOffset = tilemap.cellBounds.min.x;
        int yOffset = tilemap.cellBounds.min.y;
        foreach (Vector3Int position in tilemap.cellBounds.allPositionsWithin) {
            Color tileColor = tilemap.HasTile(position) ? Color.gray : Color.clear;
            image.SetPixel(position.x - xOffset, position.y - yOffset, tileColor);
        }

        string savePath = Application.dataPath + "/_EditorGenerated/Maps/" + savedScene.name + "/";
        if (!Directory.Exists(savePath)) {
            Directory.CreateDirectory(savePath);
        }
        // Write a PNG image reprepsenting the level geometry.
        File.WriteAllBytes(savePath + "bounds_image.png", image.EncodeToPNG());

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;
        // Write data about the doorways for this level.
        file = File.Create(savePath + "room_data.dat");
        bf.Serialize(file, CreateRoomDataForEditor(tilemap));
        file.Close();
    }

    private static EditorRoomData CreateRoomDataForEditor(Tilemap tilemap) {
        EditorRoomData data = new EditorRoomData();
        // Default value at (100, 100) for new Scene tile pos/size data.
        data.sceneRect = new SerializableRect(100, 100, 100, 100);

        int xOffset = tilemap.cellBounds.min.x;
        int yOffset = tilemap.cellBounds.min.y;

        foreach (Doorway doorway in GameObject.FindObjectsOfType(typeof(Doorway))) {
            EditorDoorwayData doorwayData = new EditorDoorwayData();
            doorwayData.id = doorway.guid;
            Vector3Int positionOnTilemap = tilemap.layoutGrid.WorldToCell(doorway.transform.position);
            doorwayData.gridXPos = positionOnTilemap.x - xOffset;
            doorwayData.gridYPos = positionOnTilemap.y - yOffset;
            data.doorwayData.Add(doorwayData);
        }

        return data;
    }
}

// Serialization objects for creating editor data for the map editor.
[Serializable]
public class EditorRoomData {
    public SerializableRect sceneRect;
    public List<EditorDoorwayData> doorwayData = new List<EditorDoorwayData>();
}

[Serializable]
public class EditorDoorwayData {
    public System.Guid id;
    public int gridXPos;
    public int gridYPos;

    // dynamically set during editor drawing.
    [NonSerialized] public Rect editorPosition;
}
