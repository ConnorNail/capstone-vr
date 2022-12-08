using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;

public class HandSync : MonoBehaviour, IOnEventCallback
{
    public const byte updatePoseEventCode = 1;
    public const byte updateButtonsEventCode = 2;
    public const byte updateJointStateEventCode = 3;

    public GameObject rightHand;

    private float[] rightPose;
    private bool[] buttons;
    private float[] rightVelocities;

    private float lastXPos;
    private float lastYPos;
    private float lastZPos;

    private Quaternion deltaRotation;
    private Quaternion lastRotation;

    //public float[] jointState;
    public List<RosSharp.RosBridgeClient.JointStateWriter> JointStateWriters;

    // Start is called before the first frame update
    void Start()
    {
        rightPose = new float[6];
        buttons = new bool[3];
        rightVelocities = new float[6];

        lastXPos = rightHand.transform.localPosition.x;
        lastYPos = rightHand.transform.localPosition.y;
        lastZPos = rightHand.transform.localPosition.z;
        lastRotation = rightHand.transform.rotation;
        //jointState = new float[6];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        sendVelocityData();
        sendButtonUpdates();
    }

    private void sendVelocityData()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Position
            rightVelocities[0] = (rightHand.transform.localPosition.x - lastXPos) / Time.deltaTime;
            rightVelocities[1] = (rightHand.transform.localPosition.y - lastYPos) / Time.deltaTime;
            rightVelocities[2] = (rightHand.transform.localPosition.z - lastZPos) / Time.deltaTime;

            lastXPos = rightHand.transform.localPosition.x;
            lastYPos = rightHand.transform.localPosition.y;
            lastZPos = rightHand.transform.localPosition.z;

            // Rotation
            deltaRotation = rightHand.transform.rotation * Quaternion.Inverse(lastRotation);
            deltaRotation.ToAngleAxis(out var angle, out var axis);

            angle *= Mathf.Deg2Rad;

            rightVelocities[3] = (1.0f / Time.deltaTime) * angle * axis.x;
            rightVelocities[4] = (1.0f / Time.deltaTime) * angle * axis.y;
            rightVelocities[5] = (1.0f / Time.deltaTime) * angle * axis.z;

            lastRotation = rightHand.transform.rotation;

            RaiseEventOptions raiseEvent = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(updatePoseEventCode, rightVelocities, raiseEvent, SendOptions.SendReliable);
        }
    }

    private void sendButtonUpdates()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            buttons[0] = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
            buttons[1] = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            buttons[2] = OVRInput.Get(OVRInput.RawButton.A);
            RaiseEventOptions raiseEvent = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(updateButtonsEventCode, buttons, raiseEvent, SendOptions.SendReliable);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == updatePoseEventCode)
        {
            Debug.Log("Updating location from master...");
            float[] poseData = (float[])photonEvent.CustomData;
            rightHand.transform.position = new Vector3(poseData[0], poseData[1], poseData[2]);
            rightHand.transform.rotation = Quaternion.Euler(new Vector3(poseData[3], poseData[4], poseData[5]));
        }

        if (eventCode == updateJointStateEventCode)
        {
            Debug.Log("Updating location from master...");
            float[] jointStateData = (float[])photonEvent.CustomData;
            //jointState[0] = jointStateData[0];
            //jointState[1] = jointStateData[1];
            //jointState[2] = jointStateData[2];
            //jointState[3] = jointStateData[3];
            //jointState[4] = jointStateData[4];
            //jointState[5] = jointStateData[5];
            JointStateWriters[0].Write((float) jointStateData[0]);
            JointStateWriters[1].Write((float) jointStateData[1]);
            JointStateWriters[2].Write((float) jointStateData[2]);
            JointStateWriters[3].Write((float) jointStateData[3]);
            JointStateWriters[4].Write((float) jointStateData[4]);
            JointStateWriters[5].Write((float) jointStateData[5]);
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
