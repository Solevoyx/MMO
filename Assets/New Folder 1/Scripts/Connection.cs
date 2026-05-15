using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class Connection : MonoBehaviourPunCallbacks
{
    [SerializeField] private string sceneName; //имя сцены меню
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server, succesful!");
        SceneManager.LoadScene(sceneName);
    }
}
