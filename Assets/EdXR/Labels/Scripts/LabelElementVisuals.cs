// <copyright file="LabelVisuals.cs" company="Universität Stuttgart">
// Copyright (c) Universität Stuttgart. All rights reserved.
// </copyright>

namespace EdXR.Labels
{
    using Microsoft.MixedReality.Toolkit.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Video;

    /// <summary>
    /// Visual representation of an element of LabelContent. The visuals adjust dynamically to the content.
    /// </summary>
    public class LabelElementVisuals : MonoBehaviour
    {
        private readonly float seperatorThickness = 4;

        [SerializeField] private float buttonWidth = 20;
        [SerializeField] private GameObject buttonVideoPlay;
        [SerializeField] private Interactable button;
        [SerializeField] private MeshRenderer imageQuadMain;
        [SerializeField] private MeshRenderer imageQuadSecondary;
        [SerializeField] private RectTransform seperator;
        [SerializeField] private TextMeshPro textMain;
        [SerializeField] private TextMeshPro headline;
        [SerializeField] private TextMeshPro imageDescriptionMain;
        [SerializeField] private TextMeshPro imageDescriptionSecondary;
        [SerializeField] private TextMeshPro textSecondary;
        [SerializeField] private VideoPlayer videoPlayer;

        private bool initialized = false;
        private ColumnsStyle colStyle;
        private ElementType type;

        private float colRatio;
        private float height;
        private float marginBottom;
        private float marginTop;
        private float verticalGap = 10;
        private float width;

        // private WaitForSeconds CheckVideoWait = new WaitForSeconds(0.1f);
        private RectTransform rectTransform;
        private Texture2D imageMain;
        private Texture2D imageSecondary;

        /// <summary>
        /// Gets the height value of this visual element.
        /// </summary>
        public float Height => height;

        /// <summary>
        /// Gets the bottom margin.
        /// </summary>
        public float MarginBottom => marginBottom;

        /// <summary>
        /// Gets the top margin of this element.
        /// </summary>
        public float MarginTop => marginTop;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelElementVisuals"/> class.
        /// </summary>
        /// <param name="element">LabelElement of this visualization.</param>
        /// <param name="labelContent">LabelContent of this visualization.</param>
        /// <param name="labelVisuals">LabelVisuals of this visualization.</param>
        public void InitElement(LabelElement element, LabelContent labelContent, LabelVisuals labelVisuals)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            rectTransform = GetComponent<RectTransform>();

            // Get content from element.
            colStyle = element.ColumnsStyle;
            colRatio = element.ColumnsRatio;
            type = element.Type;

            // Set texts.
            headline.SetText(element.Headline);
            textMain.SetText(element.TextMain);
            textSecondary.SetText(element.TextSecondary);
            imageDescriptionMain.SetText(element.ImageDescriptionMain);
            imageDescriptionSecondary.SetText(element.ImageDescriptionSecondary);

            // Set text alignments.
            headline.alignment = element.TextAlignment;

            if (element.Type == ElementType.TwoColumns)
            {
                textMain.alignment = element.ColumnsStyle == ColumnsStyle.SingleText && !string.IsNullOrEmpty(textSecondary.text) ? TextAlignmentOptions.TopFlush : TextAlignmentOptions.TopJustified;
                textSecondary.alignment = TextAlignmentOptions.TopJustified;
            }
            else
            {
                textMain.alignment = element.TextAlignment;
                textSecondary.alignment = element.TextAlignment;
            }

            // Set font sizes.
            headline.fontSize = (element.HeadlineStyle == HeadlineStyle.Title ? 2f : 1f) * labelContent.FontSize;
            headline.fontStyle = element.HeadlineStyle == HeadlineStyle.Title ? FontStyles.Normal : FontStyles.Bold;
            textMain.fontSize = labelContent.FontSize;
            textSecondary.fontSize = labelContent.FontSize;
            imageDescriptionMain.fontSize = labelContent.FontSize;
            imageDescriptionSecondary.fontSize = labelContent.FontSize;

            // Set images.
            imageMain = element.ImageMain;
            imageSecondary = element.ImageSecondary;

