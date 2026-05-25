using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DisconnectUIManager : MonoBehaviourPunCallbacks
{
    [Header("Disconnect UI Objects")]
    [SerializeField] private GameObject[] objectsOnDisconnect;

    [Header("Menu Buttons")]
    [SerializeField] private Button[] menuButtons;

    [Header("Scene")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Debug")]
    [SerializeField] private bool forceDisconnect = false;

    private bool alreadyForceDisconnected;
    private string lastRoomName;

    void Start()
    {
        SetDisconnectObjects(false);

        // подписываем ВСЕ кнопки меню
        if (menuButtons != null)
        {
            foreach (var btn in menuButtons)
            {
                if (btn != null)
                    btn.onClick.AddListener(GoMenu);
            }
        }
    }

    void Update()
    {
        // 🧪 тестовый дисконнект
        if (forceDisconnect && !alreadyForceDisconnected)
        {
            alreadyForceDisconnected = true;
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnJoinedRoom()
    {
        lastRoomName = PhotonNetwork.CurrentRoom.Name;
        SetDisconnectObjects(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("DISCONNECTED: " + cause);

        SetDisconnectObjects(true);
        alreadyForceDisconnected = false;

        // 👉 только включаем курсор
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetDisconnectObjects(bool state)
    {
        if (objectsOnDisconnect == null) return;

        foreach (var obj in objectsOnDisconnect)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }

    private void GoMenu()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(menuSceneName);
    }

    [ContextMenu("TEST DISCONNECT NOW")]
    private void TestDisconnect()
    {
        PhotonNetwork.Disconnect();
    }
}