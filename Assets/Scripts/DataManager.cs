using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using Photon.Pun;
using System;
using UnityEngine.EventSystems;

public class DataManager : MonoBehaviourPunCallbacks
{

    //public List<int> answers = null;
    public List<double> speeds = null;
    public List<float> times = null;
    public List<Vector3> positions = null;
    public List<float> interPersonalDistance = null;
    public float time = 0;
    public TextMeshProUGUI conditionIndicator;

    public float outerDistance = 0;

    private string conditionPrompt = "Record this number in your post round survey:";

    public string conditionText = "Default";

    public int models = 0;

    public int UId;
    string path;
    string path1;

    public GameController gameController;
    public ETRecorder ET;






    public List<Record> records = new List<Record>();
    public struct Record
    {
        public float time;

        public Vector3 position;

        public bool eventTriggered;

        public int condition;

        //public float[] eyeData;
        //public string currentGazeObject;
        public DataPoint eyeData;

        public Record(float _time, Vector3 _position,   int _condition, DataPoint _eyeData, bool _eventTriggered)
        {
            Debug.Log("Add Data: " + _time);
            condition = _condition;
            time = _time;
            position = _position;
          
            

            eventTriggered = _eventTriggered;

            eyeData = _eyeData;


            



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
    public Conditons conditions;






    public

    // Start is called before the first frame update
    void Start()
    {
        if (UId < 0)
        {
            path = "Assets/Results/test.txt";
        }
        else
        {
            Debug.Log("Set Path");
            path = "Assets/Results/" + DateTime.Now.ToFileTime() + ".csv";
        }



        //TODO
        // Send the data based on the conditon
        //SendCondition();
    }

    public void SendCondition()
    {
        string conditionText = conditionPrompt;
        float width = 0f;
        float speed = 0f;

        switch (conditions)
        {

            #region models

            // Study Styles
            case Conditons.One_Default:
                //Set model group

                models = 0;
                conditionText = conditionPrompt + " 1";
                break;
            case Conditons.Two_Toon:
                //Set model group

                models = 1;//2
                conditionText = conditionPrompt + " 2";
                break;
            case Conditons.Three_Creepy:
                //Set model group

                models = 2;
                conditionText = conditionPrompt + " 3";
                break;
            case Conditons.Four_Spooky:
                //Set model group

                models = 3;
                conditionText = conditionPrompt + " 4";
                break;
            case Conditons.Five_Robot:
                //Set model group

                models = 4;
                conditionText = conditionPrompt + " 5";
                break;
            #endregion

            #region follow
            /// Study Cylinders
            //No follow
            case Conditons.Cylinder_Small_No_Follow:
                //Set model group

                models = 5;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Medium_No_Follow:
                //Set model group

                models = 6;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Large_No_Follow:
                //Set model group

                models = 7;
                conditionText = conditionPrompt + " 5";
                break;

            //Follow Slow
            case Conditons.Cylinder_Small_Follow_Slow:
                //Set model group

                models = 5;
                speed = 0.5f;
                gameController.currentCurve = gameController.smallCurve;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Medium_Follow_Slow:
                //Set model group

                models = 6;
                speed = 0.5f;
                gameController.currentCurve = gameController.mediumCurve;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Large_Follow_Slow:
                //Set model group

                models = 7;
                speed = 0.5f;
                gameController.currentCurve = gameController.largeCurve;
                conditionText = conditionPrompt + " 5";
                break;


            //Follow Medium
            case Conditons.Cylinder_Small_Follow_Medium:
                //Set model group

                models = 5;
                speed = 1.2f;
                gameController.currentCurve = gameController.smallCurve;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Medium_Follow_Medium:
                //Set model group

                models = 6;
                speed = 1.2f;
                gameController.currentCurve = gameController.mediumCurve;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Large_Follow_Medium:
                //Set model group

                models = 7;
                speed = 1.2f;
                gameController.currentCurve = gameController.largeCurve;
                conditionText = conditionPrompt + " 5";
                break;

            //Follow Fast
            case Conditons.Cylinder_Small_Follow_Fast:
                //Set model group

                models = 5;
                speed = 1.4f;
                gameController.currentCurve = gameController.smallCurve;
                conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Cylinder_Medium_Follow_Fast:
                //Set model group

                models = 6;
                speed = 1.4f;
                conditionText = conditionPrompt + " 5";
                gameController.currentCurve = gameController.mediumCurve;
                break;

            case Conditons.Cylinder_Large_Follow_Fast:
                //Set model group

                models = 7;
                speed = 1.4f;
                gameController.currentCurve = gameController.largeCurve;
                conditionText = conditionPrompt + " 5";
                break;
            #endregion

            /// Study Sneeze
            /// 
            #region proxemics

            // Idle Modes
            case Conditons.Close_Idle:
                //Set model group

                models = 11;
                outerDistance = .5f;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Middle_Idle:
                //Set model group

                models = 12;
                outerDistance = 1;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Far_Idle:
                //Set model group

                models = 13;
                outerDistance = 3.5f;
                //conditionText = conditionPrompt + " 5";
                break;


            // Minor arugement modes
            case Conditons.Close_Minor:
                //Set model group

                models = 8;
                outerDistance = .5f;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Middle_Minor:
                //Set model group

                models = 9;
                outerDistance = 1;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Far_Minor:
                //Set model group

                models = 10;
                outerDistance = 3.5f;
                //conditionText = conditionPrompt + " 5";
                break;


            // Moderate arugement modes
            case Conditons.Close_Moderate:
                //Set model group

                models = 8;
                outerDistance = .5f;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Middle_Moderate:
                //Set model group

                models = 9;
                outerDistance = 1;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Far_Moderate:
                //Set model group

                models = 10;
                outerDistance = 3.5f;
                //conditionText = conditionPrompt + " 5";
                break;


            // Major arugement modes
            case Conditons.Close_Major:
                //Set model group

                models = 8;
                outerDistance = .5f;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Middle_Major:
                //Set model group

                models = 9;
                outerDistance = 1;
                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Far_Major:
                //Set model group

                models = 10;
                outerDistance = 3.5f;
                //conditionText = conditionPrompt + " 5";
                break;

            #endregion

            #region sneeze
            case Conditons.Sneeze_None:
                //Set model group

                models = 14;
                conditionText = "Sneeze_None";

                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Sneeze_Away:
                //Set model group
                conditionText = "Sneeze_Away";
                models = 15;

                //conditionText = conditionPrompt + " 5";
                break;

            case Conditons.Sneeze_Towards:
                //Set model group

                models = 16;
                conditionText = "Sneeze_Towards";

                //conditionText = conditionPrompt + " 5";
                break;

                #endregion

        }
        int mode = models;
        conditionIndicator.text = conditionText;

        photonView.RPC("UpdatePromptText", RpcTarget.All, conditionText, mode);
        gameController.GetConditions(models, width, speed);
    }




    /// <summary>
    /// Write the data when reaching the last node
    /// </summary>
    public void WriteData()
    {
        //string rep = " A";
        //path = "Assets/Results/" + (records[0].condition + 1).ToString() + "A.csv";
        //path1 = "Assets/Results/" + (records[0].condition + 1).ToString() + "A_Summary.csv";
        //if (System.IO.File.Exists(path))
        //{
        //    path = "Assets/Results/" + (records[0].condition + 1).ToString() + "B.csv";
        //    path1 = "Assets/Results/" + (records[0].condition + 1).ToString() + "B_Summary.csv";
        //    rep = " B";
        //}

        switch (records[0].condition)
        {
            case 14:
                conditionText = "Sneeze_None";
                break;
            case 15:
                conditionText = "Sneeze_Away";
                break;
            case 16:
                conditionText = "Sneeze_Towards";
                break;

        }
        Debug.Log("Write Data");
#if UNITY_EDITOR
        path = Application.dataPath + "/" + conditionText + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString() + "_Raw.csv";
        path1 = Application.dataPath + "/" + conditionText + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString() + "_Summary.csv";
#else
            path  = Application.persistentDataPath  + "/" + conditionText + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString() + "_Raw.csv";
            path1  = Application.persistentDataPath  + "/" + conditionText + "_" + ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString() + "_Summary.csv";
#endif

        float sumDistance = 0;
        float totalTime = 0;
        float startTime = records[0].time;
        float averageSpeed = 0;
        float minimumDistance = 100;

        List<float> passingDistances = new List<float>();
        float totalPassing = 0;
        float passingDistance = 100;

        bool isMinDistSide = false;
        string isMinDistanceFrontOrBack = "";
        string passingSide = "";
        string outer = "";



        int tIndex = 0;
        int maxTIndex = records.Count - 1;

        StreamWriter writer = new StreamWriter(path, true);



        int i = 0;
        Debug.Log("--------------------------------");
        Debug.Log("Condition: " + models);
        Debug.Log("--------------------------------");
        //writer.WriteLine("--------------------------------");
        //writer.WriteLine("Condition: " + (records[0].condition +1).ToString() + rep);
        //writer.WriteLine("--------------------------------");
        writer.WriteLine("Condition, Index, Time, Distance, X, Y, Z, isCenter, isFront, isSide, isLeft, isOuter,GazeObject,GP_X,GP_X,GP_Z,GZ_X,GZ_Y,GZ_Y,GZD_X,GZD_Y,GZD_Z,RE_X,RE_Y,RE_Z,RED_X,RED_Y,RED_Z,LE_X,LE_Y,LE_Z,LED_X,LED_Y,LED_Z,H_X,H_Y,H_Z,HR_X,HR_Y,HR_Z,eye_closed,blink_count, eventTriggered");


        // Write everything from the array.


        foreach (Record record in records)
        {

            if (tIndex != maxTIndex)
            {
                sumDistance += Vector3.Distance(record.position, records[tIndex + 1].position);
            }
            else
            {
                totalTime = record.time;
            }

            

            Debug.Log("--------------------------------");
            Debug.Log("Index: " + i);
            Debug.Log("--------------------------------");
            Debug.Log("Time: " + record.time);
            Debug.Log("Distance: " + record.distance);
            Debug.Log("position: " + record.position);
            Debug.Log("isCenter: " + record.isCenter);
            Debug.Log("isFront: " + record.isFront);
            Debug.Log("isSide: " + record.isSide);
            Debug.Log("isLeft: " + record.isLeft);
            Debug.Log("isOutside: " + record.isOutside);

            //writer.WriteLine("--------------------------------");
            //writer.WriteLine("Index: " + i);
            //writer.WriteLine("--------------------------------");
            //writer.WriteLine("Time: " + record.time);
            //writer.WriteLine("Distance: " + record.distance);
            //writer.WriteLine("position: " + record.position);
            //writer.WriteLine("isCenter: " + record.isCenter);
            //writer.WriteLine("isFront: " + record.isFront);
            //writer.WriteLine("isSide: " + record.isSide);
            //writer.WriteLine("isLeft: " + record.isLeft);

            writer.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11},{12},{13}"
                , conditionText, i, record.time, record.distance, record.x, record.y, record.z, record.isCenter, record.isFront, record.isSide, record.isLeft, record.isOutside, record.eyeData.JoinValues(), record.eventTriggered));




