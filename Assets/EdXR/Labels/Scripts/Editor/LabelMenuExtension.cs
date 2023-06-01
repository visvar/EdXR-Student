using EdXR.Utilities;
using UnityEngine;
using UnityEditor;

namespace EdXR.Labels
{
    /// <summary>
    /// Menu extension to show label options on right click in hierarchy view.
    /// </summary>
    public class LabelMenuExtension : MonoBehaviour
    {
        [MenuItem("GameObject/EdXR/Create Label", priority = 0)]
        private static void CreateLabel()
        {
            var name = SceneHelpers.UniqueInstanceName<LabelContent>("[EdXR] Label");

            var labelObject = new GameObject(name);
            SceneHelpers.PosToSceneView(labelObject.transform);
            var labelContent = labelObject.AddComponent<LabelContent>();
            labelContent.SetTemplate((GameObject)AssetDatabase.LoadAssetAtPath("Assets/EdXR/Labels/Prefabs/Utility/EdXR_Label_Template.prefab", typeof(GameObject)));
            labelContent.transform.localScale = 0.35f * Vector3.one;
        }
    }
}