            // Set video properties.
            videoPlayer.clip = element.VideoClip;
            videoPlayer.playOnAwake = element.AutoPlayAndLoop;
            videoPlayer.isLooping = element.AutoPlayAndLoop;
            videoPlayer.loopPointReached += VideoPlayer_LoopPointReached;

            // Set button on click function.
            button.OnClick = element.OnButtonClick;

            // Validate user input.
            var textMainValid = !string.IsNullOrWhiteSpace(textMain.text);
            var textSecondaryValid = !string.IsNullOrWhiteSpace(textSecondary.text);
            var headlineValid = !string.IsNullOrWhiteSpace(headline.text);
            var imageMainValid = imageMain != null && imageMain is object;
            var videoClipValid = element.VideoClip != null && element.VideoClip is object;
            var imageSecondaryValid = imageSecondary != null && imageSecondary is object;
            var imageDescriptionMainValid = !string.IsNullOrWhiteSpace(imageDescriptionMain.text);
            var imageDescriptionSecondaryValid = !string.IsNullOrWhiteSpace(imageDescriptionSecondary.text);

            // Activate UI elements according to type.
            var showHeadline = headlineValid && (type == ElementType.Headline || type == ElementType.Button);
            headline.gameObject.SetActive(showHeadline);

            var showTextMain = textMainValid && (type == ElementType.Text || type == ElementType.Button || (type == ElementType.TwoColumns && colStyle != ColumnsStyle.TwoImages));
            textMain.gameObject.SetActive(showTextMain);

            var showTextSecondary = (textMainValid && type == ElementType.TwoColumns && colStyle == ColumnsStyle.SingleText) || (textSecondaryValid && type == ElementType.TwoColumns && colStyle == ColumnsStyle.TwoTexts);
            textSecondary.gameObject.SetActive(showTextSecondary);

            var showSeperator = type == ElementType.Seperator && element.ShowSeperatorLine;
            seperator.gameObject.SetActive(showSeperator);

            var showVideo = videoClipValid && type == ElementType.Video;
            videoPlayer.enabled = videoClipValid && type == ElementType.Video;
            buttonVideoPlay.SetActive(showVideo && !videoPlayer.playOnAwake);

            var showImageMain = (imageMainValid && (type == ElementType.Image || (type == ElementType.TwoColumns && colStyle != ColumnsStyle.SingleText && colStyle != ColumnsStyle.TwoTexts))) || showVideo;
            imageQuadMain.gameObject.SetActive(showImageMain);
            imageDescriptionMain.gameObject.SetActive(imageDescriptionMainValid && showImageMain);

            var showImageSecondary = imageSecondaryValid && type == ElementType.TwoColumns && colStyle == ColumnsStyle.TwoImages;
            imageQuadSecondary.gameObject.SetActive(showImageSecondary);
            imageDescriptionSecondary.gameObject.SetActive(imageDescriptionSecondaryValid && showImageSecondary);

            button.gameObject.SetActive(type == ElementType.Button);

            // Create own instance of material for main image.
            if (showImageMain)
            {
                imageQuadMain.sharedMaterial = new Material(imageQuadMain.sharedMaterial);
                if (imageMainValid)
                {
                    imageQuadMain.sharedMaterial.mainTexture = imageMain;
                }

                if (videoClipValid)
                {
                    // Play video so that the first frame loads.
                    videoPlayer.Play();

                    // Pause video unless we're in play mode and the video is supposed to play on awake.
                    if (!videoPlayer.playOnAwake || !Application.isPlaying)
                    {
                        videoPlayer.Pause();
                    }

                    videoPlayer.frame = 0;
                }
            }

            // Create own instance of material for secondary image.
            if (showImageSecondary)
            {
                imageQuadSecondary.sharedMaterial = new Material(imageQuadSecondary.sharedMaterial)
                {
                    mainTexture = imageSecondary
                };
            }

            // Initialize dimensions.
            width = labelVisuals.Width;
            verticalGap = labelVisuals.VerticalGap;
            marginTop = 0;

            // Initialize local variables.
            var headlineAlignment = 0f;
            var horizontalGap = 2 * verticalGap;
            var smallGap = 0.3f * verticalGap;

