// <copyright file="Content.cs" company="Universität Stuttgart">
// Copyright (c) Universität Stuttgart. All rights reserved.
// </copyright>

namespace EdXR
{
    using UnityEngine;

    /// <summary>
    /// General content class used by visuals.
    /// </summary>
    [DisallowMultipleComponent, SelectionBase, ExecuteInEditMode]
    public abstract class Content : MonoBehaviour
    {
        [SerializeField] private GameObject template;
        [HideInInspector][SerializeField] private Transform visualsContainer;
        [HideInInspector][SerializeField] private Visuals visuals;

        /// <summary>
        /// Sets a new visuals template for this content.
        /// </summary>
        /// <param name="template">Template prefab from assets.</param>
        public void SetTemplate(GameObject template)
        {
            this.template = template;
            InitContent();
        }

        /// <summary>
        /// Gets the prefab reference that will be instantiated for this content.
        /// </summary>
        protected GameObject Template => template;

        /// <summary>
        /// Gets or sets the parent object of the visuals.
        /// </summary>
        protected Transform VisualsContainer
        {
            get => visualsContainer;
            set => visualsContainer = value;
        }

        /// <summary>
        /// Gets or sets the visuals of the instantiated prefab.
        /// </summary>
        protected Visuals Visuals
        {
            get => visuals;
            set => visuals = value;
        }

        /// <summary>
        /// Initialize this content.
        /// </summary>
        protected abstract void InitContent();

        /// <summary>
        /// Update the visuals prefab of this content.
        /// </summary>
        /// <param name="updateVisuals">Should visuals be updated?</param>
        protected abstract void UpdateContent();
    }
}