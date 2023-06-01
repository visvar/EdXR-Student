namespace EdXR
{
    using UnityEngine;

    /// <summary>
    /// Generic class for visual representations (GameObject) in AR and VR. These hold all references to different parts of the visuals (text-fields, UI, etc.).
    /// All visuals should inherit from the generic class.
    /// </summary>
    [ExecuteInEditMode]
    public class GenericVisuals : Visuals
    {
        /// <inheritdoc/>
        public override void InitVisuals(Content content)
        {
            if (content == null)
            {
                return;
            }

            this.Content = content;
            Initialized = true;
        }

        /// <inheritdoc/>
        public override void UpdateVisuals()
        {
        }

        /// <summary>
        /// Iterate through all children and set the hide flag "not editable" to lock interaction.
        /// </summary>
        /// <param name="targetObject">Parent object.</param>
        /// <param name="recursive">Iterate through child objects recursively.</param>
        protected void LockObject(GameObject targetObject, bool recursive)
        {
            targetObject.hideFlags |= HideFlags.HideInInspector| HideFlags.NotEditable | HideFlags.DontSave;

            if (recursive)
            {
                foreach (Transform child in targetObject.transform)
                {
                    LockObject(child.gameObject, true);
                }
            }
        }
    }
}
