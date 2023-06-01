using EdXR;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector gui for the <see cref="InteractiveObject"/>.
/// </summary>
[CustomEditor(typeof(InteractiveObject))]
public class InteractiveObjectEditor : Editor
{
    private bool showEvents;
    private SerializedProperty prefab;
    private SerializedProperty manipulationType;
    private SerializedProperty useHandles;
    private SerializedProperty allowFarManipulation;
    private SerializedProperty physics;
    private SerializedProperty mass;
    private SerializedProperty onManipulationStarted;
    private SerializedProperty onManipulationEnded;

    /// <inheritdoc/>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(prefab);
        if (prefab.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Prefab needs to be set to a valid template.", MessageType.Warning);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(manipulationType);
        EditorGUILayout.PropertyField(useHandles);
        EditorGUILayout.PropertyField(allowFarManipulation);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(physics);

        if (physics.intValue > 1)
        {
            EditorGUILayout.PropertyField(mass);
            if (useHandles.boolValue)
            {
                EditorGUILayout.HelpBox("Using handles with physics can cause problems.", MessageType.Info);
            }
        }

        EditorGUILayout.Space();

        // Advanced Settings group foldout.
        showEvents = EditorGUILayout.Foldout(showEvents, "Events", true, EditorStyles.foldoutHeader);

        if (showEvents)
        {
            EditorGUILayout.PropertyField(onManipulationStarted);
            EditorGUILayout.PropertyField(onManipulationEnded);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        prefab = serializedObject.FindProperty("prefab");
        manipulationType = serializedObject.FindProperty("manipulationType");
        useHandles = serializedObject.FindProperty("useHandles");
        allowFarManipulation = serializedObject.FindProperty("allowFarManipulation");
        physics = serializedObject.FindProperty("physics");
        mass = serializedObject.FindProperty("mass");
        onManipulationStarted = serializedObject.FindProperty("onManipulationStarted");
        onManipulationEnded = serializedObject.FindProperty("onManipulationEnded");
    }
}