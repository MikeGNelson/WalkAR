using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Avoidance;

public class TrialController : MonoBehaviour
{
    [Header("References")]
    public GameController GC;
    public DataManager DM;
    public PlayerController PC;

    [Header("UI")]

    public GameObject initUIPrefab;

    [HideInInspector] public GameObject activeInitUI;




    private TrialStateBase currentState;
    private int currentTrialIndex = 0;

    void Start()
    {
        GC = FindFirstObjectByType<GameController>();
        DM = FindFirstObjectByType<DataManager>();
        PC = FindFirstObjectByType<PlayerController>();

        ChangeState(new TrialInitState(this));
    }

    void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(TrialStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    // For other states to access
    public int CurrentTrialIndex => currentTrialIndex;
    public void IncrementTrialIndex() => currentTrialIndex++;
}
