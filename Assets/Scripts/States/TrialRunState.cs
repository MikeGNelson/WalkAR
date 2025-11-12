using UnityEngine;

public class TrialRunState : TrialStateBase
{
    private bool hasStarted = false;

    public TrialRunState(TrialController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[TrialRunState] Entered – calling GameController.StartTrial()");

        // Start the trial (spawns UI and begins recording)
        gameController.StartTrial();
        hasStarted = true;

        
    }

    public override void Update()
    {
        if (!hasStarted)
            return;

        bool reachedGoal = playerController.hasReachedDestination;
        bool manualEnd = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);


        if (reachedGoal || manualEnd)
        {
            Debug.Log("[TrialRunState] Ending trial – calling GameController.EndTrial()");
            gameController.EndTrial();
            controller.ChangeState(new TrialEndState(controller));
        }
    }

    public override void Exit()
    {
        Debug.Log("[TrialRunState] Exiting Run state");
    }
}