            tIndex++;
            i++;
        }


        writer.Close();

        //Summary
        Debug.Log("write sum");

        passingDistance = totalPassing / (passingDistances.Count - 1);
        averageSpeed = sumDistance / totalTime;
        if (isMinDistSide)
        {
            isMinDistanceFrontOrBack = "";
        }

        totalTime -= startTime;

        StreamWriter sw1 = new StreamWriter(path1);
        sw1.WriteLine("Condition, Total Distance, Total Time, Average Speed, Minimum Distance, Passing Distance, Is Minimum Distance Side, Is Minimum Distance Front or Back, Passing Side, Inside or Outside");
        sw1.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}",
            conditionText,
            sumDistance.ToString(),
            totalTime.ToString(),
            averageSpeed.ToString(),
            minimumDistance.ToString(),
            passingDistance.ToString(),
            isMinDistSide,
            isMinDistanceFrontOrBack,
            passingSide,
            outer
            ));

        sw1.Close();

        if (SessionManager.Instance.mode == 1)
        {
            // Read the files as byte arrays
            byte[] rawData = File.ReadAllBytes(path);
            byte[] summaryData = File.ReadAllBytes(path1);

            // Send them via RPC (you might want to send them to others, not yourself)
            photonView.RPC("ReceiveFileData", RpcTarget.Others, Path.GetFileName(path), rawData);
            photonView.RPC("ReceiveFileData", RpcTarget.Others, Path.GetFileName(path1), summaryData);
        }


        records.Clear();

    }

    [PunRPC]
    void ReceiveFileData(string fileName, byte[] fileData)
    {
        string filePath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(filePath, fileData);
        Debug.Log("Received and wrote file to: " + filePath);
    }


    [PunRPC]
    void UpdatePromptText(string text, int mode)
    {
        Debug.Log(text);
        gameController.isRecording = true;
        models = mode;
        ET.blinkCount = 0;
        //path = "Assets/Results/" + DateTime.Now.ToFileTime() + ".txt";
        conditionIndicator.text = text;
    }


}
