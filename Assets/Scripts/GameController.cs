using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("Prefabs & References")]
    public List<GameObject> peoplePrefabs;
    public Transform midPoint;
    public Transform startPoint;
    public Transform endPoint;
    public GameObject vrRig;
    public TextMeshProUGUI promptText;

    [Header("Runtime")]
    public List<GameObject> modelsList = new List<GameObject>();
    public bool isRecording = false;

    [Header("Experiment Control")]
    public int UID = 0;
    private int currentConditionIndex = 0;
    private List<DataManager.Conditons> conditionOrder;
    private DataManager dataManager;
    private Avoidance.PlayerController playerController;

    private void Start()
    {
        dataManager = FindObjectOfType<DataManager>();
        playerController = FindObjectOfType<Avoidance.PlayerController>();
        promptText.text = "Enter UID to begin.";
    }

    /// <summary>
    /// Called when the proctor enters the UID before starting.
    /// </summary>
    public void InitializeExperiment(int enteredUID)
    {
        UID = enteredUID;
        conditionOrder = GenerateLatinSquareOrder(UID);
        currentConditionIndex = 0;
        promptText.text = $"Experiment ready. Press 'Start Trial' to begin Condition 1 of {conditionOrder.Count}.";
    }

    /// <summary>
    /// Called by the proctor when they press “Start Trial”.
    /// </summary>
    public void StartTrial()
    {
        if (conditionOrder == null || conditionOrder.Count == 0)
        {
            Debug.LogWarning("Experiment not initialized. Please enter UID first.");
            return;
        }

        if (currentConditionIndex >= conditionOrder.Count)
        {
            promptText.text = "All conditions complete. Thank you!";
            return;
        }

        // Clear old models and set up for new trial
        ClearModels();

        var currentCondition = conditionOrder[currentConditionIndex];
        dataManager.conditions = currentCondition;

        // Spawn the appropriate model for this condition
        SpawnModelForCondition(currentCondition);

        // Reset player state
        playerController.stopWriting = false;
        isRecording = true;

        // Start recording
        promptText.text = $"Condition {currentConditionIndex + 1} / {conditionOrder.Count}\nWalk to the target.";
        dataManager.SendCondition();
    }

    /// <summary>
    /// Called automatically when participant reaches end point (via PlayerController),
    /// or can be called manually by the proctor if needed.
    /// </summary>
    public void EndTrial()
    {
        if (!isRecording)
            return;

        isRecording = false;
        dataManager.WriteData();

        currentConditionIndex++;

        if (currentConditionIndex < conditionOrder.Count)
        {
            promptText.text = $"Condition complete.\nReturn to start, then press 'Start Trial' for condition {currentConditionIndex + 1}.";
        }
        else
        {
            promptText.text = "All conditions complete. Thank you!";
        }
    }

    private void SpawnModelForCondition(DataManager.Conditons condition)
    {
        int modelIndex = (int)condition;
        if (modelIndex >= 0 && modelIndex < peoplePrefabs.Count)
        {
            GameObject model = Instantiate(
                peoplePrefabs[modelIndex],
                midPoint.position,
                Quaternion.LookRotation(startPoint.position - midPoint.position)
            );
            modelsList.Add(model);
        }
        else
        {
            Debug.LogWarning($"No prefab for condition index {modelIndex}. Check peoplePrefabs list.");
        }
    }

    private void ClearModels()
    {
        foreach (var model in modelsList)
        {
            if (model != null)
                Destroy(model);
        }
        modelsList.Clear();
    }

    private List<DataManager.Conditons> GenerateLatinSquareOrder(int uid)
    {
        var baseOrder = new List<DataManager.Conditons>
        {
            DataManager.Conditons.Center_Head_Close,
            DataManager.Conditons.Center_Head_Far,
            DataManager.Conditons.Center_Dir_Close,
            DataManager.Conditons.Center_Dir_Far,
            DataManager.Conditons.Right_Head_Close,
            DataManager.Conditons.Right_Head_Far,
            DataManager.Conditons.Right_Dir_Close,
            DataManager.Conditons.Right_Dir_Far
        };

        int n = baseOrder.Count;
        List<DataManager.Conditons> order = new List<DataManager.Conditons>();
        for (int i = 0; i < n; i++)
        {
            int index = (i + uid) % n;
            order.Add(baseOrder[index]);
        }
        return order;
    }
}