            var imageDescriptionMainHeight = 0f;
            var imageDescriptionSecondaryHeight = 0f;

            var imageMainContainerHeight = 0f;
            var imageMainWidth = 0f;
            var imageMainHeight = 0f;
            var imageMainPosX = 0f;

            var imageSecondaryContainerHeight = 0f;
            var imageSecondaryWidth = 0f;
            var imageSecondaryHeight = 0f;

            var textMainWidth = 0f;
            var textMainPosX = 0f;
            var textMainHeight = 0f;

            var textSecondaryWidth = 0f;
            var textSecondaryPosX = 0f;
            var textSecondaryHeight = 0f;

            // The min and max values are the same as in LabelElement class ColumnsRatio range.
            const float colRatioMin = 0.25f;
            const float colRatioMax = 0.75f;

            // The button container width accounts for the left margin of the button.
            var buttonContainerWidth = buttonWidth + horizontalGap;

            // We reuse the right column position later.
            var rightColPosX = 0f;

            // Set headline width to panel width.
            var headlineWidth = width;
            var headlinePosX = 0f;

            // If the element type is "button", we display the headline next to the button, so we adjust headline width/position to make room for the button.
            if (type == ElementType.Button)
            {
                headlineWidth -= buttonContainerWidth;
                headlinePosX -= 0.5f * buttonContainerWidth;
            }

            // Set width of headline.
            headline.rectTransform.sizeDelta = new Vector2(headlineWidth, 0);

