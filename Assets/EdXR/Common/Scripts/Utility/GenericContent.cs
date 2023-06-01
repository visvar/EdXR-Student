namespace EdXR
{
    using UnityEngine;

    /// <summary>
    /// Generic class for content. All content types should inherit from the generic class.
    /// </summary>
    [ExecuteInEditMode]
    public class GenericContent : Content
    {
#if UNITY_EDITOR
        private bool inspectorEdited = false;
#endif

        /// <inheritdoc/>
        protected override void InitContent()
        {
            // Don't do anything when there is no template prefab to instantiate.
            if (Template == null)
            {
                return;
            }

            // If there is no visuals parent yet, create game object and make it a child of this content object.
            if (VisualsContainer == null)
            {
                var go = new GameObject(gameObject.name + " " + "[Visuals]");
                go.hideFlags |= HideFlags.HideInHierarchy | HideFlags.DontSave;
                VisualsContainer = go.transform;
                VisualsContainer.SetParent(transform, false);
            }

            // If there is no visuals prefab, instantiate prefab.
            if (Visuals == null)
            {
                var prefab = Instantiate(Template, VisualsContainer, false);
                Visuals = prefab.GetComponent<Visuals>();

                if (Visuals == null)
                {
                    Debug.LogWarning("Prefab for [" + gameObject.name + "] content does not contain a Visuals script. Please use correct prefab.");
                }
            }

            // If visuals prefab had been successfully instantiated, initialize visuals.
            if (Visuals != null && !Visuals.Initialized)
            {
                Visuals.InitVisuals(this);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateContent()
        {
            InitContent();
            Visuals?.UpdateVisuals();
        }

        private void Start()
        {
            this.UpdateContent();
        }

#if UNITY_EDITOR
        /// <summary>
        /// When in the editor, OnValidate gets called when inspector values have changed.
        /// </summary>
        private void OnValidate()
        {
            inspectorEdited = true;
        }

        /// <summary>
        /// When inspector values have changed or the visuals had been deleted, update the visuals.
        /// </summary>
        private void Update()
        {
            if (inspectorEdited || Visuals == null || VisualsContainer == null)
            {
                UpdateContent();
                inspectorEdited = false;
            }
        }
#endif
    }
}
