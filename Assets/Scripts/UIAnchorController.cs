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
    public Color normalColor = Color.black;
    public Color highlightColor = Color.green;

    [Header("Scaling Settings")]
    public float baseDistance = 1.5f;   // Reference distance for scale = 1
    public float baseScale = 1.0f;      // Scale at baseDistance
    public float minScale = 0.8f;       // Minimum allowed scale
    public float maxScale = 2.0f;       // Maximum allowed scale

    [Header("Settings")]
    public bool followHead = true;  // false = follow torso
    public bool offsetRight = false;
    public float followDistance = 1.5f;
    public float torsoSmoothing = 0.9f;
    public float rightOffsetAmount = 0.5f;

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
            {
                // fallback to right controller forward
                //forward = new Vector3(-leftController.forward.x, 0, leftController.forward.z).normalized;
                Vector3 chestForward = -leftController.up; // up points away from chest
                forward = new Vector3(chestForward.x, 0, chestForward.z).normalized;
            }
                
        }

        // Compute position + offset
        Vector3 basePos = anchor.position + forward * followDistance;
        Vector3 rightOffset = offsetRight ? Vector3.Cross(Vector3.up, forward).normalized * rightOffsetAmount : Vector3.zero;
        Vector3 targetPos = basePos + rightOffset + Vector3.up * 0.25f;

        // Smooth follow
        uiCanvas.transform.position = Vector3.Lerp(uiCanvas.transform.position, targetPos, Time.deltaTime * 8f);

        // Face toward player
        Vector3 lookDir = uiCanvas.transform.position - playerHead.position;
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            uiCanvas.transform.rotation = Quaternion.Slerp(uiCanvas.transform.rotation, targetRot, Time.deltaTime * 8f);
        }

        // === Distance-based scaling ===
        float distance = Vector3.Distance(playerHead.position, uiCanvas.transform.position);


        // scale grows with distance
        float scaleFactor = baseScale * (distance / baseDistance);
        scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);

        // smooth scaling
        uiCanvas.transform.localScale = Vector3.Lerp(
            uiCanvas.transform.localScale,
            Vector3.one * scaleFactor,
            Time.deltaTime * 6f
        );
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
    private float lastInputTime = 0f;
    private const float inputCooldown = 0.25f;

    void UpdateInput()
    {
        Vector2 rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        bool rightTriggerPressed = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        bool leftTriggerPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);

        const float stickThreshold = 0.5f;

        bool horizontalDominant = Mathf.Abs(rightStick.x) > Mathf.Abs(rightStick.y);

        // Handle thumbstick directional input with cooldown
        if (Time.time - lastInputTime > inputCooldown)
        {
            //if (rightStick.x < -stickThreshold) // Left → A
            //{
            //    currentSelection = 0;
            //    lastInputTime = Time.time;
            //}
            //else if (Mathf.Abs(rightStick.y) > stickThreshold) // Up/Down → B
            //{
            //    currentSelection = 1;
            //    lastInputTime = Time.time;
            //}
            //else if (rightStick.x > stickThreshold) // Right → Both
            //{
            //    currentSelection = 2;
            //    lastInputTime = Time.time;
            //}
            if (horizontalDominant)
            {
                if (rightStick.x < -0.5f) currentSelection = 0; // Left → A
                else if (rightStick.x > 0.5f) currentSelection = 2; // Right → Both
            }
            else
            {
                if (Mathf.Abs(rightStick.y) > 0.5f) currentSelection = 1; // Up/Down → B
            }

            lastInputTime = Time.time;
        }

        // Highlight current selection
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var button = optionButtons[i];
            var txt = button.GetComponentInChildren<TextMeshProUGUI>();
            var outline = button.GetComponent<Outline>();
            bool selected = (i == currentSelection);

            // Visual highlight: text color + bold font
            if (txt != null)
            {
                txt.color = selected ? highlightColor : normalColor;
                txt.overrideColorTags = true;
                txt.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }

            // Enable outline if selected
            if (outline != null)
                outline.enabled = selected;

            // Apply scale change for selected button
            var rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = selected ? Vector3.one * 1.15f : Vector3.one; // Grow slightly when selected
            }
        }

        // Right trigger → submit
        if (rightTriggerPressed && !questionAnswered)
        {
            string answerLabel = optionButtons[currentSelection]
                .GetComponentInChildren<TextMeshProUGUI>().text;
            SubmitAnswer(answerLabel);
        }

        // Left trigger → end trial
        if (leftTriggerPressed)
        {
            Debug.Log("[UIAnchorController] Left trigger pressed — ending trial.");
            //dataManager.LogEvent("TrialEndedViaLeftTrigger");
            // Optionally notify your TrialController here:
            // controller.ChangeState(new TrialEndState(controller));
        }
    }




    void GenerateNewQuestion()
    {
        questionAnswered = false;
        questionSpawnTime = Time.time;
        //currentSelection = 0;

        // Randomly choose which question(s) will be correct:
        // 0 = only A true, 1 = only B true, 2 = both true
        int truthType = Random.Range(0, 3);

        // Generate operands that guarantee sums < 100
        int a1 = Random.Range(10, 90);
        int b1 = Random.Range(10, 90 - a1); // ensures a1 + b1 < 100

        int a2 = Random.Range(10, 90);
        int b2 = Random.Range(10, 90 - a2); // ensures a2 + b2 < 100

        int correctSumA = a1 + b1;
        int correctSumB = a2 + b2;

        int resultA, resultB;
        bool eqA_correct, eqB_correct;

        switch (truthType)
        {
            case 0: // only A is true
                eqA_correct = true;
                eqB_correct = false;
                resultA = correctSumA;
                // create wrong but still <100
                do { resultB = correctSumB + Random.Range(-10, 11); }
                while (resultB == correctSumB || resultB >= 100 || resultB < 0);
                correctAnswer = "A";
                break;

            case 1: // only B is true
                eqA_correct = false;
                eqB_correct = true;
                do { resultA = correctSumA + Random.Range(-10, 11); }
                while (resultA == correctSumA || resultA >= 100 || resultA < 0);
                resultB = correctSumB;
                correctAnswer = "B";
                break;

            default: // both true
                eqA_correct = true;
                eqB_correct = true;
                resultA = correctSumA;
                resultB = correctSumB;
                correctAnswer = "A&B";
                break;
        }

        // Format text for display
        questionA = $"A. {a1} + {b1} = {resultA}";
        questionB = $"B. {a2} + {b2} = {resultB}";
        questionText.text = $"{questionA}\n{questionB}";

        // Update button labels
        optionButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "A";
        optionButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "B";
        optionButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = "Both";

        Debug.Log($"[UIAnchorController] New Question | TruthType:{truthType} | Correct:{correctAnswer}");
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

        GenerateNewQuestion();
        nextQuestionTime = Time.time + questionInterval;
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
