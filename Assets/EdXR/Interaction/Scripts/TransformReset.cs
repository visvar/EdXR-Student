using EdXR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformReset : MonoBehaviour
{
    [SerializeField] private bool resetOnEnable;

    private Vector3 originalPostion;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Transform originalTransform;
    private bool isInizialized;

    // Start is called before the first frame update
    private void Awake()
    {
        originalTransform = transform;
        var interactiveObject = GetComponent<InteractiveObject>();

        // If this object uses the InteractiveObject script, we get the parent after it had been initialized.
        if (interactiveObject != null)
        {
            interactiveObject.OnInitialized += InteractiveObject_OnInitialized;
        }
        else
        {
            GetOriginalTransform();
        }
    }

    private void OnEnable()
    {
        if (resetOnEnable)
        {
            ResetAll();
        }
    }

    private void GetOriginalTransform()
    {
        originalRotation = originalTransform.rotation;
        originalPostion = originalTransform.position;
        originalScale = originalTransform.localScale;
        isInizialized = true;
    }

    private void InteractiveObject_OnInitialized()
    {
        originalTransform = transform.parent;
        GetOriginalTransform();
    }

    /// <summary>
    /// Reset position, rotation and scale.
    /// </summary>
    public void ResetAll()
    {
        if (isInizialized)
        {
            originalTransform.position = originalPostion;
            originalTransform.rotation = originalRotation;
            originalTransform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Reset position only.
    /// </summary>
    public void ResetPosition()
    {
        if (isInizialized)
        {
            originalTransform.position = originalPostion;
        }
    }

    /// <summary>
    /// Reset rotation only.
    /// </summary>
    public void ResetRotation()
    {
        if (isInizialized)
        {
            originalTransform.rotation = originalRotation;
        }
    }

    /// <summary>
    /// Reset scale only.
    /// </summary>
    public void ResetScale()
    {
        if (isInizialized)
        {
            originalTransform.localScale = originalScale;
        }
    }
}
