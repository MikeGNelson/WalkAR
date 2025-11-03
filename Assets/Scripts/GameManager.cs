using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float roadLength = 12;
    public int checkPointCount = 100;
    public GameObject startCheckPoint;

    public AudioClip clip;

    AudioSource audioData;

    // Start is called before the first frame update
    void Start()
    {
        audioData = this.GetComponent<AudioSource>();
        CreateTriggers();
        StartCoroutine(PlayBeep());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateTriggers()
    {
        for(int i =0; i < checkPointCount; i++)
        {
            Vector3 moveDistance = new Vector3(0, 0, -roadLength / checkPointCount);
            Quaternion rot = Quaternion.FromToRotation(-startCheckPoint.transform.forward, startCheckPoint.transform.position);
            GameObject checkPoint = Instantiate(startCheckPoint, startCheckPoint.transform.position + i*moveDistance, rot);
            if (i!=checkPointCount-1)
            {
                checkPoint.tag = "Speed";
            }
            else
            {
                checkPoint.tag = "Time";
            }
            //Debug.Log(i.ToString() + " : " + checkPoint.tag);
        }
        
    }

    IEnumerator PlayBeep()
    {
        //print(Time.time);
        yield return new WaitForSeconds(10);
       //print(Time.time);
        audioData.PlayOneShot(clip);
    }

    public void PlayBeepImmediate()
    {
        audioData.PlayOneShot(clip);
    }
}
