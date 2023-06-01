using EdXR.Labels;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Custom inspector gui for the LabelElement.
/// </summary>
[CustomEditor(typeof(LabelContent))]
public class LabelContentEditor : Editor
{
    private ReorderableList reorderableList;
    private SerializedProperty template;
    private SerializedProperty fontSize;
    private SerializedProperty borderWidth;
    private Transform targetTransform;
    private bool showAdvancedSettigns = false;
    private List<float> heights;
    private Color colElementBackground = new Color(0.07f, 0.39f, 0.7f, 0.1f);
    private Color colElementHeader = new Color(0.38f, 0.25f, 0.45f, 0.3f);
    private Color colLight = new Color(1, 1, 1, 0.1f);
    private Color colShadow = new Color(0, 0, 0, 0.25f);
    private Color colBox = new Color(1, 1, 1, 0.03f);
    private bool imageTextColumn = false;

    private float posX;
    private float posY;
    private float marginInner = 15;
    private float propertiesGap = 10;
    private float paddingBox = 5;
    private float boxWidth;
    private float innerWidth;
    private float offsetX = -5;
    private float elementGap = 10;

    // Get the serialized properties from the element class.
    private string propertyNameType = "Type";
    private string propertyNameHeadline = "Headline";
    private string propertyNameHeadlineStyle = "HeadlineStyle";
    private string propertyNameTextMain = "TextMain";
    private string propertyNameTextSecondary = "TextSecondary";
    private string propertyNameImageMain = "ImageMain";
    private string propertyNameImageSecondary = "ImageSecondary";
    private string propertyNameImageDescriptionMain = "ImageDescriptionMain";
    private string propertyNameImageDescriptionSecondary = "ImageDescriptionSecondary";
    private string propertyNameOnButtonClick = "OnButtonClick";
    private string propertyNameColumnsStyle = "ColumnsStyle";
    private string propertyNameColumnsRatio = "ColumnsRatio";
    private string propertyNameAutoColumnsRatio = "AutoColumnsRatio";
    private string propertyNameTextAlignment = "TextAlignment";
    private string propertyNameSeperatorSpace = "SeperatorSpace";
    private string propertyNameShowSeperatorLine = "ShowSeperatorLine";
    private string propertyNameVideoClip = "VideoClip";
    private string propertyNameAutoPlayAndLoop = "AutoPlayAndLoop";

