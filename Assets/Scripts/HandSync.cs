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

    public GameObject rightHand;

    private float[] rightPose;
    private bool[] buttons;
    // Start is called before the first frame update
    void Start()
    {
        rightPose = new float[6];
        buttons = new bool[3];
    }

    // Update is called once per frame
    void Update()
    {
        sendMapUpdates();
        sendButtonUpdates();
    }

    private void sendMapUpdates()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            rightPose[0] = rightHand.transform.localPosition.x;
            rightPose[1] = rightHand.transform.localPosition.y;
            rightPose[2] = rightHand.transform.localPosition.z;
            rightPose[3] = rightHand.transform.rotation.eulerAngles.x;
            rightPose[4] = rightHand.transform.rotation.eulerAngles.y;
            rightPose[5] = rightHand.transform.rotation.eulerAngles.z;
            RaiseEventOptions raiseEvent = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(updatePoseEventCode, rightPose, raiseEvent, SendOptions.SendReliable);
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