            // Set the dimensions of main content for one and two col layout.
            if (type != ElementType.TwoColumns)
            {
                // In one column layout, there is only one text or one image and they take up the full width.
                imageMainWidth = width;
                imageSecondaryWidth = 0;
                textMainWidth = width;
                textSecondaryWidth = 0;

                // Position is right in the middle.
                textMainPosX = 0;
                imageMainPosX = 0;

                // If the element type is "button", main text will be smaller to make space for the button.
                if (type == ElementType.Button)
                {
                    textMainWidth -= buttonContainerWidth;
                    textMainPosX -= 0.5f * buttonContainerWidth;
                }
            }
            else
            {
                // Initialize temporary col ratio. 0 means that the left column has 0% width and right column gets 100% width.
                // Later we will clip this to a range to make sure that both col will always be visible.
                var tempColRatio = 0f;

                // "one text" mode col will always be split right in the middle.
                if (colStyle == ColumnsStyle.SingleText)
                {
                    tempColRatio = 0.5f;
                }

                // If there are two columns, in "auto ratio mode" we want to iterate through different ratios to find the best fit for the dynamic content.
                // The shortcut being: If there are two images and no description texts, we can just go for setting the images to equal height and no further iterations required.
                if (colStyle == ColumnsStyle.TwoImages && element.AutoColumnsRatio && imageMainValid && imageSecondaryValid)
                {
                    // Calculate the image's aspect ratios.
                    var aspectRatioImageMain = (float)imageMain.width / imageMain.height;
                    var aspectRatioImageSecondary = (float)imageSecondary.width / imageSecondary.height;

                    if (!imageDescriptionMainValid && !imageDescriptionSecondaryValid)
                    {
                        // Calculate images height to be equal for the given aspect ratios and total width.
                        var imagesHeight = (width - horizontalGap) / (aspectRatioImageMain + aspectRatioImageSecondary);

                        // Get columns ratio from main image aspect ratio and height.
                        tempColRatio = aspectRatioImageMain * imagesHeight / (width - horizontalGap);
                    }
                    else
                    {
                        // If the description texts are not empty, they are dynamic in size and need to be taken into consideration.
                        var descrMainHeight = 0f;
                        var descrSecondaryHeight = 0f;
                        var smallestColHeightDelta = 0f;

                        // We iterate through vaious columns rations and get back the delta in height from the elements.
                        // We find the descritption text heights for the columns ratio that has the smalles delta.
                        // Knowing the description text heights, image aspect ratios and total width, we can calculate the exact columns ratio for the perfect fit.
                        for (var i = colRatioMin; i < colRatioMax; i += 0.01f)
                        {
                            var tempLeftColWidth = (this.width - horizontalGap) * i;
                            var tempRightColWidth = (this.width - horizontalGap) * (1 - i);

                            // Set image description texts to temporary with.
                            imageDescriptionMain.rectTransform.sizeDelta = new Vector2(tempLeftColWidth, 0);
                            imageDescriptionSecondary.rectTransform.sizeDelta = new Vector2(tempRightColWidth, 0);

                            // Get the preferred height of the texts.
                            var tempDescrMainHeight = Mathf.Max(0, imageDescriptionMain.GetPreferredValues().y);
                            var tempDescrSecondaryHeight = Mathf.Max(0, imageDescriptionSecondary.GetPreferredValues().y);

                            // Calculate total heights.
                            var tempLeftColHeight = (tempLeftColWidth / aspectRatioImageMain) + tempDescrMainHeight;
                            var tempRightColHeight = (tempRightColWidth / aspectRatioImageSecondary) + tempDescrSecondaryHeight;

                            // Get delta in column heights.
                            var colHeightsDelta = Mathf.Abs(tempLeftColHeight - tempRightColHeight);

                            // Updating the smallest heights delta and inherent description texts hights as we iterate through the different ratios.
                            if (i == colRatioMin || colHeightsDelta < smallestColHeightDelta)
                            {
                                smallestColHeightDelta = colHeightsDelta;
                                descrMainHeight = tempDescrMainHeight;
                                descrSecondaryHeight = tempDescrSecondaryHeight;
                            }
                        }

                        // Mathematical magic for getting the ratio from dynamic left column width.
                        // This is possible because now we know the ideal description text heights, aspect ratios, total width and gap width.
                        // We also know that left column height and right column height need to be the same and that the right column width can be calculated from left column with, total width and gap width.
                        // Like this we can calculate the only left unknown, which is the left column width, and get the ideal ratio.
                        tempColRatio = (width - horizontalGap + (aspectRatioImageSecondary * descrSecondaryHeight) - (aspectRatioImageSecondary * descrMainHeight)) / ((aspectRatioImageSecondary / aspectRatioImageMain) + 1) / (width - horizontalGap);
                    }
                }

                // For every other mode we use the custom ratio provided by the user.
                if (tempColRatio == 0)
                {
                    tempColRatio = colRatio;
                }

                // Clamp ratio so that both columns can be visible.
                tempColRatio = Mathf.Clamp(tempColRatio, colRatioMin, colRatioMax);

                // Get columns dimensions and positions.
                var leftColWidth = (width - horizontalGap) * tempColRatio;
                var rightColWidth = (width - horizontalGap) * (1 - tempColRatio);

                var centerX = 0.5f * width;
                var leftColPosX = (0.5f * leftColWidth) - centerX;
                rightColPosX = centerX - (0.5f * rightColWidth);

                // Depending on style, we put the main/secondary texts and images in different columns.
                switch (colStyle)
                {
                    case ColumnsStyle.SingleText:
                    case ColumnsStyle.TwoTexts:
                        textMainPosX = leftColPosX;
                        textMainWidth = leftColWidth;
                        textSecondaryPosX = rightColPosX;
                        textSecondaryWidth = rightColWidth;
                        break;
                    case ColumnsStyle.ImageRight:
                        textMainPosX = leftColPosX;
                        imageMainPosX = rightColPosX;
                        imageMainWidth = rightColWidth;
                        textMainWidth = leftColWidth;
                        break;
                    case ColumnsStyle.ImageLeft:
                        imageMainPosX = leftColPosX;
                        textMainPosX = rightColPosX;
                        imageMainWidth = leftColWidth;
                        textMainWidth = rightColWidth;
                        break;
                    case ColumnsStyle.TwoImages:
                        imageMainPosX = leftColPosX;
                        imageMainWidth = leftColWidth;
                        imageSecondaryWidth = rightColWidth;
                        textMainWidth = 0;
                        break;
                    default:
                        textMainPosX = 0;
                        imageMainPosX = 0;
                        break;
                }
            }