    /// <inheritdoc/>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // The x-axis world scale controls the label size.
        // Therefore, if the world scale of the game object is less than 0, we show a message.
        if (targetTransform.lossyScale.x < 0)
        {
            EditorGUILayout.HelpBox("The X-axis world scale of this object is less than 0. Only values greater than 0 will affect label size.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("To change the width of the label, change the scale of the x-axis scale of the object transform.", MessageType.None);
        }

        // Reference to the template.
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(template);

        // Advanced Settings group foldout.
        showAdvancedSettigns = EditorGUILayout.Foldout(showAdvancedSettigns, "Advanced Settings", true, EditorStyles.foldout);

        if (showAdvancedSettigns)
        {
            // Font size.
            EditorGUILayout.PropertyField(fontSize);

            // Border width.
            EditorGUILayout.PropertyField(borderWidth);
        }

        // Clamp user input values.
        fontSize.intValue = Mathf.Clamp(fontSize.intValue, 16, 250);
        borderWidth.floatValue = Mathf.Clamp(borderWidth.floatValue, 0.01f, 0.1f);

        EditorGUILayout.Space();

        // Show warnings if the template field is empty ord does not contain a LabelVisuals component.
        if (template.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Template cannot be empty.", MessageType.Warning);
            EditorGUILayout.Space();
        }
        else
        {
            var templateObj = template.objectReferenceValue as GameObject;
            if (templateObj.GetComponent<LabelVisuals>() == null)
            {
                EditorGUILayout.HelpBox("Template prefab does not contain LabelVisuals component.", MessageType.Warning);
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space();

        // If the elements list is empty, show help text.
        if (reorderableList.count == 0)
        {
            EditorGUILayout.HelpBox("Add, remove and rearrange vertical elements (i.e. text, images, buttons, etc.) using the list below. Press the plus symbol to start adding elements.", MessageType.Info);
        }

        reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        var targetContent = (LabelContent)target;
        targetTransform = targetContent.transform;

        template = serializedObject.FindProperty("template");
        fontSize = serializedObject.FindProperty("fontSize");
        borderWidth = serializedObject.FindProperty("borderWidth");

        reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("elements"), true, true, true, true);
        heights = new List<float>(reorderableList.count);

        // This callback gets called when the header for the elements list gets drawn.
        reorderableList.drawHeaderCallback =
           (Rect rect) =>
           {
               EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Vertical UI building blocks");
           };

        // This callback gets called when an element gets removed from the list.
        reorderableList.onRemoveCallback =
            (ReorderableList list) =>
            {
                // Remove the element.
                list.serializedProperty.DeleteArrayElementAtIndex(list.index);

                // Since we have a new order of the elements list, we have to update the array of element heights.
                UpdateHeights();
            };

        // This callback gets called when the elements order get changed by the user.
        reorderableList.onReorderCallback =
            (ReorderableList list) =>
            {
                // Since we have a new order of the elements list, we have to update the array of element heights.
                UpdateHeights();
            };

        // The callback gets called whenever the elements get drawn (e.g. when they were updated).
        reorderableList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var labelContent = (LabelContent)target;

                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                // Get the serialized properties from the element class.
                var typeProperty = element.FindPropertyRelative(propertyNameType);
                var headlineProperty = element.FindPropertyRelative(propertyNameHeadline);
                var headlineStyleProperty = element.FindPropertyRelative(propertyNameHeadlineStyle);
                var textMainProperty = element.FindPropertyRelative(propertyNameTextMain);
                var textSecondaryProperty = element.FindPropertyRelative(propertyNameTextSecondary);
                var imageMainProperty = element.FindPropertyRelative(propertyNameImageMain);
                var imageSecondaryProperty = element.FindPropertyRelative(propertyNameImageSecondary);
                var imageDescriptionMainProperty = element.FindPropertyRelative(propertyNameImageDescriptionMain);
                var imageDescriptionSecondaryProperty = element.FindPropertyRelative(propertyNameImageDescriptionSecondary);
                var buttonClickProperty = element.FindPropertyRelative(propertyNameOnButtonClick);
                var columnsStyleProperty = element.FindPropertyRelative(propertyNameColumnsStyle);
                var columnsRatioProperty = element.FindPropertyRelative(propertyNameColumnsRatio);
                var autoColumnsRatioProperty = element.FindPropertyRelative(propertyNameAutoColumnsRatio);
                var textAlignmentProperty = element.FindPropertyRelative(propertyNameTextAlignment);
                var seperatorSpaceProperty = element.FindPropertyRelative(propertyNameSeperatorSpace);
                var showSeperatorLineProperty = element.FindPropertyRelative(propertyNameShowSeperatorLine);
                var videoClipProperty = element.FindPropertyRelative(propertyNameVideoClip);
                var autoPlayAndLoopProperty = element.FindPropertyRelative(propertyNameAutoPlayAndLoop);

                var columnsStyle = (ColumnsStyle)columnsStyleProperty.intValue;

                posX = rect.x;
                posY = rect.y;
                boxWidth = rect.width - (2 * marginInner);
                innerWidth = boxWidth - (2 * paddingBox);
                posX += offsetX;

                // Draw light edge line.
                var edgeHeight = 1;
                var lightEdgeRect = new Rect(posX, posY, rect.width, edgeHeight);
                EditorGUI.DrawRect(lightEdgeRect, colLight);

                // Draw header background.
                var headerHeight = EditorGUIUtility.singleLineHeight + (2 * paddingBox);
                var headerRect = new Rect(posX, posY, rect.width, headerHeight);
                EditorGUI.DrawRect(headerRect, colElementHeader);

                // Type field in header
                EditorGUI.PropertyField(new Rect(posX + marginInner + paddingBox, posY + paddingBox, 100, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Type"), GUIContent.none);

                // Get the height of this element.
                var height = GetHeightByType(element, (ElementType)typeProperty.enumValueIndex);

                posY += headerHeight;

                // Draw background
                var backgroundHeight = height - headerHeight - elementGap;
                var backgroundRect = new Rect(posX, posY, rect.width, backgroundHeight);
                EditorGUI.DrawRect(backgroundRect, colElementBackground);

                // Draw shadow edge line.
                var shadowEdgeRect = new Rect(posX, posY + backgroundHeight, rect.width, edgeHeight);
                EditorGUI.DrawRect(shadowEdgeRect, colShadow);

                posY += marginInner;
                posX += marginInner;

                // Show fields for headline.
                if (typeProperty.enumValueIndex == (int)ElementType.Headline)
                {
                    // Show a dropdown for the different headline styles.
                    EditorGUI.PropertyField(new Rect(posX, posY, boxWidth, EditorGUIUtility.singleLineHeight), headlineStyleProperty, GUIContent.none);
                    posY += EditorGUIUtility.singleLineHeight + propertiesGap;

                    DrawTextFieldProperties(rect, headlineProperty, 1, textAlignmentProperty);
                }

                // Show fields for text.
                if (typeProperty.enumValueIndex == (int)ElementType.Text)
                {
                    DrawTextFieldProperties(rect, textMainProperty, 6, textAlignmentProperty);
                }

                // Show fields for two columns
                if (typeProperty.enumValueIndex == (int)ElementType.TwoColumns)
                {
                    EditorGUI.PropertyField(new Rect(posX, posY, boxWidth, EditorGUIUtility.singleLineHeight), columnsStyleProperty, GUIContent.none);
                    posY += EditorGUIUtility.singleLineHeight + propertiesGap;

                    // Draw text fields.
                    if (columnsStyle != ColumnsStyle.TwoImages)
                    {
                        var textLines = columnsStyle == ColumnsStyle.SingleText ? 15 : 6;
                        string label1Text;

                        // Text label depends on choice.
                        switch (columnsStyle)
                        {
                            case ColumnsStyle.SingleText:
                                label1Text = "Both Columns:";
                                break;
                            case ColumnsStyle.TwoTexts:
                                label1Text = "Left Column:";
                                break;
                            default:
                                label1Text = "Text:";
                                break;
                        }

                        // Text field 1.
                        DrawTextFieldProperties(rect, textMainProperty, textLines, null, label1Text);
                        posY += propertiesGap;

                        // Text field 2.
                        if (columnsStyle == ColumnsStyle.TwoTexts)
                        {
                            DrawTextFieldProperties(rect, textSecondaryProperty, 6, null, "Right Column:");
                        }
                    }
                }

                // Image 1.
                if (typeProperty.enumValueIndex == (int)ElementType.Image || (typeProperty.enumValueIndex == (int)ElementType.TwoColumns && columnsStyleProperty.enumValueIndex != (int)ColumnsStyle.SingleText && columnsStyleProperty.enumValueIndex != (int)ColumnsStyle.TwoTexts))
                {
                    DrawImageProperties(rect, imageMainProperty, imageDescriptionMainProperty);
                }

                // Image 2.
                if (typeProperty.enumValueIndex == (int)ElementType.TwoColumns && columnsStyleProperty.enumValueIndex == (int)ColumnsStyle.TwoImages)
                {
                    posY += propertiesGap;
                    DrawImageProperties(rect, imageSecondaryProperty, imageDescriptionSecondaryProperty);
                }

                // Columns slider.
                if (typeProperty.enumValueIndex == (int)ElementType.TwoColumns && columnsStyleProperty.enumValueIndex != (int)ColumnsStyle.SingleText)
                {
                    posY += propertiesGap;
                    posX += paddingBox;

                    var labelRatioWidth = 94;
                    EditorGUI.LabelField(new Rect(posX, posY, labelRatioWidth, EditorGUIUtility.singleLineHeight), "Columns width:");
                    var labelAutoWidth = 0;
                    var checkBoxAutoWidth = 0;

                    if (columnsStyleProperty.enumValueIndex == (int)ColumnsStyle.TwoImages)
                    {
                        labelAutoWidth = 32;
                        checkBoxAutoWidth = 12;
                        EditorGUI.LabelField(new Rect(posX + labelRatioWidth, posY, labelAutoWidth, EditorGUIUtility.singleLineHeight), "Auto");
                        EditorGUI.PropertyField(new Rect(posX + labelRatioWidth + labelAutoWidth, posY, checkBoxAutoWidth, EditorGUIUtility.singleLineHeight), autoColumnsRatioProperty, GUIContent.none);
                    }

                    if (!autoColumnsRatioProperty.boolValue || columnsStyleProperty.enumValueIndex != (int)ColumnsStyle.TwoImages)
                    {
                        var offsetPosX = labelRatioWidth + labelAutoWidth + checkBoxAutoWidth + propertiesGap;
                        EditorGUI.PropertyField(new Rect(posX + offsetPosX, posY, innerWidth - offsetPosX, EditorGUIUtility.singleLineHeight), columnsRatioProperty, GUIContent.none);
                    }
                }

                // Show fields for button.
                if (typeProperty.enumValueIndex == (int)ElementType.Button)
                {
                    DrawButtonProperties(rect, buttonClickProperty, headlineProperty, headlineStyleProperty, textMainProperty, textAlignmentProperty);
                }

                // Show fields for seperator.
                if (typeProperty.enumValueIndex == (int)ElementType.Seperator)
                {
                    var labelSeperatorWidth = 43f;
                    var fieldSeperatorWidth = 32f;
                    EditorGUI.LabelField(new Rect(posX, posY, labelSeperatorWidth, EditorGUIUtility.singleLineHeight), "Space");
                    EditorGUI.PropertyField(new Rect(posX + labelSeperatorWidth, posY, fieldSeperatorWidth, EditorGUIUtility.singleLineHeight), seperatorSpaceProperty, GUIContent.none);

                    var labelShowSeperatorLineWidth = 70f;
                    var offsetLineX = labelSeperatorWidth + (2.5f * propertiesGap) + fieldSeperatorWidth;
                    EditorGUI.LabelField(new Rect(posX + offsetLineX, posY, labelShowSeperatorLineWidth, EditorGUIUtility.singleLineHeight), "Show Line");
                    EditorGUI.PropertyField(new Rect(posX + offsetLineX + labelShowSeperatorLineWidth, posY, innerWidth - labelShowSeperatorLineWidth, EditorGUIUtility.singleLineHeight), showSeperatorLineProperty, GUIContent.none);
                }

                // Show fields for video.
                if (typeProperty.enumValueIndex == (int)ElementType.Video)
                {
                    DrawVideoProperties(rect, videoClipProperty, imageDescriptionMainProperty, autoPlayAndLoopProperty);
                }
            };

        // Add new element callback. Add element and insert placeholder text.
        reorderableList.onAddCallback = (_) =>
        {
            // Add new element to list.
            reorderableList.serializedProperty.arraySize++;

            var element = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.serializedProperty.arraySize - 1);
            heights.Add(GetHeightByType(element, (ElementType)element.FindPropertyRelative("Type").enumValueIndex));

            // Set Type to headline.
            element.FindPropertyRelative(propertyNameType).intValue = 0;

            // Place holder text.
            element.FindPropertyRelative(propertyNameHeadline).stringValue = "Headline";
            element.FindPropertyRelative(propertyNameTextMain).stringValue = "Text body.";

            // Setting the text alignment to "middle justified" (4104). This is important, otherwise initialized with 0 shows unexpected alignment.
            element.FindPropertyRelative(propertyNameTextAlignment).intValue = 4104;

            // Initializing image column ratio.
            element.FindPropertyRelative(propertyNameColumnsRatio).floatValue = 0.5f;

            // Init values for other properties.
            element.FindPropertyRelative(propertyNameTextSecondary).stringValue = String.Empty;
            element.FindPropertyRelative(propertyNameImageMain).objectReferenceValue = null;
            element.FindPropertyRelative(propertyNameImageSecondary).objectReferenceValue = null;
            element.FindPropertyRelative(propertyNameImageDescriptionMain).stringValue = String.Empty;
            element.FindPropertyRelative(propertyNameImageDescriptionSecondary).stringValue = String.Empty;
            element.FindPropertyRelative(propertyNameSeperatorSpace).floatValue = 1f;

            serializedObject.ApplyModifiedProperties();
        };

        // Element height callback. Returns the height for element of index.
        reorderableList.elementHeightCallback = (index) =>
        {
            Repaint();
            UpdateHeights();
            float height = 0;

            try
            {
                height = heights[index];
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning(e.Message);
            }

            return height;
        };
    }

    /// <summary>
    /// This draws an image properties box.
    /// </summary>
    /// <param name="rect">Element rect.</param>
    /// <param name="imageProperty">Serialized property of the image.</param>
    /// <param name="imageDescirptionProperty">Serialized property of the image description text.</param>
    private void DrawImageProperties(Rect rect, SerializedProperty imageProperty, SerializedProperty imageDescirptionProperty)
    {
        var boxExtra = 4;

        // Draw box around image properties.
        var boxHeight = (6 * EditorGUIUtility.singleLineHeight) + (2 * paddingBox) + propertiesGap + boxExtra;
        EditorGUI.DrawRect(new Rect(posX, posY, boxWidth, boxHeight), colBox);

        posY += paddingBox;
        posX += paddingBox;

        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, EditorGUIUtility.singleLineHeight), imageProperty, GUIContent.none);

