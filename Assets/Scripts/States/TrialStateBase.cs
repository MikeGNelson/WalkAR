using Avoidance;
using UnityEngine;

public abstract class TrialStateBase
{
    protected TrialController controller;
    protected GameController gameController;
    protected PlayerController playerController;
    protected DataManager dataManager;

    public TrialStateBase(TrialController controller)
    {
        this.controller = controller;
        this.gameController = controller.GC;
        this.playerController = controller.PC;
        this.dataManager = controller.DM;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
