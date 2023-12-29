using UnityEditor;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera.Editor
{
    [CustomEditor(typeof(TacticalCamera))]
    public class TacticalCameraEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("The values below are only applied during Awake. Assigning new values will not be applied.", MessageType.Info);
                EditorGUILayout.Space(20f);
            }

            SerializedObject tacticalCamera = new SerializedObject(target);

            EditorGUILayout.PropertyField(tacticalCamera.FindProperty(nameof(TacticalCamera.initialSettings)), new GUIContent("Settings"));
            EditorGUILayout.PropertyField(tacticalCamera.FindProperty(nameof(TacticalCamera.initialInputProvider)), new GUIContent("Input provider"));
            EditorGUILayout.PropertyField(tacticalCamera.FindProperty(nameof(TacticalCamera.initialCameraBounds)), new GUIContent("Operational bounds (optional)"));

            tacticalCamera.ApplyModifiedProperties();
        }
    }
}

