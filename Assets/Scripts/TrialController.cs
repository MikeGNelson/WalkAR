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
    public GameObject startUIPrefab;

    [HideInInspector] public GameObject activeInitUI;
    [HideInInspector] public GameObject activeStartUI;




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


    public void SpawnStartUI()
    {
        if (activeStartUI != null)
            Destroy(activeInitUI);

        activeStartUI = Instantiate(startUIPrefab);

        var ui = activeStartUI.GetComponent<TrialStartUI>();

        var order = GC.GenerateLatinSquareOrder(GC.UID);

        ui.Initialize(GC, this, GC.UID, order, GC.currentConditionIndex);

        // assign head for smooth follow
        ui.head = DM.ET.playerHead;
    }

    public void SetTrialIndex(int value)
    {
        currentTrialIndex = Mathf.Clamp(value, 0, GC.GenerateLatinSquareOrder(GC.UID).Count - 1);

        if (activeStartUI != null)
            activeStartUI.GetComponent<TrialStartUI>().UpdateIndex(currentTrialIndex);
    }

    public void DestroyStartUI()
    {
        if (activeStartUI != null)
        {
            Destroy(activeStartUI);
            activeStartUI = null;
        }
    }

    // For other states to access
    public int CurrentTrialIndex => currentTrialIndex;
    public void IncrementTrialIndex() => currentTrialIndex++;
}
