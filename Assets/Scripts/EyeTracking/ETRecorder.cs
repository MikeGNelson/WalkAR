using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ETRecorder : MonoBehaviour
{
    public Transform playerHead;
    public CyclopRay eyeTrackingWorld;
    public EyeTrackingRay RightEyeTracking, LeftEyeTracking;
    public string outputDirectoryPath;
    public bool isBlinking;
    public float dataRate = 60.0f;
    public int blinkCount = 0;

    private string fileName;
    private List<DataPoint> reportedEyeRecords;

    // Start is called before the first frame update
    void Start()
    {
        reportedEyeRecords = new List<DataPoint>();
        //InvokeRepeating("LogEyeData", 2.0f, (1 / dataRate));
    }

    private void LogEyeData()
    {
        Vector3 gazePoint = eyeTrackingWorld.GetCurrentGazePoint();
        Vector3 gazeRayDirection = eyeTrackingWorld.Direction();
        Vector3 RgazeRayDirection = RightEyeTracking.Direction();
        Vector3 LgazeRayDirection = LeftEyeTracking.Direction();

        float[] eyeData =
        {
           gazePoint.x,
           gazePoint.y,
           gazePoint.z,
           eyeTrackingWorld.transform.position.x,
           eyeTrackingWorld.transform.position.y,
           eyeTrackingWorld.transform.position.z,
           gazeRayDirection.x,
           gazeRayDirection.y,
           gazeRayDirection.z,
           RightEyeTracking.transform.position.x,
           RightEyeTracking.transform.position.y,
           RightEyeTracking.transform.position.z,
           RgazeRayDirection.x,
           RgazeRayDirection.y,
           RgazeRayDirection.z,
           LeftEyeTracking.transform.position.x,
           LeftEyeTracking.transform.position.y,
           LeftEyeTracking.transform.position.z,
           LgazeRayDirection.x,
           LgazeRayDirection.y,
           LgazeRayDirection.z,
           playerHead.position.x,
           playerHead.position.y,
           playerHead.position.z,
           playerHead.eulerAngles.x,
           playerHead.eulerAngles.y,
           playerHead.eulerAngles.z,
           isBlinking? 1 : 0
        };

        reportedEyeRecords.Add(new DataPoint(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(), eyeTrackingWorld.GetCurrentGazeGameObject(), eyeData));
    }

    public DataPoint GetEyeData()
    {
        Vector3 gazePoint = eyeTrackingWorld.GetCurrentGazePoint();
        Vector3 gazeRayDirection = eyeTrackingWorld.Direction();
        Vector3 RgazeRayDirection = RightEyeTracking.Direction();
        Vector3 LgazeRayDirection = LeftEyeTracking.Direction();

        float[] eyeData =
        {
           gazePoint.x,
           gazePoint.y,
           gazePoint.z,
           eyeTrackingWorld.transform.position.x,
           eyeTrackingWorld.transform.position.y,
           eyeTrackingWorld.transform.position.z,
           gazeRayDirection.x,
           gazeRayDirection.y,
           gazeRayDirection.z,
           RightEyeTracking.transform.position.x,
           RightEyeTracking.transform.position.y,
           RightEyeTracking.transform.position.z,
           RgazeRayDirection.x,
           RgazeRayDirection.y,
           RgazeRayDirection.z,
           LeftEyeTracking.transform.position.x,
           LeftEyeTracking.transform.position.y,
           LeftEyeTracking.transform.position.z,
           LgazeRayDirection.x,
           LgazeRayDirection.y,
           LgazeRayDirection.z,
           playerHead.position.x,
           playerHead.position.y,
           playerHead.position.z,
           playerHead.eulerAngles.x,
           playerHead.eulerAngles.y,
           playerHead.eulerAngles.z,
           isBlinking? 1 : 0,
           blinkCount
        };

        return new DataPoint(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(), eyeTrackingWorld.GetCurrentGazeGameObject(), eyeData);
    }

    public void GenerateReport()
    {
        SetFileNameString();
        WriteDataToFile();

        CancelInvoke();
    }

    public void SetBlink(bool blink)
    {
        Debug.Log("Blink");
        isBlinking = blink;
        if(blink ) { blinkCount++; }
    }

    void SetFileNameString()
    {
        fileName = System.DateTime.Now.ToShortDateString() + ", " + GetTimestamp();
        foreach (var c in Path.GetInvalidFileNameChars()) { fileName = fileName.Replace(c, '.'); }
    }

    void WriteDataToFile()
    {
        // Create the file.
        string path = Application.dataPath + "/" + outputDirectoryPath + "/" + fileName + ".csv";

        using (StreamWriter sw = File.CreateText(path))
        {
            sw.WriteLine("Time,GazeObject,GP_X,GP_X,GP_Z,GZ_X,GZ_Y,GZ_Y,GZD_X,GZD_Y,GZD_Z,RE_X,RE_Y,RE_Z,RED_X,RED_Y,RED_Z,LE_X,LE_Y,LE_Z,LED_X,LED_Y,LED_Z,H_X,H_Y,H_Z,HR_X,HR_Y,HR_Z,blinks");

            // Write everything from the array.

            foreach (DataPoint entry in reportedEyeRecords)
            {
                sw.WriteLine(entry.JoinValues());
            }

            // Write the shingle header.
            sw.WriteLine();

            // Close the stream.
            sw.Close();
        }
    }

    private string GetTimestamp()
    {
        return System.DateTime.Now.ToLongTimeString();
    }


}

public class DataPoint
{
    public string timestamp;
    public string currentGazeObject;
    public float[] eyeData;

    public DataPoint(string timestamp, string currentGazeObject, float[] eyeData)
    {
        this.timestamp = timestamp;
        this.currentGazeObject = currentGazeObject;
        this.eyeData = eyeData;
    }

    public string JoinValues()
    {
        return currentGazeObject + "," + string.Join(",", eyeData);
    }
}
