using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private bool isConnecting;
    public static NetworkManager _instance;

    public bool playerJoined;

    public static NetworkManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.Log("destroyed");
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Connect();
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            isConnecting = PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            Debug.Log("Connected to Master: " + PhotonNetwork.CloudRegion);
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
    }

    public void CreateRoom(string roomName, string sceneName)
    {
        PhotonNetwork.CreateRoom(roomName);
        PhotonNetwork.LoadLevel(sceneName);
    }

    public void JoinRoom(string roomName, string sceneName)
    {
        PhotonNetwork.JoinRoom(roomName);
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        if (SceneManager.GetActiveScene().name == "Title")
        {
            PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        }
    }

    public void ChangeScene(string sceneName)
    {
        Debug.Log("Scene changed to " + sceneName);
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed starting app.");
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master client migrated to " + newMasterClient);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        PhotonNetwork.CreateRoom("Capstone");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("New Player entered: " + newPlayer);
        playerJoined = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log("Player " + otherPlayer + " has left the room");
    }
}