        posY += EditorGUIUtility.singleLineHeight + propertiesGap;

        var thumbnailHeight = (5f * EditorGUIUtility.singleLineHeight) + boxExtra;

        // Show texture preview image.
        if (imageProperty.objectReferenceValue != null)
        {
            var imageSrc = imageProperty.objectReferenceValue as Texture2D;
            var imageAspectRatio = (float)imageSrc.width / imageSrc.height;

            var thumbnailWidth = Mathf.Clamp(thumbnailHeight * imageAspectRatio, 10, innerWidth * 0.5f);

            EditorGUI.DrawTextureTransparent(new Rect(posX, posY, thumbnailWidth, thumbnailHeight), imageSrc);

            // Draw description text field.
            EditorGUI.LabelField(new Rect(posX + thumbnailWidth + propertiesGap, posY - 3, innerWidth - thumbnailWidth - propertiesGap, EditorGUIUtility.singleLineHeight), "Description (optional)");
            EditorGUI.PropertyField(new Rect(posX + thumbnailWidth + propertiesGap, posY, innerWidth - thumbnailWidth - propertiesGap, thumbnailHeight), imageDescirptionProperty, GUIContent.none);
        }
        else
        {
            // When no image source is set, show info.
            EditorGUI.HelpBox(new Rect(posX, posY, rect.width - 50, EditorGUIUtility.singleLineHeight), "Set a reference to an image texture.", MessageType.Info);
        }

