using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;


public class Person : MonoBehaviour
{
    public int index = 0;
    public NavMeshAgent agent;
    public GameController controller;
    public int spawnIndex { get; set; }
    public float angle { get; set; }

    public float distanceFromGoal;

    public State state = State.NotCrossing;
    public enum State
    {
        NotCrossing,
        Crossing
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        controller = GameObject.FindObjectOfType<GameController>();
        //agent.speed = controller.crowdSpeed;
        agent.radius = distanceFromGoal;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (!agent.pathPending && agent.remainingDistance <  distanceFromGoal)
    //         GotoNextPoint();

        

    // }
    // private void OnTriggerEnter(Collider collision)
    // {
    //     if (collision.gameObject.tag == "Time" || collision.gameObject.tag == "Time1")
    //     {
    //         if(state==State.NotCrossing)
    //         {
    //             state = State.Crossing;
    //         }
            
    //     }
    // }

    //     public void GotoNextPoint()
    // {
    //     // Returns if no points have been set up
    //     if (controller.startPoints.Count == 0)
    //         return;

    //     if (index == 0)
    //     {
    //         // Set the agent to go to the currently selected destination.
    //         //Debug.Log(spawnIndex);
    //         //agent.destination = controller.startPoints[spawnIndex].position;
    //         ChooseNextMidPoint();
    //         index++;
    //     }
    //     if (index == 1)
    //     {
    //         // Set the agent to go to the currently selected destination.
    //         ChooseNextMidPoint();
    //     }
    //     if (index == 2)
    //     {
    //         // Set the agent to go to the currently selected destination.
    //         agent.destination = controller.endPoints[spawnIndex].position;
    //     }
    //     if(index >2)
    //     {
    //         //Debug.Log("complete");
    //         //Destroy(this);
    //         state = State.NotCrossing;
    //         //int spawnPoint = (int)Random.Range(0, controller.spawnPoints.Count);
    //         //transform.position = controller.RandomSpawnPoint(spawnPoint);
    //         index = 0;
    //         agent.destination = controller.startPoints[spawnIndex].position;
    //         return;
    //     }


    //     // Choose the next point in the array as the destination,
    //     // cycling to the start if necessary.
    //     index++;
    // }

    public void ChooseNextMidPoint()
    {
        
        // float rad = angle * Mathf.Deg2Rad;
        // float a = (controller.startPoints[spawnIndex].position.z - controller.midPoints[spawnIndex].position.z) * Mathf.Tan(rad);
        // int random = (int)Random.Range(0, controller.startPoints.Count);
        // Vector3 dir = new Vector3(a, 0, 0);
        //Debug.Log(angle);
        //Debug.Log(rad);
        //Debug.Log(a);
        // switch (controller.crowdDirection)
        // {
            
        //     //Go to corresponding node
        //     case CrowdController.crowdDirections.Straight:
        //         agent.destination = controller.midPoints[random].position;
        //         break;
        //     //Go to corresponding node - (angle)
        //     case CrowdController.crowdDirections.Left:
        //         //a = (controller.midPoints[spawnIndex].position - controller.startPoints[spawnIndex].position) * Mathf.Tan(angle);
        //         agent.destination = controller.midPoints[random].position +  dir;
        //         break;
        //     //Go to corresponding node + (angle)
        //     case CrowdController.crowdDirections.Right:
        //         //Vector3 a = (controller.midPoints[spawnIndex].position - controller.startPoints[spawnIndex].position) * Mathf.Tan(angle);
        //         agent.destination = controller.midPoints[random].position - dir;
        //         break;

        // }
    }
    public void ChooseNextEndPoint()
    {

    }
}
