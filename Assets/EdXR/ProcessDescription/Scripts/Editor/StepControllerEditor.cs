using EdXR;
using EdXR.ProcessDescription;
using EdXR.Utilities;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector gui for the StepController.
/// </summary>
[CustomEditor(typeof(StepController))]
public class StepControllerEditor : Editor
{
    private SerializedProperty steps;
    private SerializedProperty stepUI;

    private int previewIndex = -1;

    /// <inheritdoc/>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        StepController stepController = (StepController)target;
        EditorGUILayout.PropertyField(stepUI);

        if (stepController.StepUI == null)
        {
            EditorGUILayout.HelpBox("The UI is optional. You can also use the step controller methods directly.", MessageType.Info);
        }
        if (stepController.StepUI == null && GUILayout.Button("Add Template UI"))
        {
            var stepUIObject = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EdXR/ProcessDescription/Prefabs/StepUI_Template.prefab")) as GameObject;
            SceneHelpers.PosToSceneView(stepUIObject.transform);
            stepUIObject.name = SceneHelpers.UniqueInstanceName<StepUI>("[EdXR] StepUI");
            stepController.StepUI = stepUIObject.GetComponent<StepUI>();
            stepUIObject.transform.SetParent(stepController.transform);
            Selection.activeObject = stepUIObject;
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(steps);

        EditorGUILayout.HelpBox("When removing steps from the list above, the child objects in the hiearchy will not be deleted. If you want to delete step content for good, manually delete the game object in the hierarchy, too.", MessageType.None);

        if (GUILayout.Button("Add Step"))
        {
            stepController.AddStep();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Previous"))
        {
            stepController.PreviewToggleAll(false);
            previewIndex = Mathf.Clamp(--previewIndex, 0, stepController.StepCount - 1);
            stepController.Preview(previewIndex);
        }

        if (GUILayout.Button("Next"))
        {
            stepController.PreviewToggleAll(false);
            previewIndex = Mathf.Clamp(++previewIndex, 0, stepController.StepCount - 1);
            stepController.Preview(previewIndex);
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Toggle All"))
        {
            stepController.PreviewToggleAll();
        }

        EditorGUILayout.HelpBox("Note: Persistent content will not be shown as persistent in the Editor preview.", MessageType.None);
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        steps = serializedObject.FindProperty("steps");
        stepUI = serializedObject.FindProperty("stepUI");
    }
}
