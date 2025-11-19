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

    public Color outlineNormalColor = Color.black;
    public Color outlineHighlightColor = Color.green;

    public TextMeshProUGUI countdownText;

    [Header("Scaling Settings")]
    public float baseDistance = 1.5f;   // reference distance for scale = 1
    public float baseScale = 0.01f;     // scale at baseDistance
    public float minScale = 0.01f;      // minimum allowed scale
    public float maxScale = 0.06f;      // maximum allowed scale

    [Header("Settings")]
    public bool followHead = true;      // false = follow torso / left controller
    public bool offsetRight = false;
    public float followDistance = 1.5f;
    public float torsoSmoothing = 0.9f;
    public float rightOffsetAngle = 10f; // visual right offset angle in degrees
    public float baseYawOffset = 5f;     // left offset in degrees (torso / left controller only)

    public float pitchOffsetDegrees = 20f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothSpeed = 10f;
    public float scaleSmoothSpeed = 10f;

    private Vector3 smoothedPos;
    private Vector3 positionVelocity;
    private Quaternion smoothedRot;
    private float currentScale = 1f;

    [Header("Task Timing")]
    public float questionInterval = 5f;
    private float nextQuestionTime;

    private int currentSelection = 0;
    private string questionA, questionB;
    private float questionSpawnTime;
    private bool questionAnswered = false;
    private string correctAnswer;

    [Header("Question Fade")]
    public float questionFadeDuration = 0.3f;
    private Queue<Vector3> movementHistory = new Queue<Vector3>();
    private int historyLength = 30;
    private Vector3 avgDirection = Vector3.forward;

    private Coroutine questionFadeRoutine;

    void Start()
    {
        nextQuestionTime = Time.time + questionInterval;
        GenerateNewQuestion();

        if (uiCanvas != null)
        {
            smoothedPos = uiCanvas.transform.position;
            smoothedRot = uiCanvas.transform.rotation;
            currentScale = uiCanvas.transform.localScale.x;
        }
    }

    void Update()
    {
        UpdateAnchor();
        UpdateInput();
        UpdateCountdownUI();

        if (Time.time >= nextQuestionTime)
        {
            HandleQuestionTimeout();
        }
    }

    public void ApplyCondition(DataManager.Conditons condition)
    {
        // reset defaults
        followHead = true;
        offsetRight = false;
        followDistance = 1.5f;

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

        Debug.Log($"[UIAnchorController] Applied Condition: {condition} " +
                  $"→ HeadFollow:{followHead}, RightOffset:{offsetRight}, Dist:{followDistance}");
    }

    // -------------------------------------------------------------------
    // Anchor logic with torso-only base yaw offset + optional right offset
    // -------------------------------------------------------------------
    void UpdateAnchor()
    {
        if (uiCanvas == null || playerHead == null)
            return;

        // anchor: head or torso
        Transform anchor = followHead ? playerHead : playerBody;

        // forward direction
        Vector3 forward;
        if (followHead)
        {
            Vector3 flatForward = new Vector3(playerHead.forward.x, 0f, playerHead.forward.z).normalized;
            forward = flatForward;
        }
        else
        {
            Transform torso = GetMetaTorso();
            if (torso != null)
            {
                forward = new Vector3(torso.forward.x, 0f, torso.forward.z).normalized;
            }
            else
            {
                Vector3 chestForward = -leftController.up;
                forward = new Vector3(chestForward.x, 0f, chestForward.z).normalized;
            }

            // apply base yaw offset only when following torso / left controller
            if (Mathf.Abs(baseYawOffset) > 0.0001f)
            {
                forward = Quaternion.AngleAxis(-baseYawOffset, Vector3.up) * forward;
            }
        }

        Vector3 anchorPos = anchor.position;

        // base position in front of anchor
        Vector3 basePos = anchorPos + forward * followDistance;

        // right offset by fixed visual angle (same angle for all right-offset conditions)
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        float lateralOffset = 0f;
        if (offsetRight)
        {
            float angleRad = rightOffsetAngle * Mathf.Deg2Rad;
            lateralOffset = Mathf.Tan(angleRad) * followDistance;
        }

        // vertical offset:
        // right-offset: -0.2, others: -0.5
        Vector3 verticalOffset = offsetRight ? Vector3.up * -0.2f : Vector3.up * -0.5f;

        Vector3 targetPos = basePos + right * lateralOffset + verticalOffset;

        // keep radius fixed, smooth only direction
        Vector3 desiredOffset = targetPos - anchorPos;
        if (desiredOffset.sqrMagnitude < 0.0001f)
        {
            desiredOffset = forward * followDistance;
        }

        float radius = desiredOffset.magnitude;
        Vector3 desiredDir = desiredOffset / radius;

        Vector3 currentOffset = smoothedPos - anchorPos;
        Vector3 currentDir;
        if (currentOffset.sqrMagnitude < 0.0001f)
        {
            currentDir = desiredDir;
        }
        else
        {
            currentDir = currentOffset.normalized;
        }

        float dirLerp = Mathf.Clamp01(Time.deltaTime / Mathf.Max(positionSmoothTime, 0.0001f));
        Vector3 newDir = Vector3.Slerp(currentDir, desiredDir, dirLerp);

        smoothedPos = anchorPos + newDir * radius;
        uiCanvas.transform.position = smoothedPos;

        // rotation
        Quaternion targetRot;
        if (!offsetRight)
        {
            Vector3 lookDir = smoothedPos - playerHead.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion baseRot = Quaternion.LookRotation(lookDir, Vector3.up);
                Quaternion pitchOffset = Quaternion.AngleAxis(pitchOffsetDegrees, Vector3.right);
                targetRot = baseRot * pitchOffset;
            }
            else
            {
                targetRot = uiCanvas.transform.rotation;
            }
        }
        else
        {
            if (forward.sqrMagnitude > 0.001f)
            {
                targetRot = Quaternion.LookRotation(forward, Vector3.up);
            }
            else
            {
                targetRot = uiCanvas.transform.rotation;
            }
        }

        float rotLerp = 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);
        smoothedRot = Quaternion.Slerp(smoothedRot, targetRot, rotLerp);
        uiCanvas.transform.rotation = smoothedRot;

        // distance-based scaling
        float distance = Vector3.Distance(playerHead.position, smoothedPos);
        float targetScale = baseScale * (distance / baseDistance);
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        float scaleLerp = 1f - Mathf.Exp(-scaleSmoothSpeed * Time.deltaTime);
        currentScale = Mathf.Lerp(currentScale, targetScale, scaleLerp);
        uiCanvas.transform.localScale = Vector3.one * currentScale;
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
    // INPUT + QUESTION LOGIC (unchanged except SubmitAnswer timing)
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

        if (Time.time - lastInputTime > inputCooldown)
        {
            bool selectionChanged = false;

            if (horizontalDominant)
            {
                if (rightStick.x < stickThreshold * -1f)
                {
                    if (currentSelection != 0)
                    {
                        currentSelection = 0;
                        selectionChanged = true;
                    }
                }
                else if (rightStick.x > stickThreshold)
                {
                    if (currentSelection != 2)
                    {
                        currentSelection = 2;
                        selectionChanged = true;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(rightStick.y) > stickThreshold)
                {
                    if (currentSelection != 1)
                    {
                        currentSelection = 1;
                        selectionChanged = true;
                    }
                }
            }

            if (selectionChanged)
            {
                lastInputTime = Time.time;
                ShortMoveHaptic();
            }
        }

        // highlight current selection
        for (int i = 0; i < optionButtons.Count; i++)
        {
            var button = optionButtons[i];
            var txt = button.GetComponentInChildren<TextMeshProUGUI>();
            var outline = button.GetComponent<Outline>();
            bool selected = (i == currentSelection);

            if (txt != null)
            {
                txt.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }

            if (outline != null)
            {
                outline.enabled = true;
                outline.effectColor = selected ? outlineHighlightColor : outlineNormalColor;
            }

            var rect = button.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
            }
        }

        // right trigger → submit (question changes only on timeout)
        if (rightTriggerPressed && !questionAnswered)
        {
            string answerLabel = optionButtons[currentSelection]
                .GetComponentInChildren<TextMeshProUGUI>().text;
            SubmitAnswer(answerLabel);
        }

        // left trigger → end trial
        if (leftTriggerPressed)
        {
            Debug.Log("[UIAnchorController] Left trigger pressed — ending trial.");
        }
    }

    void UpdateCountdownUI()
    {
        if (countdownText == null)
            return;

        float remaining = nextQuestionTime - Time.time;

        if (remaining < 0f)
            remaining = 0f;

        int seconds = Mathf.CeilToInt(remaining);

        if (seconds <= 0)
        {
            countdownText.text = "";
        }
        else
        {
            countdownText.text = seconds.ToString();
        }
    }

    void HandleQuestionTimeout()
    {
        try
        {
            if (!questionAnswered)
            {
                LogIfUnanswered();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UIAnchorController] AddMathTask error in HandleQuestionTimeout: " + e.Message);
        }

        GenerateNewQuestion();
        nextQuestionTime = Time.time + questionInterval;
    }

    void SetQuestionTextWithFade(string newText)
    {
        if (questionText == null)
            return;

        if (questionFadeRoutine != null)
        {
            StopCoroutine(questionFadeRoutine);
        }

        questionFadeRoutine = StartCoroutine(FadeQuestionTextCoroutine(newText));
    }

    IEnumerator FadeQuestionTextCoroutine(string newText)
    {
        if (questionText == null)
            yield break;

        float duration = questionFadeDuration;
        if (duration <= 0f)
        {
            questionText.text = newText;
            yield break;
        }

        Color c = questionText.color;

        float t = 0f;
        float startAlpha = c.a;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, t / duration);
            c.a = alpha;
            questionText.color = c;
            yield return null;
        }

        c.a = 0f;
        questionText.color = c;
        questionText.text = newText;

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration);
            c.a = alpha;
            questionText.color = c;
            yield return null;
        }

        c.a = 1f;
        questionText.color = c;

        questionFadeRoutine = null;
    }

    void GenerateNewQuestion()
    {
        questionAnswered = false;
        questionSpawnTime = Time.time;

        int truthType = Random.Range(0, 3);

        int a1 = Random.Range(10, 90);
        int b1 = Random.Range(10, 90 - a1);

        int a2 = Random.Range(10, 90);
        int b2 = Random.Range(10, 90 - a2);

        int correctSumA = a1 + b1;
        int correctSumB = a2 + b2;

        int resultA, resultB;
        bool eqA_correct, eqB_correct;

        switch (truthType)
        {
            case 0:
                eqA_correct = true;
                eqB_correct = false;
                resultA = correctSumA;
                do { resultB = correctSumB + Random.Range(-10, 11); }
                while (resultB == correctSumB || resultB >= 100 || resultB < 0);
                correctAnswer = "A";
                break;

            case 1:
                eqA_correct = false;
                eqB_correct = true;
                do { resultA = correctSumA + Random.Range(-10, 11); }
                while (resultA == correctSumA || resultA >= 100 || resultA < 0);
                resultB = correctSumB;
                correctAnswer = "B";
                break;

            default:
                eqA_correct = true;
                eqB_correct = true;
                resultA = correctSumA;
                resultB = correctSumB;
                correctAnswer = "A&B";
                break;
        }

        questionA = $"A. {a1} + {b1} = {resultA}";
        questionB = $"B. {a2} + {b2} = {resultB}";

        string newText = $"{questionA}\n{questionB}";
        SetQuestionTextWithFade(newText);

        optionButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "A";
        optionButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "B";
        optionButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = "Both";

        Debug.Log($"[UIAnchorController] New Question | TruthType:{truthType} | Correct:{correctAnswer}");
    }

    void SubmitAnswer(string answerLabel)
    {
        if (questionAnswered) return;

        Debug.Log("[UIAnchorController] SubmitAnswer: " + answerLabel);

        ConfirmHaptic();
        questionAnswered = true;
        float answerTime = Time.time;

        bool correct = (answerLabel.Contains("A") && correctAnswer.Contains("A")) ||
                       (answerLabel.Contains("B") && correctAnswer.Contains("B"));

        try
        {
            if (dataManager != null)
            {
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
            else
            {
                Debug.LogError("[UIAnchorController] dataManager is NULL in SubmitAnswer");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[UIAnchorController] AddMathTask error in SubmitAnswer: " + e.Message);
        }

        // question will change only in HandleQuestionTimeout
    }

    void LogIfUnanswered()
    {
        if (!questionAnswered)
        {
            try
            {
                if (dataManager != null)
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
                else
                {
                    Debug.LogError("[UIAnchorController] dataManager is NULL in LogIfUnanswered");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[UIAnchorController] AddMathTask error in LogIfUnanswered: " + e.Message);
            }
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

    private IEnumerator HapticPulse(float amplitude, float duration, OVRInput.Controller controller)
    {
        OVRInput.SetControllerVibration(0.5f, amplitude, controller);
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    private void ShortMoveHaptic()
    {
        StartCoroutine(HapticPulse(0.3f, 0.05f, OVRInput.Controller.RTouch));
    }

    private void ConfirmHaptic()
    {
        StartCoroutine(HapticPulse(0.9f, 0.2f, OVRInput.Controller.RTouch));
    }
}
