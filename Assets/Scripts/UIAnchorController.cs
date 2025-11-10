using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIAnchorController : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;
    public Transform playerBody; // can use PlayerController.transform
    public Transform leftController;
    public DataManager dataManager;

    [Header("UI Elements")]
    public Canvas uiCanvas;
    public TextMeshProUGUI questionText;
    public List<Button> optionButtons; // 0:A, 1:B, 2:Both
    public Color normalColor = Color.white;
    public Color highlightColor = Color.green;

    [Header("Settings")]
    public bool followHead = true;  // false = follow torso
    public bool offsetRight = false;
    public float followDistance = 1.5f;
    public float torsoSmoothing = 0.9f;

    [Header("Task Timing")]
    public float questionInterval = 5f;
    private float nextQuestionTime;

    private int currentSelection = 0;
    private string questionA, questionB;
    private float questionSpawnTime;
    private bool questionAnswered = false;
    private string correctAnswer;

    // For torso estimation
    private Queue<Vector3> movementHistory = new Queue<Vector3>();
    private int historyLength = 30;
    private Vector3 avgDirection = Vector3.forward;

    void Start()
    {
        nextQuestionTime = Time.time + questionInterval;
        GenerateNewQuestion();
    }

    void Update()
    {
        UpdateAnchor();
        UpdateInput();

        if (Time.time >= nextQuestionTime)
        {
            LogIfUnanswered();
            GenerateNewQuestion();
            nextQuestionTime = Time.time + questionInterval;
        }
    }

    public void ApplyCondition(DataManager.Conditons condition)
    {
        // Reset defaults
        followHead = true;
        offsetRight = false;
        followDistance = 1.5f;

        // Parse enum
        switch (condition)
        {
            case DataManager.Conditons.Center_Head_Close:
                followHead = true;
                offsetRight = false;
                followDistance = 1.5f;
                break;

            case DataManager.Conditons.Center_Head_Far:
                followHead = true;
                offsetRight = false;
                followDistance = 3.3f;
                break;

            case DataManager.Conditons.Center_Dir_Close:
                followHead = false;   // torso direction
                offsetRight = false;
                followDistance = 1.5f;
                break;

            case DataManager.Conditons.Center_Dir_Far:
                followHead = false;
                offsetRight = false;
                followDistance = 3.3f;
                break;

            case DataManager.Conditons.Right_Head_Close:
                followHead = true;
                offsetRight = true;
                followDistance = 1.5f;
                break;

            case DataManager.Conditons.Right_Head_Far:
                followHead = true;
                offsetRight = true;
                followDistance = 3.3f;
                break;

            case DataManager.Conditons.Right_Dir_Close:
                followHead = false;
                offsetRight = true;
                followDistance = 1.5f;
                break;

            case DataManager.Conditons.Right_Dir_Far:
                followHead = false;
                offsetRight = true;
                followDistance = 3.3f;
                break;
        }

        // Optional debug feedback
        Debug.Log($"[UIAnchorController] Applied Condition: {condition} " +
                  $"→ HeadFollow:{followHead}, RightOffset:{offsetRight}, Dist:{followDistance}");
    }

    // -------------------------------------------------------------------
    // UPDATED ANCHOR LOGIC (smooth follow + torso tracking)
    // -------------------------------------------------------------------
    void UpdateAnchor()
{
    if (uiCanvas == null || playerHead == null)
        return;

    // Which transform to base from
    Transform anchor = followHead ? playerHead : playerBody;

    // Decide forward vector
    Vector3 forward;
    if (followHead)
    {
        // smooth yaw-only head direction
        Vector3 flatForward = new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized;
        forward = flatForward;
    }
    else
    {
        // try Meta torso joint first
        Transform torso = GetMetaTorso();
        if (torso != null)
            forward = new Vector3(torso.forward.x, 0, torso.forward.z).normalized;
        else
            // fallback to right controller forward
            forward = new Vector3(leftController.forward.x, 0, leftController.forward.z).normalized;
    }

    // compute position + offset
    Vector3 basePos = anchor.position + forward * followDistance;
    Vector3 rightOffset = offsetRight ? Vector3.Cross(Vector3.up, forward).normalized * 0.5f : Vector3.zero;
    Vector3 targetPos = basePos + rightOffset + Vector3.up * 0.25f;

    // smooth follow
    uiCanvas.transform.position = Vector3.Lerp(uiCanvas.transform.position, targetPos, Time.deltaTime * 8f);

    // face toward player
    Vector3 lookDir = uiCanvas.transform.position - playerHead.position;
    lookDir.y = 0;
    if (lookDir.sqrMagnitude > 0.001f)
    {
        Quaternion targetRot = Quaternion.LookRotation(lookDir);
        uiCanvas.transform.rotation = Quaternion.Slerp(uiCanvas.transform.rotation, targetRot, Time.deltaTime * 8f);
    }
}



    // -------------------------------------------------------------------
    // Torso tracking helper (Meta XR Body Tracking)
    // -------------------------------------------------------------------
    Transform GetMetaTorso()
    {
#if META_XR_AVAILABLE
        if (Meta.XR.BodyTracking.MetaBody.Instance != null &&
            Meta.XR.BodyTracking.MetaBody.Instance.TryGetJointTransform(
                Meta.XR.BodyTracking.BodyJointType.SpineLower, out Transform torso))
        {
            return torso;
        }
#endif
        return null;
    }

    // -------------------------------------------------------------------
    // INPUT + QUESTION LOGIC (unchanged)
    // -------------------------------------------------------------------
    void UpdateInput()
    {
        float h = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;   // ✅ right stick
        bool triggerPressed = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger); // ✅ right trigger

        if (h > 0.5f) currentSelection = (currentSelection + 1) % optionButtons.Count;
        else if (h < -0.5f) currentSelection = (currentSelection - 1 + optionButtons.Count) % optionButtons.Count;

        // highlight feedback (bold + scale pulse)
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var txt = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt == null) continue;

            bool selected = (i == currentSelection);
            txt.color = selected ? highlightColor : normalColor;
            txt.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            optionButtons[i].transform.localScale = Vector3.Lerp(
                optionButtons[i].transform.localScale,
                selected ? Vector3.one * 1.15f : Vector3.one,
                Time.deltaTime * 10f
            );
        }

        if (triggerPressed && !questionAnswered)
        {
            string answerLabel = optionButtons[currentSelection]
                .GetComponentInChildren<TextMeshProUGUI>().text;
            SubmitAnswer(answerLabel);
        }
    }


    void GenerateNewQuestion()
    {
        questionAnswered = false;
        questionSpawnTime = Time.time;
        currentSelection = 0;

        // Generate two random addition problems
        int a1 = Random.Range(10, 50);
        int b1 = Random.Range(10, 50);
        int a2 = Random.Range(10, 50);
        int b2 = Random.Range(10, 50);

        // Randomly decide whether each equation is true or false
        bool eqA_correct = Random.value > 0.5f;
        bool eqB_correct = Random.value > 0.5f;

        int resultA = eqA_correct ? a1 + b1 : (a1 + b1 + Random.Range(-5, 6));
        int resultB = eqB_correct ? a2 + b2 : (a2 + b2 + Random.Range(-5, 6));

        questionA = $"A. {a1}+{b1}={resultA}";
        questionB = $"B. {a2}+{b2}={resultB}";

        // Determine the correct selection type
        if (eqA_correct && !eqB_correct) correctAnswer = "A";
        else if (!eqA_correct && eqB_correct) correctAnswer = "B";
        else if (eqA_correct && eqB_correct) correctAnswer = "A&B";
        else correctAnswer = "None"; // neither true — none of the options correct

        // Display both lines stacked
        questionText.text = $"{questionA}\n{questionB}";

        // Update visible labels for buttons
        optionButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "A";
        optionButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "B";
        optionButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = "Both";
    }

    void SubmitAnswer(string answerLabel)
    {
        questionAnswered = true;
        float answerTime = Time.time;

        bool correct = (answerLabel.Contains("A") && correctAnswer.Contains("A")) ||
                       (answerLabel.Contains("B") && correctAnswer.Contains("B"));

        dataManager.AddMathTask(
            questionSpawnTime,
            answerTime,
            $"{questionA} vs {questionB}",
            answerLabel,
            correctAnswer,
            correct,
            true
        );
    }

    void LogIfUnanswered()
    {
        if (!questionAnswered)
        {
            dataManager.AddMathTask(
                questionSpawnTime,
                Time.time,
                $"{questionA} vs {questionB}",
                "N/A",
                correctAnswer,
                false,
                false
            );
        }
    }

    public void OnButtonClicked(int index)
    {
        if (!questionAnswered)
        {
            var label = optionButtons[index].GetComponentInChildren<TextMeshProUGUI>().text;
            SubmitAnswer(label);
        }
    }
}
