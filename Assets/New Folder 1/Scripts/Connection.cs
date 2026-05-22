using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connection : MonoBehaviourPunCallbacks
{
    [SerializeField] private string sceneName; // имя сцены меню

    [Header("Reconnect Settings")]
    [SerializeField] private int maxReconnectTries = 5;
    [SerializeField] private float reconnectDelay = 2f;

    private int reconnectTries = 0;
    private bool isConnecting = false;

    void Start()
    {
        Connect();
    }

    private void Connect()
    {
        if (isConnecting) return;

        isConnecting = true;
        Debug.Log("Trying to connect to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server successfully!");

        reconnectTries = 0;
        isConnecting = false;

        SceneManager.LoadScene(sceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause);

        isConnecting = false;

        if (reconnectTries < maxReconnectTries)
        {
            reconnectTries++;
            StartCoroutine(Reconnect());
        }
        else
        {
            Debug.Log("Max reconnect attempts reached.");
        }
    }

    private IEnumerator Reconnect()
    {
        Debug.Log($"Reconnecting... attempt {reconnectTries}/{maxReconnectTries}");

        yield return new WaitForSeconds(reconnectDelay);

        Connect();
    }
}