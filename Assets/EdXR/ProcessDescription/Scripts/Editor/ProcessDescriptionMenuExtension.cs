using EdXR.Utilities;
using UnityEditor;
using UnityEngine;

namespace EdXR.ProcessDescription
{
    /// <summary>
    /// Menu extension to show process description options on right click in hierarchy view.
    /// </summary>
    public class ProcessDescriptionMenuExtension : MonoBehaviour
    {
        [MenuItem("GameObject/EdXR/Create Process Description", priority = 0)]
        private static void CreateLabel()
        {
            var name = SceneHelpers.UniqueInstanceName<StepController>("[EdXR] Process Description");
            var stepControllerObject = new GameObject(name);
            stepControllerObject.AddComponent<StepController>();
        }
    }
}