            // For images, we calculate the main image's height from the width and its original aspect ratio.
            if (type != ElementType.Video && showImageMain)
            {
                imageMainHeight = imageMainWidth * imageMain.height / imageMain.width;
            }

            // We render videos in the main image texture. So for videos, we calculate the main image's height from the width and the video's aspect ratio.
            if (showVideo && showImageMain)
            {
                imageMainHeight = imageMainWidth * videoPlayer.clip.height / videoPlayer.clip.width;
            }

            // Set images' scale.
            if (showImageMain)
            {
                imageQuadMain.transform.localScale = new Vector3(imageMainWidth, imageMainHeight, 1);

                // We assign the images' widths to the description texts so that we can get the required height of the texts.
                imageDescriptionMain.rectTransform.sizeDelta = new Vector2(imageMainWidth, 0);

                // Get image descripttions' heights.
                imageDescriptionMainHeight = !imageDescriptionMainValid || !showImageMain ? 0 : imageDescriptionMain.GetPreferredValues().y;

                // Calculate overall height of the image containers including image, description text and gap there inbetween (if applicable).
                imageMainContainerHeight = imageMainHeight + (imageDescriptionMainHeight > 0 ? smallGap : 0) + imageDescriptionMainHeight;
            }

            if (showImageSecondary)
            {
                // We calculate the secondary image's height from the width and its original aspect ratio.
                imageSecondaryHeight = imageSecondaryWidth * imageSecondary.height / imageSecondary.width;

                // This is like above for main image.
                imageQuadSecondary.transform.localScale = new Vector3(imageSecondaryWidth, imageSecondaryHeight, 1);
                imageDescriptionSecondary.rectTransform.sizeDelta = new Vector2(imageSecondaryWidth, 0);
                imageDescriptionSecondaryHeight = !imageDescriptionSecondaryValid || !showImageSecondary ? 0 : imageDescriptionSecondary.GetPreferredValues().y;
                imageSecondaryContainerHeight = imageSecondaryHeight + (imageDescriptionSecondaryHeight > 0 ? smallGap : 0) + imageDescriptionSecondaryHeight;
            }

            // Initialize main text.
            if (showTextMain)
            {
                textMain.rectTransform.sizeDelta = new Vector2(textMainWidth, 0);

                // Reset main text to not be linked and use overflow mode, so that we can get the total number of lines required to display the text.
                textMain.overflowMode = TextOverflowModes.Overflow;
                textMain.linkedTextComponent = null;
            }

            // Initialize secondary text.
            if (showTextSecondary)
            {
                textSecondary.rectTransform.sizeDelta = new Vector2(textSecondaryWidth, 0);
            }

            // Set text heights for the single text in two columns.
            if (type == ElementType.TwoColumns && colStyle == ColumnsStyle.SingleText)
            {
                // Force update so that we can get the total number of lines required to display the text.
                textMain.ForceMeshUpdate();

                if (textMain.textInfo.lineCount > 0)
                {
                    // Since now we know the total number of lines of the text content, the left and right column text fields need to display half of the lines each.
                    // The columns line count gets rounded up, so that in the case of uneven line count the right column will show an empty line at the end.
                    var colLineCount = Mathf.Ceil(0.5f * textMain.textInfo.lineCount);

                    // Calculate line height and use it to get the height of main/secondary texts.
                    var lineHeight = textMain.textBounds.size.y / textMain.textInfo.lineCount;
                    textMainHeight = lineHeight * colLineCount;
                    textSecondaryHeight = textMainHeight;
                }

                // Set the main text to be linked to the secondary text.
                textMain.overflowMode = TextOverflowModes.Linked;
                textMain.linkedTextComponent = textSecondary;
            }
            else
            {
                // If not single text in two columns, main text gets set to required height if not empty or inactive. Otherwise will be 0.
                if (showTextMain)
                {
                    textMainHeight = textMain.GetPreferredValues().y;
                }

                // If the element type is "button" and main text height is smaller than the button, it gets vertically centered by getting same height as button.
                if (type == ElementType.Button && textMainHeight < buttonWidth && !headlineValid)
                {
                    textMainHeight = buttonWidth;
                }

                // Set secondary text to required height if not empty or inactive. Otherwise will be 0.
                if (showTextSecondary)
                {
                    textSecondaryHeight = textSecondary.GetPreferredValues().y;
                }
            }

