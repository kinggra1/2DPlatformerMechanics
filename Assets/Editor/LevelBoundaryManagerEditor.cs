using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(LevelBoundaryManager), true)]
public class LevelBoundaryManagerEditor : Editor {

    List<Doorway> doorways = new List<Doorway>();
    System.Array doorLocations = System.Enum.GetValues(typeof(LevelBoundaryManager.DoorLocation));

    public override void OnInspectorGUI() {
        LevelBoundaryManager levelBoundaryManager = target as LevelBoundaryManager;
        LevelBoundaryManager.LevelDoorDictionary oldDoorMap = levelBoundaryManager.doorMap;
        //EditorGUILayout.ObjectField(levelBoundaryManager, typeof(LevelBoundaryManager), false);

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        doorways.Clear();
        foreach (LevelBoundaryManager.DoorLocation doorLocation in doorLocations) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(doorLocation.ToString());
            doorways.Add(EditorGUILayout.ObjectField(oldDoorMap[doorLocation], typeof(Doorway), true) as Doorway);
            EditorGUILayout.EndHorizontal();
        }
        //SceneAsset newScene = EditorGUILayout.ObjectField("scene", oldScene, typeof(SceneAsset), false) as SceneAsset;
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("doorways"));


        if (EditorGUI.EndChangeCheck()) {
            SerializedProperty doorMap = serializedObject.FindProperty("_doorMap");

            /*
            SerializedProperty itr = serializedObject.GetIterator();
            while (itr.Next(true)) {
                Debug.Log(itr.name);
            }
            */

            SerializedProperty doorMapKeys = doorMap.FindPropertyRelative("keys");
            SerializedProperty doorMapValues = doorMap.FindPropertyRelative("values");

            doorMapKeys.ClearArray();
            doorMapValues.ClearArray();
            for (int i = 0; i < doorLocations.Length; i++) {
                doorMapKeys.InsertArrayElementAtIndex(i);
                doorMapKeys.GetArrayElementAtIndex(i).enumValueIndex = doorLocations.GetValue(i).GetHashCode();

                doorMapValues.InsertArrayElementAtIndex(i);
                doorMapValues.GetArrayElementAtIndex(i).objectReferenceValue = doorways[i];
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}