using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.Events;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

namespace EdXR
{
    /// <summary>
    /// Define an object that will snap in place at the position of the target object.
    /// </summary>
    public class TargetSnap : MonoBehaviour
    {
        private readonly WaitForSeconds waitForCheck = new WaitForSeconds(0.1f);

        [SerializeField] private Transform target;
        [SerializeField] private bool resetOnEnable = true;

        [Header("Snap Tolerance")]
        [SerializeField] private float snapDistance = 0.01f;
        [SerializeField] private float snapAngle = 10f;

        [Header("Symmetry (optional)")]
        [SerializeField] private bool symmetryX;
        [SerializeField] private bool symmetryY;
        [SerializeField] private bool symmetryZ;

        [Space]
        [SerializeField] private UnityEvent onSnap;

        private Quaternion flipX = Quaternion.Euler(Vector3.up * 180);
        private Quaternion flipY = Quaternion.Euler(Vector3.forward * 180);
        private Quaternion flipZ = Quaternion.Euler(Vector3.right * 180);

        private Transform manipulationParent;
        private BoundsControl boundsControl;
        private new Rigidbody rigidbody;
        private ObjectManipulator manipulator;
        private InteractiveObject interactiveObject;

        private bool isInitialized = false;

        private TransformReset transformReset;

        /// <summary>
        /// Reset the target snap to its original state.
        /// </summary>
        public void ResetSnap()
        {
            if (isInitialized)
            {
                transformReset.ResetAll();
                SnappedState(false);
            }
        }

        private void Awake()
        {
            // We add the TransformReset script, so that we can reset the transform.
            transformReset = GetComponent<TransformReset>() ?? gameObject.AddComponent<TransformReset>();
            interactiveObject = GetComponent<InteractiveObject>();

            // If this object uses the InteractiveObject script, we get the parent after it had been initialized.
            if (interactiveObject != null)
            {
                interactiveObject.OnInitialized += InteractiveObject_OnInitialized;
            }
            else
            {
                manipulationParent = transform;
                rigidbody = GetComponent<Rigidbody>();
                boundsControl = manipulationParent.GetComponent<BoundsControl>();
                manipulator = manipulationParent.GetComponent<ObjectManipulator>();
                isInitialized = true;
            }
        }

        private void InteractiveObject_OnInitialized()
        {
            manipulationParent = transform.parent;
            isInitialized = true;
        }

        // Start is called before the first frame update
        private void OnEnable()
        {
            if (resetOnEnable)
            {
                ResetSnap();
            }

            StartCoroutine(CheckPos());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            if (interactiveObject != null)
            {
                interactiveObject.OnInitialized -= InteractiveObject_OnInitialized;
            }
        }

        private void SnappedState(bool isSnapped)
        {
            if (interactiveObject == null)
            {
                // Make rigidbody kinematic.
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = isSnapped;
                }

                // Deactivate the object manipulator so that it cannot be moved anymore.
                if (manipulator != null)
                {
                    manipulator.enabled = !isSnapped;
                }

                // Same goes for bounds control.
                if (boundsControl != null)
                {
                    boundsControl.enabled = !isSnapped;
                }
            }

            interactiveObject.Freeze(isSnapped);
            target.gameObject.SetActive(!isSnapped);
        }

        // CheckPos is called several times per second.
        private IEnumerator CheckPos()
        {
            while (true)
            {
                var deltaDistance = Vector3.Distance(transform.position, target.position);
                var deltaScale = Mathf.Abs(transform.lossyScale.magnitude - target.lossyScale.magnitude);
                var transformRotations = new List<Quaternion>();

                transformRotations.Add(transform.rotation);

                // For every possible rotation add flipped rotation to transformRotations.
                if (symmetryX)
                {
                    transformRotations.Add(transform.rotation * flipX);
                }

                if (symmetryX && symmetryY)
                {
                    transformRotations.Add(transform.rotation * flipX * flipY);
                }

                if (symmetryY)
                {
                    transformRotations.Add(transform.rotation * flipY);
                }

                if (symmetryY && symmetryZ)
                {
                    transformRotations.Add(transform.rotation * flipY * flipZ);
                }

                if (symmetryZ)
                {
                    transformRotations.Add(transform.rotation * flipZ);
                }

                if (symmetryX && symmetryY && symmetryZ)
                {
                    transformRotations.Add(transform.rotation * flipX * flipY * flipZ);
                }

                // Get angle between all transformations and target rotation.
                var deltaAngles = new float[transformRotations.Count];
                for (var i = 0; i < transformRotations.Count; i++)
                {
                    deltaAngles[i] = Quaternion.Angle(transformRotations[i], target.rotation);
                }

                // Get the smallest angle.
                var deltaAngle = Mathf.Min(deltaAngles);

                // When the object is close enough to the target, it snaps into place.
                if (target.gameObject.activeSelf && (deltaDistance < snapDistance && deltaAngle < snapAngle && deltaScale < snapDistance))
                {
                    // Set exact position and rotation to target.
                    manipulationParent.transform.rotation = target.rotation;
                    manipulationParent.transform.position = target.position;

                    // Change state to snapped.
                    SnappedState(true);

                    // Invoke onSnap events.
                    onSnap.Invoke();
                }

                yield return waitForCheck;
            }
        }
    }
}