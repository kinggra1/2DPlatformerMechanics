using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Doorway), true)]
public class DoorwayEditor : Editor {

    string[] doorLocations = System.Enum.GetNames(typeof(LevelBoundaryManager.DoorLocation));

    public override void OnInspectorGUI() {
        Doorway doorway = target as Doorway;
        LevelBoundaryManager.DoorLocation whereToAppear = doorway.whereToAppear;
        GameObject playerSpawnMarker = doorway.playerSpawnMarker;
        SceneAsset oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(doorway.scenePath);

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        SceneAsset newScene = EditorGUILayout.ObjectField("Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Where to appear in other room:");
        int selectedDoorLocation = EditorGUILayout.Popup(whereToAppear.GetHashCode(), doorLocations);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Where we appear here:");
        GameObject marker = EditorGUILayout.ObjectField(playerSpawnMarker, typeof(GameObject), true) as GameObject;
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck()) {
            string newPath = AssetDatabase.GetAssetPath(newScene);
            SerializedProperty scenePathProperty = serializedObject.FindProperty("scenePath");
            scenePathProperty.stringValue = newPath;

            SerializedProperty selectedDoor = serializedObject.FindProperty("whereToAppear");
            selectedDoor.intValue = selectedDoorLocation;

            SerializedProperty playerSpawn = serializedObject.FindProperty("playerSpawnMarker");
            playerSpawn.objectReferenceValue = marker;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
