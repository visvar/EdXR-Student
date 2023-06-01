using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace EdXR
{
    /// <summary>
    /// This script will instantiate the prefab with preset BoundsControl and ObjectManipulator components and make this game object a child of it.
    /// </summary>
    [SelectionBase]
    public class InteractiveObject : MonoBehaviour
    {
        [SerializeField] private Transform prefab;
        [SerializeField] private ManipulationHandFlags manipulationType = ManipulationHandFlags.OneHanded;
        [SerializeField] private bool useHandles;
        [SerializeField] private bool allowFarManipulation = true;
        [SerializeField] private PhysicsModes physics = PhysicsModes.None;
        [SerializeField][Tooltip("Used for physics.")] private float mass = 1f;
        [SerializeField] private UnityEvent onManipulationStarted;
        [SerializeField] private UnityEvent onManipulationEnded;

        private bool isInteractive = true;
        private InteractiveObjectPrefab interactive;
        private Transform interactiveParent;
        private bool initialized;

        /// <summary>
        /// Gets or sets an action that gets called when this interactive object has been initialized.
        /// </summary>
        public Action OnInitialized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this object is interactive.
        /// </summary>
        public bool IsInteractive
        {
            get
            {
                return isInteractive;
            }

            set
            {
                isInteractive = value;
                UpdateInteractivness();
            }
        }

        /// <summary>
        /// Sets the template prefab (used in the Editor menu extension).
        /// </summary>
        /// <param name="template">Template prefab from assets.</param>
        public void SetTemplate(Transform template)
        {
            prefab = template;
        }

        /// <summary>
        /// Freeze this interactive object so that it cannot be moved by user input or physics anymore.
        /// </summary>
        /// <param name="isFrozen">Freeze (true) or unfreeze (false).</param>
        public void Freeze(bool isFrozen)
        {
            IsInteractive = !isFrozen;
            UpdatePhysics(!isFrozen);
        }

        private void UpdatePhysics(bool usePhysics)
        {
            if (usePhysics)
            {
                // Configure rigidbody for physics.
                switch (physics)
                {
                    case PhysicsModes.None:
                        // If there is no physics, we remove the rigidbody component from the interactive object.
                        if (interactive.Rigidbody != null)
                        {
                            Destroy(interactive.Rigidbody);
                        }

                        break;
                    case PhysicsModes.Collison:
                        interactive.Rigidbody.mass = mass;
                        interactive.Rigidbody.useGravity = false;
                        interactive.Rigidbody.isKinematic = true;
                        break;
                    case PhysicsModes.UseGravity:
                        interactive.Rigidbody.mass = mass;
                        interactive.Rigidbody.useGravity = true;
                        interactive.Rigidbody.isKinematic = false;
                        break;
                    case PhysicsModes.ZeroGravity:
                        interactive.Rigidbody.mass = mass;
                        interactive.Rigidbody.useGravity = false;
                        interactive.Rigidbody.isKinematic = false;
                        break;
                }
            }
            else
            {
                // If physics get disabled, we make the rigidbody kinematic.
                if (physics != PhysicsModes.None)
                {
                    interactive.Rigidbody.isKinematic = true;
                }
            }
        }

        private void UpdateInteractivness()
        {
            interactive.BoundsControl.enabled = useHandles;
            interactive.ObjectManipulator.enabled = isInteractive;
        }

        private void ManipulationEnded(ManipulationEventData eventData)
        {
            onManipulationEnded.Invoke();
        }

        private void ManipulationStarted(ManipulationEventData eventData)
        {
            onManipulationStarted.Invoke();
        }

        private void OnDestroy()
        {
            interactive.ObjectManipulator.OnManipulationStarted.RemoveListener(ManipulationStarted);
            interactive.ObjectManipulator.OnManipulationEnded.RemoveListener(ManipulationEnded);
        }

        private void OnEnable()
        {
            if (initialized)
            {
                interactive.ObjectManipulator.ForceEndManipulation();
            }
        }

        private void OnDisable()
        {
            if (initialized)
            {
                interactive.ObjectManipulator.ForceEndManipulation();
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Create a parent object using the interactive object prefab.
            interactiveParent = Instantiate(prefab, transform);
            interactiveParent.name = gameObject.name + " [Interactive]";
            interactiveParent.tag = gameObject.tag;
            interactiveParent.gameObject.layer = gameObject.layer;
            interactiveParent.localPosition = Vector3.zero;
            interactiveParent.localEulerAngles = Vector3.zero;
            interactiveParent.SetParent(transform.parent, true);

            // Get the InteractiveObjectPrefab script from the new parent object.
            interactive = interactiveParent.GetComponent<InteractiveObjectPrefab>();

            if (interactive == null)
            {
                Debug.LogWarning("Prefab for [" + gameObject.name + "] does not contain an InteractiveObjectPrefab script. Please use correct prefab.");
                return;
            }

            // Make this object a child of the parent object and initialize bounds.
            transform.SetParent(interactiveParent);
            interactive.InitializeBounds();

            // Configure ObjectManipulator script.
            interactive.ObjectManipulator.ManipulationType = manipulationType;
            interactive.ObjectManipulator.AllowFarManipulation = allowFarManipulation;

            // Configure rigidbody.
            UpdatePhysics(true);

            // Configure BoundsControl script.
            interactive.BoundsControl.enabled = useHandles;
            interactive.RotationAxisConstraint.ConstraintOnRotation = useHandles ? AxisFlags.XAxis | AxisFlags.YAxis | AxisFlags.ZAxis : 0;

            interactive.ObjectManipulator.OnManipulationStarted.AddListener(ManipulationStarted);
            interactive.ObjectManipulator.OnManipulationEnded.AddListener(ManipulationEnded);

            initialized = true;
            OnInitialized?.Invoke();
        }
    }
}