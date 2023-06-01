using System;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

namespace EdXR.Labels
{
    /// <summary>
    /// Element that makes up the label. Can be a headline, text, image etc. (ElementType).
    /// </summary>
    [Serializable]
    public struct LabelElement
    {
        /// <summary>
        /// The type of this element.
        /// </summary>
        public ElementType Type;

        /// <summary>
        /// The text for the headline.
        /// </summary>
        public string Headline;

        /// <summary>
        /// The text for the text body.
        /// </summary>
        [TextArea] public string TextMain;

        /// <summary>
        /// The text for the text body 2 (right text column in two columns layout).
        /// </summary>
        [TextArea] public string TextSecondary;

        /// <summary>
        /// The reference to the image source.
        /// </summary>
        public Texture2D ImageMain;

        /// <summary>
        /// The reference to the image source of image 2 (right image in two columns image layout).
        /// </summary>
        public Texture2D ImageSecondary;

        /// <summary>
        /// The text for the image description.
        /// </summary>
        [TextArea] public string ImageDescriptionMain;

        /// <summary>
        /// The text for the image description of image 2 (right image in two columns image layout).
        /// </summary>
        [TextArea] public string ImageDescriptionSecondary;

        /// <summary>
        /// Image style
        /// </summary>
        public ColumnsStyle ColumnsStyle;

        /// <summary>
        /// Set the ratio of the columns automatically.
        /// </summary>
        public bool AutoColumnsRatio;

        /// <summary>
        /// Ratio between image and text column width;
        /// </summary>
        [Range(0.25f, 0.75f)] public float ColumnsRatio;

        /// <summary>
        /// Text alignment options for headline/text.
        /// </summary>
        public TextAlignmentOptions TextAlignment;

        /// <summary>
        /// The style of the headline.
        /// </summary>
        public HeadlineStyle HeadlineStyle;

        /// <summary>
        /// Space of seperator element (times gap distance).
        /// </summary>
        public float SeperatorSpace;

        /// <summary>
        /// Show a seperator line if true.
        /// </summary>
        public bool ShowSeperatorLine;

        /// <summary>
        /// Click Event for button interaction.
        /// </summary>
        public ButtonClickEvent OnButtonClick;

        /// <summary>
        /// Video clip.
        /// </summary>
        public VideoClip VideoClip;

        /// <summary>
        /// Set true to play video automatically and loop.
        /// </summary>
        public bool AutoPlayAndLoop;
    }
}