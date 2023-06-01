// <copyright file="Visuals.cs" company="Universität Stuttgart">
// Copyright (c) Universität Stuttgart. All rights reserved.
// </copyright>

namespace EdXR
{
    using UnityEngine;

    /// <summary>
    /// Abstract class for visual representations (GameObject) in AR and VR. These hold all references to different parts of the visuals (text-fields, UI, etc.).
    /// </summary>
    public abstract class Visuals : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the content associated to this visual representation.
        /// </summary>
        public Content Content { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this visuals had been initialized.
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// Gets or sets point scale for Points to Meter ratio for standard text font size of 13.
        /// </summary>
        public float PointScale { get; set; } = 0.0046f;

        /// <summary>
        /// Initialize the visuals.
        /// </summary>
        /// <param name="content">The content that belongs to this visual.</param>
        public abstract void InitVisuals(Content content);
        
        /// <summary>
        /// Update the visuals according to the content.
        /// </summary>
        public abstract void UpdateVisuals();
    }
}