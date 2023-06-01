using EdXR.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace EdXR.ProcessDescription
{
    public class StepController : MonoBehaviour
    {
        [SerializeField] StepUI stepUI;
        [SerializeField] List<Step> steps = new List<Step>();

        private int activeStepIndex = 0;
        private bool previewAllActive;

        /// <summary>
        /// Gets the index of the currently active step.
        /// </summary>
        public int CurrentStep => activeStepIndex;

        /// <summary>
        /// Gets or sets the step UI.
        /// </summary>
        public StepUI StepUI
        {
            get
            {
                return stepUI;
            }

            set
            {
                stepUI = value;
            }
        }

        /// <summary>
        /// Gets the total number of steps.
        /// </summary>
        public int StepCount => steps.Count;

        /// <summary>
        /// Go to a certain step.
        /// </summary>
        /// <param name="index">Index of new active step.</param>
        public void GoToStep(int index)
        {
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] != null)
                {
                    steps[i].SetActive(i == index);
                    stepUI?.OnStepChange(steps[index].DisableNextButton);
                }
                else
                {
                    Debug.LogWarning("Steps list null or index out of bounds.");
                }

                activeStepIndex = index;
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Activates/deacitvates the step game object. Only use this in the editor preview.
        /// </summary>
        /// <param name="index">Index of step.</param>
        public void Preview(int index)
        {
            if (steps[index] != null && steps[index].gameObject != null)
            {
                steps[index].gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Toggle all steps for preview.
        /// </summary>
        /// <param name="isOn">Toggle all to this.</param>
        public void PreviewToggleAll(bool isOn)
        {
            foreach (var step in steps)
            {
                if (step != null)
                {
                    step.gameObject.SetActive(isOn);
                }
            }

            previewAllActive = isOn;
        }

        /// <summary>
        /// Toggle all steps for preview.
        /// </summary>
        public void PreviewToggleAll()
        {
            PreviewToggleAll(!previewAllActive);
        }
#endif

        /// <summary>
        /// Go to the previous step.
        /// </summary>
        public void PreviousStep()
        {
            ChangeStep(-1);
        }

        /// <summary>
        /// Go to the next step.
        /// </summary>
        public void NextStep()
        {
            ChangeStep(1);
        }

        /// <summary>
        /// Adds an empty step to the steps.
        /// </summary>
        public void AddStep()
        {
            var stepName = SceneHelpers.UniqueInstanceName<Step>("Step", gameObject);
            var stepObject = new GameObject(stepName);
            var step = stepObject.AddComponent<Step>();
            stepObject.transform.SetParent(transform, false);
            steps.Add(step);
        }

        /// <summary>
        /// Go to the respective step by delta to currently active step.
        /// </summary>
        /// <param name="delta">Delta to currently active step.</param>
        private void ChangeStep(int delta)
        {
            // Clamp the index between index of first and last step.
            GoToStep(Mathf.Clamp(activeStepIndex + delta, 0, steps.Count - 1));
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Remove steps that are null.
            steps.RemoveAll(item => item == null);

            // Make sure that all steps are initialized, step's game objects are activated, the steps deactivated.
            for (var i = 0; i < steps.Count; i++)
            {
                steps[i].gameObject.SetActive(true);
                steps[i].InitializeStep(this, i);
                steps[i].SetActive(false);
            }

            if (stepUI != null)
            {
                stepUI.Initialize(this);
            }

            GoToStep(0);
        }
    }
}