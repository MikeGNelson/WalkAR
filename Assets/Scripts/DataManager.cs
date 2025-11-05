using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using System.Linq;

public class DataManager : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI conditionIndicator;
    public ETRecorder ET;

    [Header("Runtime")]
    public int UId;
    public string conditionText = "Default";
    public Conditons conditions;

    private string basePath;
    private string pathRaw;
    private string pathSummary;
    private string pathTasks;

    [Header("Data Buffers")]
    public List<Record> records = new List<Record>();
    public List<MathTask> mathTasks = new List<MathTask>();

    // ---------- STRUCTS ----------

    public struct Record
    {
        public float time;
        public Vector3 position;
        public Vector3 leftHandPos;
        public Quaternion leftHandRot;
        public Vector3 rightHandPos;
        public Quaternion rightHandRot;
        public int condition;
        public bool eventTriggered;
        public DataPoint eyeData;

        public Record(float _time, Vector3 _position, Vector3 _leftPos, Quaternion _leftRot,
              Vector3 _rightPos, Quaternion _rightRot, int _condition, DataPoint _eyeData, bool _eventTriggered)
        {
            time = _time;
            position = _position;
            leftHandPos = _leftPos;
            leftHandRot = _leftRot;
            rightHandPos = _rightPos;
            rightHandRot = _rightRot;
            condition = _condition;
            eyeData = _eyeData;
            eventTriggered = _eventTriggered;
        }
    }

    public struct MathTask
    {
        public float spawnTime;
        public float answerTime;
        public string question;
        public string participantAnswer;
        public string correctAnswer;
        public bool isCorrect;
        public bool answered; // true if response given

        public float ResponseTime => answered ? answerTime - spawnTime : -1f;

        public MathTask(float _spawnTime, float _answerTime, string _q, string _a, string _correct, bool _isCorrect, bool _answered)
        {
            spawnTime = _spawnTime;
            answerTime = _answerTime;
            question = _q;
            participantAnswer = _a;
            correctAnswer = _correct;
            isCorrect = _isCorrect;
            answered = _answered;
        }
    }

    public enum Conditons
    {
        Center_Head_Close,
        Center_Head_Far,
        Center_Dir_Close,
        Center_Dir_Far,
        Right_Head_Close,
        Right_Head_Far,
        Right_Dir_Close,
        Right_Dir_Far
    };

    // ---------- LIFECYCLE ----------

    void Start()
    {
#if UNITY_EDITOR
        basePath = Application.dataPath + "/Results/";
#else
        basePath = Application.persistentDataPath + "/Results/";
#endif
        Directory.CreateDirectory(basePath);
        CreatePaths();
    }

    void CreatePaths()
    {
        string timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
        string prefix = $"UID{UId}_{conditionText}_{timestamp}";
        pathRaw = Path.Combine(basePath, prefix + "_Raw.csv");
        pathSummary = Path.Combine(basePath, prefix + "_Summary.csv");
        pathTasks = Path.Combine(basePath, prefix + "_Task.csv");
    }

    public void SendCondition()
    {
        // Convert enum to readable label for both file names and UI
        conditionText = conditions.ToString();

        conditionIndicator.text = $"Condition: {conditionText}  (UID {UId})";
    }

    // ---------- LOGGING ----------

    public void AddRecord(Vector3 playerPos, Vector3 leftHandPos, Quaternion leftHandRot,
                      Vector3 rightHandPos, Quaternion rightHandRot,
                      int condition, DataPoint eyeData, bool eventTriggered)
    {
        records.Add(new Record(Time.time, playerPos, leftHandPos, leftHandRot, rightHandPos, rightHandRot, condition, eyeData, eventTriggered));
    }


    public void AddMathTask(float spawnTime, float answerTime, string question, string answer, string correct, bool isCorrect, bool answered)
    {
        mathTasks.Add(new MathTask(spawnTime, answerTime, question, answer, correct, isCorrect, answered));
    }

    // ---------- WRITE ----------

    public void WriteData()
    {
        if (records.Count == 0)
        {
            Debug.LogWarning("No data to write!");
            return;
        }

        if (string.IsNullOrEmpty(conditionText))
            conditionText = conditions.ToString();


        CreatePaths();
        WriteRawData();
        WriteSummaryData();
        WriteTaskData();

        records.Clear();
        mathTasks.Clear();
    }

    private void WriteRawData()
    {
        using (StreamWriter writer = new StreamWriter(pathRaw))
        {
            writer.WriteLine("UID,Condition,Index,Time,Player_X,Player_Y,Player_Z," +
                 "Left_X,Left_Y,Left_Z,LeftRot_X,LeftRot_Y,LeftRot_Z,LeftRot_W," +
                 "Right_X,Right_Y,Right_Z,RightRot_X,RightRot_Y,RightRot_Z,RightRot_W," +
                 "GazeObject,GP_X,GP_Y,GP_Z,GZ_X,GZ_Y,GZ_Z,RE_X,RE_Y,RE_Z,RED_X,RED_Y,RED_Z," +
                 "LE_X,LE_Y,LE_Z,LED_X,LED_Y,LED_Z,H_X,H_Y,H_Z,HR_X,HR_Y,HR_Z,Blinking,BlinkCount,EventTriggered");


            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                writer.WriteLine($"{UId},{conditions},{i},{r.time}," +
                 $"{r.position.x},{r.position.y},{r.position.z}," +
                 $"{r.leftHandPos.x},{r.leftHandPos.y},{r.leftHandPos.z}," +
                 $"{r.leftHandRot.x},{r.leftHandRot.y},{r.leftHandRot.z},{r.leftHandRot.w}," +
                 $"{r.rightHandPos.x},{r.rightHandPos.y},{r.rightHandPos.z}," +
                 $"{r.rightHandRot.x},{r.rightHandRot.y},{r.rightHandRot.z},{r.rightHandRot.w}," +
                 $"{r.eyeData.JoinValues()},{r.eventTriggered}");

            }
        }

        Debug.Log($"Raw data written: {pathRaw}");
    }

    //private void WriteSummaryData()
    //{
    //    if (records.Count < 2) return;

    //    float totalDist = 0f;
    //    float totalTime = records[^1].time - records[0].time;
    //    for (int i = 0; i < records.Count - 1; i++)
    //        totalDist += Vector3.Distance(records[i].position, records[i + 1].position);
    //    float avgSpeed = totalTime > 0 ? totalDist / totalTime : 0f;

    //    // ---------- Blink stats ----------
    //    int totalBlinkCount = 0;
    //    int blinkSamples = 0;
    //    foreach (var r in records)
    //    {
    //        if (r.eyeData.eyeData.Length >= 30)
    //        {
    //            totalBlinkCount += (int)r.eyeData.eyeData[^1];
    //            blinkSamples++;
    //        }
    //    }
    //    float avgBlinkPerSample = blinkSamples > 0 ? (float)totalBlinkCount / blinkSamples : 0f;

    //    // ---------- Gaze object durations ----------
    //    Dictionary<string, float> gazeDurations = new Dictionary<string, float>();
    //    for (int i = 0; i < records.Count - 1; i++)
    //    {
    //        string obj = records[i].eyeData.currentGazeObject ?? "None";
    //        float dt = records[i + 1].time - records[i].time;
    //        if (!gazeDurations.ContainsKey(obj))
    //            gazeDurations[obj] = 0f;
    //        gazeDurations[obj] += dt;
    //    }

    //    // ---------- Math task outcomes ----------
    //    int answeredRight = 0, answeredWrong = 0, notAnswered = 0;
    //    foreach (var task in mathTasks)
    //    {
    //        if (!task.answered) notAnswered++;
    //        else if (task.isCorrect) answeredRight++;
    //        else answeredWrong++;
    //    }

    //    // ---------- Write Summary ----------
    //    using (StreamWriter sw = new StreamWriter(pathSummary))
    //    {
    //        sw.WriteLine("UID,Condition,TotalDistance,TotalTime,AverageSpeed,AverageBlinkPerSample,TotalBlinkCount,AnsweredRight,AnsweredWrong,NotAnswered");

    //        sw.Write($"{UId},{conditions},{totalDist:F3},{totalTime:F3},{avgSpeed:F3},{avgBlinkPerSample:F3},{totalBlinkCount},{answeredRight},{answeredWrong},{notAnswered}\n");

    //        sw.WriteLine("\nGazeDurations(seconds):");
    //        foreach (var kvp in gazeDurations.OrderByDescending(x => x.Value))
    //            sw.WriteLine($"{kvp.Key},{kvp.Value:F3}");
    //    }

    //    Debug.Log($"Summary data written: {pathSummary}");
    //}

    private void WriteSummaryData()
    {
        if (records.Count < 2) return;

        float totalTime = records[^1].time - records[0].time;
        float totalDist = ComputeTotalDistance();
        float avgSpeed = totalTime > 0 ? totalDist / totalTime : 0f;
        float speedSD = ComputeSpeedSD(totalTime);
        float straightness = ComputeStraightness();
        float headingJitter = ComputeHeadingJitter();
        float accelCost = ComputeAccelerationCost();

        // Gaze & Math summaries
        var gazeStats = ComputeGazeStats(totalTime);
        var mathStats = ComputeMathStats();

        // Blink extraction
        int totalBlinkCount = 0;
        int blinkSamples = 0;
        foreach (var r in records)
        {
            if (r.eyeData.eyeData.Length >= 30)
            {
                totalBlinkCount += (int)r.eyeData.eyeData[^1];
                blinkSamples++;
            }
        }
        float avgBlinkPerSample = blinkSamples > 0 ? (float)totalBlinkCount / blinkSamples : 0f;

        using (StreamWriter sw = new StreamWriter(pathSummary))
        {
            sw.WriteLine("UID,Condition,TotalTime(s),TotalDistance(m),AverageSpeed(m/s),SpeedSD(m/s),StraightnessIndex,HeadingJitter(deg/s),AccelerationCost,AverageBlinkPerSample,TotalBlinkCount,Gaze_UI(%),Gaze_Ground(%),ScanRate(/s),AnsweredRight,AnsweredWrong,NotAnswered,Accuracy(%),MissRate(%),InverseEfficiency(ms),BalancedIndex");

            sw.WriteLine($"{UId},{conditions},{totalTime:F3},{totalDist:F3},{avgSpeed:F3},{speedSD:F3},{straightness:F3},{headingJitter:F3},{accelCost:F3},{avgBlinkPerSample:F3},{totalBlinkCount},{gazeStats.uiRatio * 100f:F1},{gazeStats.groundRatio * 100f:F1},{gazeStats.scanRate:F2},{mathStats.right},{mathStats.wrong},{mathStats.missed},{mathStats.accuracy:F1},{mathStats.missRate:F1},{mathStats.invEff:F2},{mathStats.bis:F3}");
        }

        Debug.Log($"Summary data written: {pathSummary}");
    }


    private void WriteTaskData()
    {
        if (mathTasks.Count == 0) return;

        using (StreamWriter writer = new StreamWriter(pathTasks))
        {
            writer.WriteLine("UID,Condition,SpawnTime,AnswerTime,ResponseTime,Question,Answer,CorrectAnswer,IsCorrect,Answered");
            foreach (var t in mathTasks)
            {
                writer.WriteLine($"{UId},{conditions},{t.spawnTime:F3},{t.answerTime:F3}," +
                                 $"{(t.answered ? t.ResponseTime.ToString("F3") : "N/A")}," +
                                 $"{t.question},{t.participantAnswer},{t.correctAnswer},{t.isCorrect},{t.answered}");
            }
        }

        Debug.Log($"Task data written: {pathTasks}");
    }



    // ---------- MOVEMENT HELPERS ----------

    private float ComputeTotalDistance()
    {
        float total = 0f;
        for (int i = 0; i < records.Count - 1; i++)
            total += Vector3.Distance(records[i].position, records[i + 1].position);
        return total;
    }

    private float ComputeSpeedSD(float totalTime)
    {
        if (records.Count < 3) return 0f;
        List<float> speeds = new List<float>();
        for (int i = 1; i < records.Count; i++)
        {
            float dt = records[i].time - records[i - 1].time;
            if (dt > 0)
                speeds.Add(Vector3.Distance(records[i - 1].position, records[i].position) / dt);
        }
        float mean = speeds.Average();
        return Mathf.Sqrt(speeds.Average(v => Mathf.Pow(v - mean, 2)));
    }

    private float ComputeStraightness()
    {
        if (records.Count < 2) return 1f;
        float displacement = Vector3.Distance(records[0].position, records[^1].position);
        float pathLength = ComputeTotalDistance();
        return pathLength > 0 ? displacement / pathLength : 0f;
    }

    private float ComputeHeadingJitter() 
    {
        if (records.Count < 3) return 0f;
        List<float> yawRates = new List<float>();
        for (int i = 1; i < records.Count; i++)
        {
            Vector3 dirPrev = (records[i - 1].position - records[Mathf.Max(0, i - 2)].position).normalized;
            Vector3 dirNow = (records[i].position - records[i - 1].position).normalized;
            float yaw = Vector3.SignedAngle(dirPrev, dirNow, Vector3.up);
            float dt = records[i].time - records[i - 1].time;
            if (dt > 0) yawRates.Add(Mathf.Abs(yaw / dt));
        }
        float mean = yawRates.Average();
        return Mathf.Sqrt(yawRates.Average(v => Mathf.Pow(v - mean, 2)));
    }

    private float ComputeAccelerationCost()
    {
        if (records.Count < 4) return 0f;
        List<float> accRates = new List<float>();
        for (int i = 2; i < records.Count; i++)
        {
            Vector3 v1 = (records[i - 1].position - records[i - 2].position) / (records[i - 1].time - records[i - 2].time);
            Vector3 v2 = (records[i].position - records[i - 1].position) / (records[i].time - records[i - 1].time);
            Vector3 accel = (v2 - v1) / (records[i].time - records[i - 1].time);
            accRates.Add(accel.magnitude);
        }
        return accRates.Average();
    }


    // ---------- GAZE HELPERS ----------

    private string GetGazeCategory(string objName)
    {
        if (string.IsNullOrEmpty(objName)) return "None";
        if (objName.Contains("UI") || objName.Contains("Panel") || objName.Contains("Math"))
            return "UI";
        if (objName.Contains("Ground") || objName.Contains("Floor"))
            return "Ground";
        return "Environment";
    }

    private (float uiRatio, float groundRatio, float scanRate) ComputeGazeStats(float totalTime)
    {
        if (records.Count < 2) return (0, 0, 0);

        int gazeSwitches = 0;
        float uiTime = 0f, groundTime = 0f;
        string prevCategory = GetGazeCategory(records[0].eyeData.currentGazeObject);

        for (int i = 0; i < records.Count - 1; i++)
        {
            string category = GetGazeCategory(records[i].eyeData.currentGazeObject);
            float dt = records[i + 1].time - records[i].time;

            if (category == "UI") uiTime += dt;
            else if (category == "Ground") groundTime += dt;

            if (category != prevCategory) gazeSwitches++;
            prevCategory = category;
        }

        float scanRate = totalTime > 0 ? gazeSwitches / totalTime : 0f;
        return (uiTime / totalTime, groundTime / totalTime, scanRate);
    } 


    // ---------- MATH TASK HELPERS ----------
    private (int right, int wrong, int missed, float accuracy, float missRate,
             float invEff, float bis) ComputeMathStats()
    {
        if (mathTasks == null || mathTasks.Count == 0)
            return (0, 0, 0, 0f, 0f, 0f, 0f);

        int right = 0, wrong = 0, missed = 0;
        List<float> rt = new List<float>();

        foreach (var t in mathTasks)
        {
            if (!t.answered)
            {
                missed++;
                continue;
            }

            if (t.isCorrect)
                right++;
            else
                wrong++;

            if (t.ResponseTime > 0)
                rt.Add(t.ResponseTime);
        }

        float accuracy = (right + wrong) > 0 ? (float)right / (right + wrong) * 100f : 0f;
        float missRate = mathTasks.Count > 0 ? (float)missed / mathTasks.Count * 100f : 0f;
        float meanRT = rt.Count > 0 ? rt.Average() * 1000f : 0f;  // ms

        // Inverse-Efficiency (ms / %correct)
        float invEff = accuracy > 0 ? meanRT / accuracy : 0f;

        // Balanced-Integration Score (scaled composite)
        float bis = (accuracy / 100f) - (meanRT / 10000f);

        return (right, wrong, missed, accuracy, missRate, invEff, bis);
    }




}
