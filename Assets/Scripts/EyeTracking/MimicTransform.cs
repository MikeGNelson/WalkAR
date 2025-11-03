using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicTransform : MonoBehaviour
{
    public Transform eyeTracked;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = eyeTracked.rotation;
    }
}
