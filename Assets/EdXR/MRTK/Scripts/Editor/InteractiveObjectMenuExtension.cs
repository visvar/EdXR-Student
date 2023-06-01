using UnityEngine;
using UnityEditor;

namespace EdXR.Labels
{
    /// <summary>
    /// Extension to create a context menu entry for making objects interactive.
    /// </summary>
    public class InteractiveObjectMenuExtension : MonoBehaviour
    {
        [MenuItem("GameObject/EdXR/Make Object Interactive", priority = 0)]
        private static void MakeObjectInteractive()
        {
            var selectedObject = Selection.activeObject as GameObject;
            if (selectedObject != null)
            {
                if (selectedObject.GetComponentInChildren<InteractiveObject>() != null)
                {
                    Debug.LogError("This game object (or one of its children) is already interactive.");
                    return;
                }

                selectedObject.AddComponent<InteractiveObject>().SetTemplate((Transform)AssetDatabase.LoadAssetAtPath("Assets/EdXR/MRTK/Prefabs/Utility/EdXR_InteractiveObject_Template.prefab", typeof(Transform)));
            }
        }

        // Validate the menu item defined by the function above.
        // The menu item will be disabled if this function returns false.
        [MenuItem("GameObject/EdXR/Make Object Interactive", true)]
        private static bool ValidateLogSelectedTransformName()
        {
            // Return false if no transform is selected.
            return Selection.activeTransform != null;
        }
    }
}