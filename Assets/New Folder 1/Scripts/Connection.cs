using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connection : MonoBehaviourPunCallbacks
{
    [SerializeField] private string sceneName;

    [Header("Reconnect Settings")]
    [SerializeField] private int maxReconnectTries = 5;
    [SerializeField] private float reconnectDelay = 2f;

    [Header("Objects")]
    [SerializeField] private GameObject[] connectedObjects;
    [SerializeField] private GameObject[] disconnectedObjects;

    private int reconnectTries = 0;
    private bool isConnecting = false;

    void Start()
    {
        SetConnectionState(false);
        Connect();
    }

    private void Connect()
    {
        if (isConnecting)
            return;

        isConnecting = true;

        Debug.Log("Trying to connect to Photon...");

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server successfully!");

        reconnectTries = 0;
        isConnecting = false;

        SetConnectionState(true);

        SceneManager.LoadScene(sceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause);

        isConnecting = false;

        SetConnectionState(false);

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

    private void SetConnectionState(bool connected)
    {
        // ¬ключаютс€ при подключении
        foreach (GameObject obj in connectedObjects)
        {
            if (obj != null)
                obj.SetActive(connected);
        }

        // ¬ключаютс€ при отключении
        foreach (GameObject obj in disconnectedObjects)
        {
            if (obj != null)
                obj.SetActive(!connected);
        }
    }
}