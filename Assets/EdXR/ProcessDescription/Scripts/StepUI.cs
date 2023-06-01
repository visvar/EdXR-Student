using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace EdXR.ProcessDescription
{
    /// <summary>
    /// This class defines UI for the <see cref="StepController"/>.
    /// </summary>
    [SelectionBase]
    public class StepUI : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textCurrentStep;
        [SerializeField] private TextMeshPro textTotalStepCount;
        [SerializeField] private Interactable buttonNext;

        private StepController stepController;

        /// <summary>
        /// Initialize this UI.
        /// </summary>
        /// <param name="stepController">The step controller this UI belongs to.</param>
        public void Initialize(StepController stepController)
        {
            this.stepController = stepController;
        }

        /// <summary>
        /// This event gets called when the steps change in order to update the label.
        /// </summary>
        /// <param name="disableNextButton">True if the next button is disabled.</param>
        public void OnStepChange(bool disableNextButton = false)
        {
            buttonNext.IsEnabled = !disableNextButton;
            textCurrentStep.text = (stepController.CurrentStep + 1).ToString();
            textTotalStepCount.text = stepController.StepCount.ToString();
        }

        /// <summary>
        /// Go to the previous step.
        /// </summary>
        public void NextStep()
        {
            stepController?.NextStep();
        }

        /// <summary>
        /// Go to the next step.
        /// </summary>
        public void PreviousStep()
        {
            stepController?.PreviousStep();
        }
    }
}
