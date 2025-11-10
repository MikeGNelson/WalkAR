using UnityEngine;

public class TrialEndState : TrialStateBase
{
    public TrialEndState(TrialController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[TrialEndState] Entered end state.");
        // GameController.EndTrial() already shows the correct prompt text.
        // Nothing else required here.
    }

    public override void Update()
    {
        // Wait for right trigger to continue
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            // If the experiment is still running, GC.EndTrial has already advanced the index.
            controller.ChangeState(new TrialStartState(controller));
        }
    }
}
