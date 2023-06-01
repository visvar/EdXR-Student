using UnityEditor;
using UnityEngine;

namespace EdXR.Utilities
{
    /// <summary>
    /// Base class for menu extensions.
    /// </summary>
    public class SceneHelpers : MonoBehaviour
    {
        private static float spawnDistanceZ = 0.5f;

        /// <summary>
        /// Checks for occurances of this instance in the scene and create a unique name.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="baseName">The base name.</param>
        /// <returns>Unique instance name.</returns>
        public static string UniqueInstanceName<T>(string baseName)
        {
            var allObjects = FindObjectsOfType(typeof(T));
            var instanceID = 0;

            var uniqueName = baseName + " " + allObjects.Length;

            while (GameObject.Find(uniqueName))
            {
                instanceID++;
                uniqueName += " (" + instanceID + ")";
            }

            return uniqueName;
        }

        /// <summary>
        /// Checks for occurances of this instance in the children and creates a unique name.
        /// </summary>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="baseName">The base name.</param>
        /// <param name="parentObject">The parent node object in the hierarchy.</param>
        /// <returns>Unique instance name.</returns>
        public static string UniqueInstanceName<T>(string baseName, GameObject parentObject)
        {
            var allChildObjects = parentObject.GetComponentsInChildren(typeof(T));
            var instanceID = 0;

            var uniqueName = baseName + " " + allChildObjects.Length;

            while (parentObject.transform.Find(uniqueName))
            {
                instanceID++;
                uniqueName += " (" + instanceID + ")";
            }

            return uniqueName;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Place the given object in the scene view in front of the camera.
        /// </summary>
        /// <param name="objectTransform">Object to be placed in scene view.</param>
        public static void PosToSceneView(Transform objectTransform)
        {
            var sceneCamTransform = SceneView.lastActiveSceneView.camera.transform;
            objectTransform.position = sceneCamTransform.position + (spawnDistanceZ * sceneCamTransform.forward);
            objectTransform.forward = objectTransform.position - sceneCamTransform.position;
            objectTransform.localEulerAngles = new Vector3(0, objectTransform.localEulerAngles.y, 0);
        }
#endif
    }
}
