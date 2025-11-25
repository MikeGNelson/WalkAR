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

        // Spawn the start-state UI
        controller.SpawnStartUI();

        Debug.Log("[TrialStartState] Awaiting trigger to start trial.");
    }

    public override void Update()
    {
        // A (one step forward)
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            controller.SetTrialIndex(controller.CurrentTrialIndex + 1);
        }

        // B (one step backward)
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            controller.SetTrialIndex(controller.CurrentTrialIndex - 1);
        }

        // Wait for right trigger input to begin
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            Debug.Log("[TrialStartState] Trigger pressed → Starting trial.");


            controller.DestroyStartUI();

            // Move to Run state
            controller.ChangeState(new TrialRunState(controller));
        }
    }

    public override void Exit()
    {
        // GameController.StartTrial() updates its own promptText automatically
        controller.DestroyStartUI();
    }
}
