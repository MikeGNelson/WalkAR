using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tracker : MonoBehaviourPunCallbacks
{
    public Transform head;
    public Transform left;
    public Transform right;
    public OVRCameraRig ovrCameraRig;  // Reference to the OVR Camera Rig

    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (ovrCameraRig == null)
        {
            ovrCameraRig = FindObjectOfType<OVRCameraRig>(); // Find the OVR Camera Rig if not assigned
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            head.gameObject.SetActive(true);
            left.gameObject.SetActive(true);
            right.gameObject.SetActive(true);

            // Map positions based on OVR Camera Rig anchors
            MapPosition(head, ovrCameraRig.centerEyeAnchor);
            MapPosition(left, ovrCameraRig.leftHandAnchor);
            MapPosition(right, ovrCameraRig.rightHandAnchor);

            // Optionally, send position/rotation data over the network if needed
            photonView.RPC("SyncTransform", RpcTarget.Others, head.position, head.rotation, left.position, left.rotation, right.position, right.rotation);
        }
    }

    void MapPosition(Transform target, Transform source)
    {
        target.position = source.position;
        target.rotation = source.rotation;
    }

    [PunRPC]
    void SyncTransform(Vector3 headPosition, Quaternion headRotation, Vector3 leftPosition, Quaternion leftRotation, Vector3 rightPosition, Quaternion rightRotation)
    {
        head.position = headPosition;
        head.rotation = headRotation;
        left.position = leftPosition;
        left.rotation = leftRotation;
        right.position = rightPosition;
        right.rotation = rightRotation;
    }
}
