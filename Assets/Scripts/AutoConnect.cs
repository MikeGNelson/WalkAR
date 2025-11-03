using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

public class AutoConnect : MonoBehaviour
{
    // Start is called before the first frame update
    private Photon.Pun.Demo.PunBasics.Launcher launcher;
    void Start()
    {
        launcher = GameObject.FindObjectOfType<Photon.Pun.Demo.PunBasics.Launcher>();
        launcher.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