            // Set main text height and width.
            if (showTextMain)
            {
                textMain.rectTransform.sizeDelta = new Vector2(textMainWidth, textMainHeight);
            }

            // Set secondary text height and width.
            if (showTextSecondary)
            {
                textSecondary.rectTransform.sizeDelta = new Vector2(textSecondaryWidth, textSecondaryHeight);
            }

            // Set top margin for headlines and buttons.
            if (type == ElementType.Headline && element.HeadlineStyle == HeadlineStyle.Title)
            {
                marginTop += 10 * headline.fontSize * labelVisuals.PointScale;
            }
            else
            {
                if (type == ElementType.Button)
                {
                    marginTop += verticalGap;
                }
            }

            // If there is no text in the texfield, height is 0. Otherwise take calculated height.
            var headlineHeight = showHeadline ? headline.GetPreferredValues().y : 0;

            // Margin Bottom is smaller for subheadlines.
            if (element.Type == ElementType.Headline && element.HeadlineStyle == HeadlineStyle.Subheading)
            {
                marginBottom = smallGap;
            }
            else
            {
                marginBottom = verticalGap;
            }

            // Get height of columns area.
            if (type == ElementType.TwoColumns)
            {
                switch (colStyle)
                {
                    case ColumnsStyle.SingleText:
                        height = textMainHeight;
                        break;
                    case ColumnsStyle.TwoTexts:
                        height = Mathf.Max(textMainHeight, textSecondaryHeight);
                        break;
                    case ColumnsStyle.TwoImages:
                        height = Mathf.Max(imageMainContainerHeight, imageSecondaryContainerHeight);
                        break;
                    case ColumnsStyle.ImageLeft:
                        height = Mathf.Max(imageMainContainerHeight, textMainHeight);
                        break;
                    case ColumnsStyle.ImageRight:
                        height = Mathf.Max(imageMainContainerHeight, textMainHeight);
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case ElementType.Headline:
                        height = headlineHeight;
                        break;
                    case ElementType.Text:
                        height = textMainHeight;
                        break;
                    case ElementType.Image:
                    case ElementType.Video:
                        height = imageMainContainerHeight;
                        break;
                    case ElementType.Button:
                        if (headlineHeight > 0 && textMainHeight == 0 && headlineHeight < buttonWidth)
                        {
                            headlineHeight = buttonWidth;
                        }

                        height = Mathf.Max(headlineHeight + textMainHeight, buttonWidth);
                        break;
                    case ElementType.Seperator:
                        height = element.SeperatorSpace * verticalGap;
                        break;
                    default:
                        height = 0;
                        break;
                }
            }

            // Set Button position.
            if (type == ElementType.Button)
            {
                var buttonPosX = 0.5f * (width - buttonWidth);
                var buttonPosY = 0.5f * (height - buttonWidth);
                button.transform.localPosition = new Vector3(buttonPosX, buttonPosY, button.transform.localPosition.z);
            }

