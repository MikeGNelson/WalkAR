using UnityEngine;

public class TrialStartState : TrialStateBase
{
    public TrialStartState(TrialController controller) : base(controller) { }

    public override void Enter()
    {
        

        // Clear leftover models/UI from the previous trial
        var clearMethod = gameController.GetType().GetMethod("ClearModels",
                          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        clearMethod?.Invoke(gameController, null);

        

        Debug.Log("[TrialStartState] Awaiting trigger to start trial.");
    }

    public override void Update()
    {
        // Wait for right trigger input to begin
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            Debug.Log("[TrialStartState] Trigger pressed → Starting trial.");

            
            // Move to Run state
            controller.ChangeState(new TrialRunState(controller));
        }
    }

    public override void Exit()
    {
        // GameController.StartTrial() updates its own promptText automatically
    }
}
