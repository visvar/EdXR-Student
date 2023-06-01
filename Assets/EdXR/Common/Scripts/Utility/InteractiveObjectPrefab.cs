using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;

public class InteractiveObjectPrefab : MonoBehaviour
{
    [SerializeField] BoundsControl boundsControl;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] ObjectManipulator objectManipulator;
    [SerializeField] RotationAxisConstraint rotationAxisConstraint;
    [SerializeField] new Rigidbody rigidbody;

    /// <summary>
    /// Gets the BoundsControl component on this prefab.
    /// </summary>
    public BoundsControl BoundsControl => boundsControl;

    /// <summary>
    /// Gets the ObjectManipulator component on this prefab.
    /// </summary>
    public ObjectManipulator ObjectManipulator => objectManipulator;

    /// <summary>
    /// Gets the RotationAxisConstraint component on this prefab.
    /// </summary>
    public RotationAxisConstraint RotationAxisConstraint => rotationAxisConstraint;

    /// <summary>
    /// Gets the RotationAxisConstraint component on this prefab.
    /// </summary>
    public Rigidbody Rigidbody => rigidbody;

    /// <summary>
    /// Initializes the bounds of the <see cref="BoundsControl"/> of this object.
    /// </summary>
    public void InitializeBounds()
    {
        Bounds totalBounds = new Bounds();

        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            totalBounds.Encapsulate(renderer.bounds);
        }

        boxCollider.center = totalBounds.center;
        boxCollider.size = totalBounds.size;

        boundsControl.UpdateBounds();
    }
}
