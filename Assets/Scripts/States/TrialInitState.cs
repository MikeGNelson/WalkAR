using UnityEngine;
using TMPro;

public class TrialInitState : TrialStateBase
{
    private GameObject uiInstance;
    private TMP_InputField uidField;
    private TextMeshProUGUI promptText;

    public TrialInitState(TrialController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[TrialInitState] Entered Init State");

        if (controller.initUIPrefab != null && gameController.vrRig != null)
        {
            Transform head = gameController.vrRig.transform;
            Vector3 spawnPos = head.position + head.forward * 1.2f;
            Quaternion spawnRot = Quaternion.LookRotation(spawnPos - head.position);

            uiInstance = Object.Instantiate(controller.initUIPrefab, spawnPos, spawnRot);
            controller.activeInitUI = uiInstance; // 🔹 track globally

            uidField = uiInstance.GetComponentInChildren<TMP_InputField>();
            promptText = uiInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null) promptText.text = "Set Participant UID";
        }
        else
        {
            Debug.LogWarning("[TrialInitState] No initUIPrefab assigned or missing VR Rig reference.");
        }
    }

    public override void Update()
    {
        // Keep facing user
        if (uiInstance != null && gameController.vrRig != null)
        {
            Transform head = gameController.vrRig.transform;
            uiInstance.transform.LookAt(head);
            uiInstance.transform.rotation = Quaternion.LookRotation(uiInstance.transform.position - head.position);
        }

        if (uidField == null) return;

        int uid = 0;
        if (!string.IsNullOrEmpty(uidField.text))
            int.TryParse(uidField.text, out uid);

        // Flick Up
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickUp))
        {
            uid++;
            uidField.text = uid.ToString();

            gameController.UID = uid;
            if (dataManager != null)
                dataManager.UId = uid;

            Debug.Log($"[TrialInitState] UID incremented to {uid}");
        }

        // Flick Down
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstickDown))
        {
            uid = Mathf.Max(0, uid - 1);
            uidField.text = uid.ToString();

            gameController.UID = uid;
            if (dataManager != null)
                dataManager.UId = uid;

            Debug.Log($"[TrialInitState] UID decremented to {uid}");
        }

        // Confirm with trigger
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
            OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            Debug.Log("[TrialInitState] Trigger pressed – confirming UID.");

            if (uid <= 0)
            {
                if (promptText != null)
                    promptText.text = "Please enter a valid UID!";
                return;
            }

            // Safe initialize
            if (gameController == null || dataManager == null)
            {
                Debug.LogError("[TrialInitState] Missing controller/dataManager reference!");
                return;
            }

            gameController.InitializeExperiment(uid);

            controller.ChangeState(new TrialStartState(controller)); // ✅ no destroy here
        }
    }

    public override void Exit()
    {
        if (controller.activeInitUI != null)
        {
            Object.Destroy(controller.activeInitUI);
            controller.activeInitUI = null;
            Debug.Log("[TrialInitState] Cleaned up Init UI in Exit().");
        }
    }
}
