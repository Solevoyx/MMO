using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCont : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private int maxPlayers = 10;
    public string sceneToLoad = "Game";
    private void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(null, roomOptions, null);
    }

    public void QuickMatch()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }


    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(sceneToLoad);
        Debug.Log("Connected to room");
    }
}
