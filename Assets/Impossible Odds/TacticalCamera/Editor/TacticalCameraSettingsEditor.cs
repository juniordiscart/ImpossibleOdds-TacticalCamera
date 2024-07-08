using UnityEditor;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera.Editor
{
    [CustomEditor(typeof(TacticalCameraSettings))]
    public class TacticalCameraSettingsEditor : UnityEditor.Editor
    {
        private bool showGeneralSettings;
        private bool showMovementSettings;
        private bool showAltitudeSettings;
        private bool showRotationSettings;
        private bool showTiltSettings;
        private bool showFieldOfViewSettings;
        private bool showWorldInteractionSettings;

        public override void OnInspectorGUI()
        {
            SerializedObject settings = new SerializedObject(target);
            GUIStyle boldFoldout = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            showGeneralSettings = EditorGUILayout.Foldout(showGeneralSettings, "Time settings", true, boldFoldout);

            if (showGeneralSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Ignore Time Scale", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Should the camera continue to function regardless of the game's time scale?", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.ignoreTimeScale)), new GUIContent("Ignore time scale"));

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, "Movement", true, boldFoldout);

            if (showMovementSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The camera can move at different speeds when it is low to the ground versus when it is high up. Set the speed range, and determine with the speed transition curve how this value range should be applied.", MessageType.None);

                EditorGUILayout.LabelField("Speed transition range");
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                SerializedProperty speedRange = settings.FindProperty(nameof(TacticalCameraSettings.movementSpeedRange));
                EditorGUILayout.PropertyField(speedRange.FindPropertyRelative(nameof(ValueRange.min)), new GUIContent("Low"));
                EditorGUILayout.PropertyField(speedRange.FindPropertyRelative(nameof(ValueRange.max)), new GUIContent("High"));
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.movementSpeedTransition)), new GUIContent("Speed transition curve"));

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Fade-out", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("When no more movement input is given, the camera will smoothly come to a halt. Define the behaviour using a fade-out curve and time value.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.movementFade)), new GUIContent("Speed fade-out curve"));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.movementFadeTime)), new GUIContent("Speed fade-out time"));

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Move to target", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Define how fast the camera should move to a specific target position.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.moveToTargetSmoothingTime)), new GUIContent("Move to target smooth time"));

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            showRotationSettings = EditorGUILayout.Foldout(showRotationSettings, "Rotation", true, boldFoldout);

            if (showRotationSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Rotation speed is determined by an input factor, e.g. mouse or controller input. The value below will determine the maximum speed the camera can rotate.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.maxRotationSpeed)), new GUIContent("Max rotational speed"));

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Fade-out", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("When no more rotational input is given, the camera will smoothly come to a halt. Define the behaviour using a fade-out curve and time value.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.rotationalFade)), new GUIContent("Rotational fade-out curve"));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.rotationalFadeTime)), new GUIContent("Rotational fade-out time"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.LabelField("Orbit versus pivot", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The tactical camera can orbit around a point. However, if no suitable orbit point is found, is the camera allowed to pivot on its current location instead? Otherwise, no rotational action is taken.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.allowPivotOnFailedOrbit)), new GUIContent("Allow pivot when no orbit point was found"));

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            showAltitudeSettings = EditorGUILayout.Foldout(showAltitudeSettings, "Altitude operation", true, boldFoldout);

            if (showAltitudeSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Absolute altitude range", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Define at what altitudes the camera is allowed to operate in. This range of values also drives the tilt and field-of-view settings.\n\nThese are expressed world-space units.", MessageType.None);
                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                SerializedProperty heightRange = settings.FindProperty(nameof(TacticalCameraSettings.absoluteHeightRange));
                EditorGUILayout.PropertyField(heightRange.FindPropertyRelative(nameof(ValueRange.min)), new GUIContent("Min"));
                EditorGUILayout.PropertyField(heightRange.FindPropertyRelative(nameof(ValueRange.max)), new GUIContent("Max"));
                EditorGUI.indentLevel--;
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            showTiltSettings = EditorGUILayout.Foldout(showTiltSettings, "Tilt", true, boldFoldout);

            if (showTiltSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Tilt ranges", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("The camera can be allowed to tilt up and down in different ranges based on its altitude. The camera's forward vector leveled with the horizon is considered to be the zero-point.\n\nThe tilt transition curve defines how the camera transitions between these tilt ranges.", MessageType.None);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Low tilt range");
                EditorGUILayout.BeginHorizontal();
                SerializedProperty tiltRangeLow = settings.FindProperty(nameof(TacticalCameraSettings.tiltRangeLow));
                EditorGUILayout.PropertyField(tiltRangeLow.FindPropertyRelative(nameof(ValueRange.min)), new GUIContent("Min"));
                EditorGUILayout.PropertyField(tiltRangeLow.FindPropertyRelative(nameof(ValueRange.max)), new GUIContent("Max"));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("High tilt range");
                EditorGUILayout.BeginHorizontal();
                SerializedProperty tiltRangeHigh = settings.FindProperty(nameof(TacticalCameraSettings.tiltRangeHigh));
                EditorGUILayout.PropertyField(tiltRangeHigh.FindPropertyRelative(nameof(ValueRange.min)), new GUIContent("Min"));
                EditorGUILayout.PropertyField(tiltRangeHigh.FindPropertyRelative(nameof(ValueRange.max)), new GUIContent("Max"));
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.tiltRangeTransition)), new GUIContent("Tilt transition curve"));

                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            showFieldOfViewSettings = EditorGUILayout.Foldout(showFieldOfViewSettings, "Field-of-view", true, boldFoldout);

            if (showFieldOfViewSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();

                SerializedProperty useDynamicFoV = settings.FindProperty(nameof(TacticalCameraSettings.useDynamicFieldOfView));
                EditorGUILayout.PropertyField(useDynamicFoV, new GUIContent("Enable dynamic field-of-view"));
                EditorGUILayout.HelpBox("The camera's field-of-view can change based on its current altitude.", MessageType.None);

                if (useDynamicFoV.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Field-of-view", EditorStyles.boldLabel);
                    EditorGUILayout.HelpBox("Based on the camera's current altitude in the operational altitude range, the field-of-view can change based on the transition curve.", MessageType.None);

                    EditorGUILayout.LabelField("Field-of-view transition range");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    SerializedProperty fovRange = settings.FindProperty(nameof(TacticalCameraSettings.dynamicFieldOfViewRange));
                    EditorGUILayout.PropertyField(fovRange.FindPropertyRelative(nameof(ValueRange.min)), new GUIContent("Min"));
                    EditorGUILayout.PropertyField(fovRange.FindPropertyRelative(nameof(ValueRange.max)), new GUIContent("Max"));
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.dynamicFieldOfViewTransition)), new GUIContent("Field-of-view transition curve"));
                }
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            showWorldInteractionSettings = EditorGUILayout.Foldout(showWorldInteractionSettings, "World interaction", true, boldFoldout);

            if (showWorldInteractionSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The camera will require some limited interaction with the game world's colliders.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.interactionMask)), new GUIContent("World interaction mask"));
                EditorGUILayout.HelpBox("The maximum length of a ray cast operation to determine a target position an orbital rotation point.", MessageType.None);
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.interactionDistance)), new GUIContent("Max ray casting distance"));
                EditorGUILayout.PropertyField(settings.FindProperty(nameof(TacticalCameraSettings.interactionBubbleRadius)), new GUIContent("Collision radius"));
            }
            
            settings.ApplyModifiedProperties();
        }
    }
}
