// <copyright file="LabelVisuals.cs" company="Universität Stuttgart">
// Copyright (c) Universität Stuttgart. All rights reserved.
// </copyright>

using System.Collections.Generic;
using UnityEngine;

namespace EdXR.Labels
{
    /// <summary>
    /// Visual representation (prefab) of LabelContent. The visuals adjust dynamically to the content.
    /// </summary>
    public class LabelVisuals : GenericVisuals
    {
        private readonly float minScaleX = 0.1f;
        [HideInInspector] [SerializeField] private RectTransform elementsParent;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Transform panelBackground;
        [SerializeField] private Transform panelBorder;
        [SerializeField] private GameObject elementPrefab;
        [SerializeField] [Range(0, 0.16f)] private float verticalSpacing = 0.08f;
        [SerializeField] private float padding = 0.3f;
        [SerializeField] private float minTextWidth = 20;

        private float verticalGap;
        private float paddingX;
        private float paddingY;

        private LabelContent labelContent;
        private float width;
        private List<LabelElementVisuals> elementVisuals = new List<LabelElementVisuals>();
        private Vector3 tempScale = Vector3.one;

        /// <summary>
        /// Gets the width (in pt.) of this label.
        /// </summary>
        public float Width => width;

        /// <summary>
        /// Gets the vertical spacing.
        /// </summary>
        public float VerticalGap => verticalGap;

        /// <inheritdoc/>
        public override void InitVisuals(Content content)
        {
            base.InitVisuals(content);
            labelContent = content as LabelContent;
            elementPrefab.SetActive(false);
            ResetElements();
        }

        /// <inheritdoc/>
        public override void UpdateVisuals()
        {
            if (!Initialized)
            {
                return;
            }

            Content.transform.localScale = new Vector3(Content.transform.localScale.x > 0.1f ? Content.transform.localScale.x : minScaleX, Content.transform.localScale.y, Content.transform.localScale.z);

            // Set panel scale. Override transform scale so that visuals stay unaffected. Instead, the scale influences the width.
            var panelUniformScaleX = transform.localScale.x / transform.lossyScale.x;
            var panelUniformScaleY = transform.localScale.y / transform.lossyScale.y;
            var panelUniformScaleZ = transform.localScale.z / transform.lossyScale.z;
            panel.localScale = PointScale * new Vector3(panelUniformScaleX, panelUniformScaleY, panelUniformScaleZ);

            // Padding and spacing relative to font size.
            minTextWidth = 0.5f * labelContent.FontSize;
            verticalGap = verticalSpacing * labelContent.FontSize;
            paddingX = padding * labelContent.FontSize;
            paddingY = 0.7f * paddingX;

            // Set text field width to panel width.
            width = transform.lossyScale.x * 1 / PointScale;
            width = width > minTextWidth ? width : minTextWidth;

            var elements = labelContent.Elements;

            ResetElements();

            var elementPosY = 0f;

            for (var i = 0; i < elements.Count; i++)
            {
                var visual = Instantiate(elementPrefab, elementsParent).GetComponent<LabelElementVisuals>();
                visual.gameObject.SetActive(true);
                visual.InitElement(elements[i], labelContent, this);
                elementVisuals.Add(visual);

                var offsetY = 0f;

                // No margin at top for the first element.
                if (i != 0)
                {
                    offsetY = visual.MarginTop;
                }

                visual.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, elementPosY - (0.5f * visual.Height) - offsetY, 0);
                elementPosY -= visual.Height + offsetY;

                // No bottom margin for last element.
                if (i != elements.Count - 1)
                {
                    elementPosY -= visual.MarginBottom;
                }
            }

            var panelHeight = -elementPosY;

            panel.sizeDelta = new Vector2(width, panelHeight);
            elementsParent.sizeDelta = new Vector2(width, panelHeight);
            panelBackground.localScale = new Vector3(panel.sizeDelta.x + paddingX, panelHeight + paddingY, 100);

            // The border has always the same width in world space. So we take the lossyScale and add borderWidth to get the aspectRatio of border size to panel size.
            var borderScaleX = (panelBackground.lossyScale.x + labelContent.BorderWidth) / panelBackground.lossyScale.x;
            var borderScaleY = (panelBackground.lossyScale.y + labelContent.BorderWidth) / panelBackground.lossyScale.y;

            panelBorder.localScale = new Vector3(panelBackground.localScale.x * borderScaleX, panelBackground.localScale.y * borderScaleY, 1);

            // Lock hierarchy for interaction.
            LockObject(gameObject, true);
        }

        private void ResetElements()
        {
            // If there already is a parent (possibly including elemens), delete it.
            if (elementsParent != null)
            {
                DestroyImmediate(elementsParent.gameObject);
            }

            // Create parent
            var elementParentObj = new GameObject("Elements Parent");
            elementsParent = elementParentObj.AddComponent<RectTransform>();
            elementsParent.SetParent(panel, false);

            // Clear elements list.
            elementVisuals.Clear();
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Update visuals if transform has changed.
            if (transform.lossyScale != tempScale)
            {
                UpdateVisuals();
                tempScale = transform.lossyScale;
            }
        }
#endif
    }
}