        posX -= paddingBox;
        posY += thumbnailHeight + paddingBox;
    }

    /// <summary>
    /// This draws a video properties box.
    /// </summary>
    /// <param name="rect">Element rect.</param>
    /// <param name="videoClipProperty">Serialized property of the video clip.</param>
    /// <param name="imageDescirptionProperty">Serialized property of the video description text.</param>
    /// <param name="autoPlayAndLoopProperty">Serialized property of the autoplay and loop bool.</param>
    private void DrawVideoProperties(Rect rect, SerializedProperty videoClipProperty, SerializedProperty imageDescirptionProperty, SerializedProperty autoPlayAndLoopProperty)
    {
        // Draw box around image properties.
        var boxHeight = (5 * EditorGUIUtility.singleLineHeight) + (2 * paddingBox) + (2 * propertiesGap);
        EditorGUI.DrawRect(new Rect(posX, posY, boxWidth, boxHeight), colBox);

        posY += paddingBox;
        posX += paddingBox;

        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, EditorGUIUtility.singleLineHeight), videoClipProperty, GUIContent.none);

        posY += EditorGUIUtility.singleLineHeight + propertiesGap;

        var descriptionHeight = 3f * EditorGUIUtility.singleLineHeight;

        // Show fields for video.
        if (videoClipProperty.objectReferenceValue != null)
        {
            // Draw description text field.
            EditorGUI.LabelField(new Rect(posX, posY - 3, innerWidth, EditorGUIUtility.singleLineHeight), "Description (optional)");
            EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, descriptionHeight), imageDescirptionProperty, GUIContent.none);
            posY += descriptionHeight + propertiesGap;

            var labelAutoPlayWidth = 115f;
            EditorGUI.LabelField(new Rect(posX, posY, labelAutoPlayWidth, EditorGUIUtility.singleLineHeight), "Auto play and loop");
            EditorGUI.PropertyField(new Rect(posX + labelAutoPlayWidth, posY, labelAutoPlayWidth, EditorGUIUtility.singleLineHeight), autoPlayAndLoopProperty, GUIContent.none);
        }
        else
        {
            // When no image source is set, show info.
            EditorGUI.HelpBox(new Rect(posX, posY, rect.width - 50, EditorGUIUtility.singleLineHeight), "Set a reference to a video clip.", MessageType.Info);
        }

        posX -= paddingBox;
        posY += paddingBox;
    }

    /// <summary>
    /// This draws a text field.
    /// </summary>
    /// <param name="rect">Element rect.</param>
    /// <param name="textProperty">Serialized property of the text.</param>
    /// <param name="lines">The number of lines for the text field height.</param>
    /// <param name="alignmentProperty">Serialites property of the text alignment (optional).</param>
    /// <param name="label">Label text to be displayed above text field (optional).</param>
    private void DrawTextFieldProperties(Rect rect, SerializedProperty textProperty, int lines, SerializedProperty alignmentProperty = null, string label = null)
    {
        // We use these to calculate the height of the text field and box.
        var linesText = lines;
        var linesBox = lines;
        var gaps = 0;
        var boxExtra = 4;
        var heightTextalignment = 0f;

        // If there is a label, we add the gap between label and text to the height.
        if (label != null)
        {
            gaps += 1;
        }
        else
        {
            // If there is no label and the text field has more than one line, the box can be smaller
            // (since the first line of a multi-line text field will be empty space for the label and thus can be ignored).
            if (lines > 1)
            {
                linesBox -= 1;
            }
        }

        // Calculate the height of the text alignment field if applicable.
        if (alignmentProperty != null)
        {
            heightTextalignment = (2 * EditorGUIUtility.singleLineHeight) + propertiesGap + boxExtra;
        }

        // Do the math for the height.
        var textBoxHeight = (linesText * EditorGUIUtility.singleLineHeight) + boxExtra;
        var boxHeight = (linesBox * EditorGUIUtility.singleLineHeight) + (2 * paddingBox) + boxExtra + (gaps * propertiesGap) + heightTextalignment;

        // Draw box around text properties.
        EditorGUI.DrawRect(new Rect(posX, posY, boxWidth, boxHeight), colBox);

        posX += paddingBox;
        posY += paddingBox;

        // Draw the label.
        if (label != null)
        {
            EditorGUI.LabelField(new Rect(posX, posY, innerWidth, EditorGUIUtility.singleLineHeight), label);
            posY += propertiesGap;
        }
        else
        {
            // If it is a multi-line text field, but there is no label, we can move the text field up.
            // (since the first line of a multi-line text field will be empty space for the label and thus can be ignored).
            if (lines > 1)
            {
                posY -= EditorGUIUtility.singleLineHeight;
            }
        }

        // Show text field for text body.
        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, textBoxHeight), textProperty, GUIContent.none);
        posY += textBoxHeight;

        // Draw text alignment fields (if applicable).
        // If the user leaves the text field empty, we will show an info instead of text alignment options.
        // We do not show the info if there are no text alignment options.
        if (alignmentProperty != null)
        {
            posY += propertiesGap;

            // Check if the user put in a string value.
            if (string.IsNullOrWhiteSpace(textProperty.stringValue))
            {
                EditorGUI.HelpBox(new Rect(posX, posY, innerWidth, EditorGUIUtility.singleLineHeight), "Insert a text to be displayed as text body.", MessageType.Info);
            }
            else
            {
                // Show the options for text alignment.
                EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, EditorGUIUtility.singleLineHeight * 2f), alignmentProperty, GUIContent.none);
            }
        }

        posY += heightTextalignment + paddingBox;
        posX -= paddingBox;
    }

    /// <summary>
    /// This draws the button properties.
    /// </summary>
    /// <param name="rect">Element rect.</param>
    /// <param name="buttonClickProperty">Serialized property of OnClick events.</param>
    /// <param name="headlineProperty">Button headline.</param>
    /// <param name="textBodyProperty">Button text.</param>
    /// <param name="textAlignmentProperty">Button text alignment.</param>
    private void DrawButtonProperties(Rect rect, SerializedProperty buttonClickProperty, SerializedProperty headlineProperty, SerializedProperty headlineStyleProperty, SerializedProperty textBodyProperty, SerializedProperty textAlignmentProperty)
    {
        var onClickHeight = EditorGUI.GetPropertyHeight(buttonClickProperty);
        var boxExtra = 4;
        var boxHeight = (6 * EditorGUIUtility.singleLineHeight) + (3 * propertiesGap) + (2 * paddingBox) + onClickHeight + (5 * boxExtra);

        // Draw box around text properties.
        EditorGUI.DrawRect(new Rect(posX, posY, boxWidth, boxHeight), colBox);

        posY += paddingBox;
        posX += paddingBox;

        // Show a text field for the headline text.
        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth - 55, EditorGUIUtility.singleLineHeight + boxExtra), headlineProperty, GUIContent.none);

        // Show a dropdown for headline style.
        EditorGUI.PropertyField(new Rect(posX + innerWidth - 50, posY, 50, EditorGUIUtility.singleLineHeight + boxExtra), headlineStyleProperty, GUIContent.none);
        posY += propertiesGap + boxExtra;

        // Show text field for text body.
        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, (4f * EditorGUIUtility.singleLineHeight) + (3f * boxExtra)), textBodyProperty, GUIContent.none);
        posY += (4f * EditorGUIUtility.singleLineHeight) + propertiesGap + (3 * boxExtra);

        // Show the options for text alignment.
        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, (2f * EditorGUIUtility.singleLineHeight) + boxExtra), textAlignmentProperty, GUIContent.none);
        posY += (2f * EditorGUIUtility.singleLineHeight) + propertiesGap + boxExtra;

        // Show OnClick action field.
        EditorGUI.PropertyField(new Rect(posX, posY, innerWidth, onClickHeight), buttonClickProperty, GUIContent.none);
        posY += onClickHeight + propertiesGap;
    }

    /// <summary>
    /// Returns the height for the element by type.
    /// </summary>
    /// <param name="type">The element type.</param>
    /// <returns>Height value.</returns>
    private float GetHeightByType(SerializedProperty element, ElementType type)
    {
        // The height depends on the paddings, gaps, lines and maybe a few extra pixels.
        // We use these to do the math, so that the height dynamically changes when we change e.g. paddings and gaps values.
        var gaps = 0;
        int lines;
        var extra = 0;
        var paddings = 2;   // Since every element has a box, there will be at least a top and bottom padding. Elements with more boxes have more padding respectively.
        var margin = 2;     // Margin is the distance of the element content to the element border. This will always be 2.

        var headerHeight = EditorGUIUtility.singleLineHeight + (2 * paddingBox) + elementGap;

        // The number of lines, gaps, paddings and extra pixels per element type:
        switch (type)
        {
            case ElementType.Headline:
                lines = 4;
                gaps = 2;
                extra = 8;
                break;
            case ElementType.Text:
                lines = 7;
                gaps = 1;
                extra = 8;
                break;
            case ElementType.Image:
                lines = 6;
                extra = 4;
                gaps = 1;
                break;
            case ElementType.TwoColumns:
                lines = 14;
                paddings = 4;
                gaps = 5;
                extra = 8;
                break;
            case ElementType.Button:
                extra = (int)EditorGUI.GetPropertyHeight(element.FindPropertyRelative("OnButtonClick")) + 24;
                gaps = 3;
                lines = 6;
                break;
            case ElementType.Seperator:
                lines = 1;
                break;
            case ElementType.Video:
                lines = 5;
                gaps = 2;
                break;
            default:
                lines = 6;
                break;
        }

        // Do the math and return the height value.
        return headerHeight + (lines * EditorGUIUtility.singleLineHeight) + (paddings * paddingBox) + (gaps * propertiesGap) + (margin * marginInner) + extra;
    }

    /// <summary>
    /// Update the array of height values of the elements list.
    /// </summary>
    private void UpdateHeights(bool reorder = true)
    {
        if (reorder)
        {
            heights.Clear();
        }

        if (heights.Capacity != reorderableList.count)
        {
            heights = new List<float>(reorderableList.count);
        }

        for (var i = 0; i < reorderableList.count; i++)
        {
            var serializedProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(i);
            var type = (ElementType)serializedProperty.FindPropertyRelative("Type").intValue;
            heights.Insert(i, GetHeightByType(serializedProperty, type));
        }
    }
}