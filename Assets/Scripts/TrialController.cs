using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class TrialController : MonoBehaviourPunCallbacks
{
    public GameController GC ;
    public DataManager DM;

    public GameObject camera;
    public GameObject canvas;

    // Start is called before the first frame update
    void Start()
    {
        GC = GameObject.FindObjectOfType<GameController>();
        DM = GameObject.FindObjectOfType<DataManager>();
        if(!photonView.IsMine)
        {
            camera.gameObject.SetActive(false);
            canvas.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTrial(int condition)
    {
        Debug.Log("Start Trial");
        DM.conditions = (DataManager.Conditons)condition;
        DM.SendCondition();

    }

    public void ResetPosition()
    {
        Debug.Log("reset");
        Transform offset = GC.vrRig.gameObject.GetComponentInChildren<Transform>();
        offset.position = GameObject.FindGameObjectWithTag("StartPosition").transform.position;
        offset.LookAt(GameObject.FindGameObjectWithTag("EndPosition").transform.position);
    }
}
