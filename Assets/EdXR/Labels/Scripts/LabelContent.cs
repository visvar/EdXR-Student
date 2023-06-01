using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EdXR.Labels
{
    /// <summary>
    /// This is like the <ref>LabelContent</ref> class, but it lets you add and arrange UI elements dynamically.
    /// </summary>
    public class LabelContent : GenericContent
    {
        [SerializeField] protected int width = 70;
        [SerializeField] protected int fontSize = 32;
        [SerializeField] protected float borderWidth = 0.01f;
        [SerializeField] protected List<LabelElement> elements = new List<LabelElement>();

        /// <summary>
        /// Gets the standard font size for this label.
        /// </summary>
        public int FontSize => fontSize;

        /// <summary>
        /// Gets the border width of this label.
        /// </summary>
        public float BorderWidth => borderWidth;

        /// <summary>
        /// Gets the list of UI elements.
        /// </summary>
        public List<LabelElement> Elements => elements;

        /// <inheritdoc/>
        protected override void UpdateContent()
        {
            InitContent();
            var labelVisuals = Visuals as LabelVisuals;
            labelVisuals?.UpdateVisuals();
        }

        protected override void InitContent()
        {
            base.InitContent();
        }
    }
}