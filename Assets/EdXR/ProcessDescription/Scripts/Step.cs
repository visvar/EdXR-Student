using UnityEngine;
using UnityEngine.Events;

namespace EdXR.ProcessDescription
{
    /// <summary>
    /// This defines a single step for the <see cref="StepController"/> in the process description.
    /// </summary>
    public class Step : MonoBehaviour
    {
        [Tooltip("When the step controller's current index is larger than this step's index, these GameObjects will remain active.")]
        [SerializeField] private Transform[] persistentElements;
        [SerializeField] private bool disableNextButton;
        [SerializeField] private UnityEvent onStepActivate;

        private GameObject stepContent;
        private GameObject persistentStepContent;
        private StepController stepController;
        private int stepIndex;

        private TargetSnap[] persistentTargetSnaps;

        /// <summary>
        /// Gets a value indicating whether the next button in the step UI should be disabled.
        /// </summary>
        public bool DisableNextButton => disableNextButton;

        /// <summary>
        /// Initializes the step.
        /// </summary>
        /// <param name="stepController">The step controller of this step.</param>
        /// <param name="stepIndex">The index given by the step controller.</param>
        public void InitializeStep(StepController stepController, int stepIndex)
        {
            this.stepController = stepController;
            this.stepIndex = stepIndex;
        }

        /// <summary>
        /// Set this step active/inactive.
        /// </summary>
        /// <param name="isActive">Determines whether this step should be active (true) or inactive (false).</param>
        public void SetActive(bool isActive)
        {
            stepContent.SetActive(isActive);
            persistentStepContent.SetActive(stepController.CurrentStep >= stepIndex);

            if (isActive)
            {
                onStepActivate?.Invoke();

                // Reset target snaps if they did not get enabled/disabled, but the step is active.
                foreach (var targetSnap in persistentTargetSnaps)
                {
                    targetSnap.ResetSnap();
                }
            }
        }

        // Start is called before the first frame update
        private void Awake()
        {
            stepContent = new GameObject("[Step Content] " + gameObject.name);
            persistentStepContent = new GameObject("[Persistent Step Content] " + gameObject.name);
            stepContent.transform.position = transform.position;
            var children = new Transform[transform.childCount];

            // Get this step's children.
            for (var i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            // Move children to step content game object.
            for (var i = 0; i < children.Length; i++)
            {
                children[i].SetParent(stepContent.transform, true);
            }

            // Move persistent elements to persistent step content game object.
            for (var i = 0; i < persistentElements.Length; i++)
            {
                persistentElements[i].SetParent(persistentStepContent.transform, true);
            }

            persistentTargetSnaps = persistentStepContent.GetComponentsInChildren<TargetSnap>();

            stepContent.transform.SetParent(transform, true);
            persistentStepContent.transform.SetParent(transform, true);

            stepContent.SetActive(false);
            persistentStepContent.SetActive(false);
        }
    }
}