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


    void UpdateAnchor()
    {
        if (followHead)
        {
            Vector3 forward = new Vector3(playerHead.forward.x, 0, playerHead.forward.z).normalized;
            Vector3 basePos = playerHead.position + forward * followDistance;
            Vector3 rightOffset = offsetRight ? Vector3.Cross(Vector3.up, forward).normalized * 0.5f : Vector3.zero;
            uiCanvas.transform.position = basePos + rightOffset;
            uiCanvas.transform.LookAt(playerHead.position);
            uiCanvas.transform.rotation = Quaternion.LookRotation(uiCanvas.transform.position - playerHead.position);
        }
        else
        {
            // Torso-follow using left controller direction
            Vector3 forward = new Vector3(leftController.forward.x, 0, leftController.forward.z).normalized;
            Vector3 basePos = playerBody.position + forward * followDistance;
            Vector3 rightOffset = offsetRight ? Vector3.Cross(Vector3.up, forward).normalized * 0.5f : Vector3.zero;

            uiCanvas.transform.position = basePos + rightOffset;
            uiCanvas.transform.rotation = Quaternion.LookRotation(forward);
        }

        //else
        //{
        //    // Compute smoothed torso heading
        //    Vector3 velocity = playerBody.GetComponent<Rigidbody>() != null ?
        //                       playerBody.GetComponent<Rigidbody>().linearVelocity :
        //                       playerBody.forward;
        //    if (velocity.magnitude > 0.01f)
        //    {
        //        movementHistory.Enqueue(velocity.normalized);
        //        if (movementHistory.Count > historyLength)
        //            movementHistory.Dequeue();

        //        Vector3 avg = Vector3.zero;
        //        foreach (var v in movementHistory)
        //            avg += v;
        //        avgDirection = Vector3.Lerp(avgDirection, avg.normalized, 1f - torsoSmoothing);
        //    }

        //    Vector3 forward = new Vector3(avgDirection.x, 0, avgDirection.z).normalized;
        //    Vector3 basePos = playerBody.position + forward * followDistance;
        //    Vector3 rightOffset = offsetRight ? Vector3.Cross(Vector3.up, forward).normalized * 0.5f : Vector3.zero;
        //    uiCanvas.transform.position = basePos + rightOffset;
        //    uiCanvas.transform.rotation = Quaternion.LookRotation(forward);
        //}
    }

    void UpdateInput()
    {
        float h = Input.GetAxis("Oculus_CrossPlatform_PrimaryThumbstickHorizontal"); // modify if using new input
        bool triggerPressed = Input.GetButtonDown("Oculus_CrossPlatform_SecondaryIndexTrigger");

        if (h > 0.5f) currentSelection = (currentSelection + 1) % 3;
        else if (h < -0.5f) currentSelection = (currentSelection + 2) % 3; // left wrap

        for (int i = 0; i < optionButtons.Count; i++)
        {
            var txt = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.color = (i == currentSelection) ? highlightColor : normalColor;
        }


        if (triggerPressed && !questionAnswered)
        {
            // Simulate clicking whichever button is selected
            string answerLabel = optionButtons[currentSelection].GetComponentInChildren<TextMeshProUGUI>().text;
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
