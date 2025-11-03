using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Avoidance
{

    public class PlayerController : MonoBehaviour
    {

        public bool hasReachedDestination = false;
        public bool stopWriting = false;
        public bool isRecording = false;
        public GameObject Manager;

        public DataManager DM;
        public GameManager GM;
        public ETRecorder ET;

        public GameController GC;
        //private GameManager EM;

        public TextMeshProUGUI message;

        public float startTime;
        public float endTime;
        public float currentSpeed;

        public Vector3 prevTrans = new Vector3(0, 0, 0);
        private float previousTime;

        public List<double> speeds = null;
        public List<float> times = null;
        public List<Vector3> positions = null;
        public List<float> averageDistances = null;
        public float time = 0;

        public bool eventTriggered = false;
        public GameObject screenBlanker;


        float m_Speed;

        public Vector3 forwardDir;


        // Start is called before the first frame update
        void Start()
        {
            //previousTime = 0;
            //rb = GetComponent<Rigidbody>();
            DM = GameObject.FindObjectOfType<DataManager>();
            GC = GameObject.FindObjectOfType<GameController>();
            ET = GameObject.FindObjectOfType<ETRecorder>();

            ToggleScreen(false);

            m_Speed = 5.0f;

        }

   

        void FixedUpdate()
        {
            isRecording = GC.isRecording;
            if (GC.isRecording)
            {
                message.text = "Walk";
                if (Vector3.Distance(this.transform.position, GC.endPoint.position) < 1.8)
                {
                    //Debug.Log("Reached Dest");

                    if (!stopWriting)
                    {
                        hasReachedDestination = true;
                    }
                }
                else
                {
                    //Debug.Log(Vector3.Distance(this.transform.position, GC.endPoint.position));
                    //Debug.Log(Vector3.Distance(this.transform.position, GC.endPoint.position));
                }
                if (hasReachedDestination)
                {
                    hasReachedDestination = false;
                    stopWriting = true;
                    GC.isRecording = false;
                    ToggleScreen(true);
                    DM.WriteData();
                    ToggleScreen(false);
                }
                else
                {
                    //Debug.Log("Recording");
                    if (!stopWriting)
                    {
                        //Debug.Log("Write");
                        //if (Vector3.Distance(prevTrans, this.transform.position) > .05)
                        {
                            float x = this.transform.position.x;
                            float y = this.transform.position.y;
                            float z = this.transform.position.z;
                            

                            DM.records.Add(new DataManager.Record(Time.time, this.transform.position,   ET.GetEyeData(), eventTriggered));
                            prevTrans = this.transform.position;
                        }


                    }

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
            {
                screenBlanker.gameObject.SetActive(mode);
            }
        }


      


    }

}