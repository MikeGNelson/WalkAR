using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrialController : MonoBehaviour
{
    [Header("References")]
    public GameController GC;
    public DataManager DM;
    public GameObject vrRig;

    [Header("UI Elements")]
    public Canvas controlCanvas;           // Canvas for UID + control buttons
    public TMP_InputField uidInputField;   // Field to enter participant ID
    public Button startButton;             // Button to initialize experiment
    public Button nextTrialButton;         // Button to advance to next trial
    public TextMeshProUGUI statusText;     // Displays experiment progress

    private bool experimentInitialized = false;

    void Start()
    {
        GC = FindObjectOfType<GameController>();
        DM = FindObjectOfType<DataManager>();

        // Hide Next Trial button until after UID entered
        nextTrialButton.interactable = false;

        // Hook up button events
        startButton.onClick.AddListener(OnStartExperimentClicked);
        nextTrialButton.onClick.AddListener(OnNextTrialClicked);
    }

    void Update()
    {
        // If the experiment is initialized and the current trial finished, enable Next Trial
        if (experimentInitialized && !GC.isRecording && nextTrialButton != null)
        {
            // Enable next trial when player returns to start position
            bool ready = Vector3.Distance(GC.playerController.transform.position, GC.startPoint.position) < 1.0f;
            nextTrialButton.interactable = ready;
        }
    }

    private void OnStartExperimentClicked()
    {
        if (string.IsNullOrEmpty(uidInputField.text))
        {
            statusText.text = "Please enter a valid UID.";
            return;
        }

        int uid = int.Parse(uidInputField.text);
        DM.UId = uid;
        GC.InitializeExperiment(uid);
        experimentInitialized = true;

        statusText.text = $"Experiment initialized for UID {uid}. Ready for first trial.";
        nextTrialButton.interactable = true; // Allow starting first trial
    }

    private void OnNextTrialClicked()
    {
        if (!experimentInitialized)
        {
            statusText.text = "Please initialize experiment first.";
            return;
        }

        //// Run the next condition trial
        //statusText.text = $"Starting trial {GC.currentConditionIndex + 1} / {GC.conditionOrder.Count}...";
        GC.StartTrial();

        nextTrialButton.interactable = false; // disable until trial completes
    }

    public void ResetPosition()
    {
        Debug.Log("Resetting player position...");
        Transform rigRoot = vrRig.transform;
        Transform startPos = GameObject.FindGameObjectWithTag("StartPosition").transform;
        Transform endPos = GameObject.FindGameObjectWithTag("EndPosition").transform;

        rigRoot.position = startPos.position;
        Vector3 lookDir = (endPos.position - startPos.position);
        lookDir.y = 0;
        rigRoot.rotation = Quaternion.LookRotation(lookDir.normalized);

        statusText.text = "Player position reset to start.";
    }
}