            // Calculate and apply headline offset.
            if (showHeadline)
            {
                // Set headline size.
                headline.rectTransform.sizeDelta = new Vector2(headlineWidth, headlineHeight);

                // Correcting headline alignment to be in aligned with smaller text: Either move to the left (-1) or the right (+1).
                if (element.HeadlineStyle == HeadlineStyle.Title)
                {
                    switch (headline.alignment)
                    {
                        case TextAlignmentOptions.Left:
                        case TextAlignmentOptions.BaselineLeft:
                        case TextAlignmentOptions.BottomLeft:
                        case TextAlignmentOptions.CaplineLeft:
                        case TextAlignmentOptions.MidlineLeft:
                        case TextAlignmentOptions.TopLeft:
                        case TextAlignmentOptions.Justified:
                        case TextAlignmentOptions.BaselineJustified:
                        case TextAlignmentOptions.BottomJustified:
                        case TextAlignmentOptions.CaplineJustified:
                        case TextAlignmentOptions.MidlineJustified:
                        case TextAlignmentOptions.TopJustified:
                        case TextAlignmentOptions.Flush:
                        case TextAlignmentOptions.BaselineFlush:
                        case TextAlignmentOptions.BottomFlush:
                        case TextAlignmentOptions.CaplineFlush:
                        case TextAlignmentOptions.MidlineFlush:
                        case TextAlignmentOptions.TopFlush:
                            headlineAlignment = -1f;
                            break;
                        case TextAlignmentOptions.Right:
                        case TextAlignmentOptions.BaselineRight:
                        case TextAlignmentOptions.BottomRight:
                        case TextAlignmentOptions.CaplineRight:
                        case TextAlignmentOptions.MidlineRight:
                        case TextAlignmentOptions.TopRight:
                            headlineAlignment = 1f;
                            break;
                        default:
                            headlineAlignment = 0f;
                            break;
                    }
                }

                var headlineOffsetX = headlineAlignment * headline.fontSize * labelVisuals.PointScale;
                headline.rectTransform.anchoredPosition = new Vector2(headlinePosX + headlineOffsetX, 0.5f * (height - headlineHeight));
            }

            // Calculate main text y-position and apply positions.
            if (showTextMain)
            {
                var textMainPosY = showHeadline ? headline.rectTransform.anchoredPosition.y - (0.5f * (headlineHeight + textMainHeight)) : 0;
                textMain.rectTransform.anchoredPosition = new Vector2(textMainPosX, textMainPosY);
            }

            // Set secondary text positions.
            if (showTextMain)
            {
                textSecondary.rectTransform.anchoredPosition = new Vector2(textSecondaryPosX, 0);
            }

            // Calculate image position.
            if (showImageMain || showImageSecondary)
            {
                var imageMainPosY = 0.5f * imageDescriptionMainHeight;
                var imageSecondaryPosY = 0.5f * imageDescriptionSecondaryHeight;

                // Put the image descriptions underneath the images.
                var imageDescriptionMainPosY = imageMainPosY - (0.5f * imageMainHeight) - (0.5f * imageDescriptionMainHeight) - smallGap;
                var imageDescriptionSecondaryPosY = imageSecondaryPosY - (0.5f * imageSecondaryHeight) - (0.5f * imageDescriptionSecondaryHeight) - smallGap;

                // Set image position.
                if (showImageMain)
                {
                    imageQuadMain.transform.localPosition = new Vector3(imageMainPosX, imageMainPosY, imageQuadMain.transform.localPosition.z);
                    imageDescriptionMain.rectTransform.sizeDelta = new Vector2(imageMainWidth, imageDescriptionMainHeight);
                    imageDescriptionMain.rectTransform.anchoredPosition = new Vector2(imageMainPosX, imageDescriptionMainPosY);

                    // Set the position of the video controls.
                    if (showVideo)
                    {
                        buttonVideoPlay.transform.localPosition = new Vector3(imageMainPosX, imageMainPosY, buttonVideoPlay.transform.localPosition.z);
                    }
                }

                if (showImageSecondary)
                {
                    imageQuadSecondary.transform.localPosition = new Vector3(rightColPosX, imageSecondaryPosY, imageQuadSecondary.transform.localPosition.z);
                    imageDescriptionSecondary.rectTransform.sizeDelta = new Vector2(imageSecondaryWidth, imageDescriptionSecondaryHeight);
                    imageDescriptionSecondary.rectTransform.anchoredPosition = new Vector2(rightColPosX, imageDescriptionSecondaryPosY);
                }
            }

            // Set seperator width.
            if (showSeperator)
            {
                seperator.localScale = new Vector3(width, seperatorThickness * labelContent.FontSize * labelVisuals.PointScale, seperator.localScale.z);
            }

            // Set overall width and height.
            rectTransform.sizeDelta = new Vector3(width, height, 1);
        }

        private void VideoPlayer_LoopPointReached(VideoPlayer player)
        {
            if (type == ElementType.Video)
            {
                player.frame = 0;
                buttonVideoPlay.SetActive(!videoPlayer.isLooping);
            }
        }
    }
}
