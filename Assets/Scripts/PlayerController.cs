using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Avoidance
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public DataManager DM;
        public GameController GC;
        public ETRecorder ET;
        public TextMeshProUGUI message;
        public GameObject screenBlanker;

        [Header("Runtime Flags")]
        public bool hasReachedDestination = false;
        public bool stopWriting = false;
        public bool isRecording = false;
        public bool eventTriggered = false;

        [Header("Kinematic Info")]
        public float startTime;
        public float endTime;
        public float currentSpeed;
        public Vector3 prevTrans = Vector3.zero;
        private float previousTime;

        [Header("Tracked Hands")]
        public Transform leftHand;
        public Transform rightHand;

        private float m_Speed = 5.0f;

        void Start()
        {
            DM = FindObjectOfType<DataManager>();
            GC = FindObjectOfType<GameController>();
            ET = FindObjectOfType<ETRecorder>();
            ToggleScreen(false);
            prevTrans = transform.position;
        }

        void FixedUpdate()
        {
            isRecording = GC.isRecording;

            if (isRecording)
            {
                message.text = "Walk";

                // Detect reaching the destination
                if (Vector3.Distance(transform.position, GC.endPoint.position) < 1.8f && !stopWriting)
                {
                    hasReachedDestination = true;
                }

                if (hasReachedDestination)
                {
                    // Trial complete
                    hasReachedDestination = false;
                    stopWriting = true;
                    isRecording = false;
                    GC.isRecording = false;

                    endTime = Time.time;
                    ToggleScreen(true);

                    // Write and end trial
                    DM.WriteData();
                    GC.EndTrial();

                    ToggleScreen(false);
                    return;
                }

                // While walking and recording
                if (!stopWriting)
                {
                    // Compute instantaneous speed
                    float deltaTime = Time.fixedDeltaTime;
                    float distance = Vector3.Distance(prevTrans, transform.position);
                    currentSpeed = distance / deltaTime;

                    // Log positional + eye data
                    var eyeData = ET.GetEyeData();
                    DM.AddRecord(transform.position,
                                 leftHand != null ? leftHand.position : Vector3.zero,
                                 leftHand != null ? leftHand.rotation : Quaternion.identity,
                                 rightHand != null ? rightHand.position : Vector3.zero,
                                 rightHand != null ? rightHand.rotation : Quaternion.identity,
                                 (int)DM.conditions, eyeData, eventTriggered);

                    prevTrans = transform.position;
                    previousTime = Time.time;
                }
            }
            else
            {
                message.text = "Stop";
            }
        }

        void ToggleScreen(bool mode)
        {
            if (screenBlanker != null)
                screenBlanker.SetActive(mode);
        }

        // Optional helper — resets player state for next trial
        public void ResetTrialState()
        {
            stopWriting = false;
            hasReachedDestination = false;
            eventTriggered = false;
            prevTrans = transform.position;
            startTime = Time.time;
        }
    }
}
