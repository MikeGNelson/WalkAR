using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TrialInitState : TrialStateBase
{
    private GameObject uiInstance;
    private TMP_InputField uidField;
    private TextMeshProUGUI promptText;
    private Toggle loggingToggle;

    // A button long-press detection
    private bool aIsPressed = false;
    private float aPressStartTime = 0f;
    private float aHoldThreshold = 1.0f;   // seconds

    // Haptics
    private float hapticEndTime = 0f;

    public TrialInitState(TrialController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("[TrialInitState] Entered Init State");

        if (controller.initUIPrefab != null && gameController.vrRig != null)
        {
            Transform head = gameController.vrRig.transform;
            Vector3 spawnPos = head.position + head.forward * 2.2f + new Vector3(0, 1, 0);
            Quaternion spawnRot = Quaternion.LookRotation(spawnPos - head.position);

            uiInstance = Object.Instantiate(controller.initUIPrefab, spawnPos, spawnRot);
            controller.activeInitUI = uiInstance;

            uidField = uiInstance.GetComponentInChildren<TMP_InputField>();
            loggingToggle = uiInstance.GetComponentInChildren<Toggle>();
            promptText = uiInstance.GetComponentInChildren<TextMeshProUGUI>();

            if (promptText != null)
                promptText.text = "Set Participant UID";

            if (loggingToggle != null && dataManager != null)
                loggingToggle.isOn = dataManager.enableLogging;
        }
        else
        {
            Debug.LogWarning("[TrialInitState] No initUIPrefab assigned or missing VR Rig reference.");
        }
    }

    public override void Update()
    {
        // Keep UI facing the user
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

        // Sync toggle state to DataManager.enableLogging (in case of manual click)
        if (loggingToggle != null && dataManager != null)
        {
            dataManager.enableLogging = loggingToggle.isOn;
        }

        // Right controller B button: increment UID
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            uid++;
            uidField.text = uid.ToString();

            gameController.UID = uid;
            if (dataManager != null)
                dataManager.UId = uid;

            Debug.Log($"[TrialInitState] UID incremented to {uid}");
        }

        // Right controller A button: short press = decrement UID, long press = disable logging
        bool aDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
        bool aHeld = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
        bool aUp = OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch);

        if (aDown)
        {
            aIsPressed = true;
            aPressStartTime = Time.time;
        }

        if (aIsPressed && aUp)
        {
            aIsPressed = false;
            float heldTime = Time.time - aPressStartTime;

            if (heldTime >= aHoldThreshold)
            {
                // Long press: toggle logging on/off + haptic feedback
                bool newLoggingState = true;

                if (dataManager != null)
                {
                    newLoggingState = !dataManager.enableLogging;
                    dataManager.enableLogging = newLoggingState;
                    Debug.Log("[TrialInitState] Logging toggled via long press A. New state: " + newLoggingState);
                }
                else if (loggingToggle != null)
                {
                    newLoggingState = !loggingToggle.isOn;
                }

                if (loggingToggle != null)
                {
                    loggingToggle.isOn = newLoggingState;
                }

                // Haptic feedback for toggle
                hapticEndTime = Time.time + 0.2f;
                OVRInput.SetControllerVibration(0.5f, 0.9f, OVRInput.Controller.RTouch);
            }
            else
            {
                // Short press: decrement UID
                uid = Mathf.Max(0, uid - 1);
                uidField.text = uid.ToString();

                gameController.UID = uid;
                if (dataManager != null)
                    dataManager.UId = uid;

                Debug.Log($"[TrialInitState] UID decremented to {uid}");
            }
        }

        // Stop haptics when time is up
        if (hapticEndTime > 0f && Time.time >= hapticEndTime)
        {
            OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
            hapticEndTime = 0f;
        }

        // Confirm UID with trigger
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

            if (gameController == null || dataManager == null)
            {
                Debug.LogError("[TrialInitState] Missing controller/dataManager reference!");
                return;
            }

            gameController.InitializeExperiment(uid);
            controller.ChangeState(new TrialStartState(controller));
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

        OVRInput.SetControllerVibration(0f, 0f, OVRInput.Controller.RTouch);
    }
}
