using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [Header("Inputs")]
    public TMP_InputField createField;
    public TMP_InputField joinField;

    [Header("Buttons")]
    public Button createButton;
    public Button joinButton;

    [Header("Room List UI")]
    public GameObject roomListContent;
    public GameObject roomListItemPrefab;

    [Header("Settings")]
    public int maxPlayers = 10;
    public string sceneToLoad = "GameScene"; // ← сцена для загрузки (задаётся в инспекторе)

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        PhotonNetwork.JoinLobby();

        // привязка кнопок
        if (createButton != null)
            createButton.onClick.AddListener(CreateRoom);

        if (joinButton != null)
            joinButton.onClick.AddListener(JoinRoom);
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;

        PhotonNetwork.CreateRoom(createField.text, roomOptions, null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(sceneToLoad);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo room in roomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListContent.transform);

            TMP_Text roomText = roomItem.GetComponent<TMP_Text>();
            roomText.text = room.Name + " (" + room.PlayerCount + "/" + room.MaxPlayers + ")";

            Button joinBtn = roomItem.transform.Find("Con").GetComponent<Button>();

            string roomName = room.Name;
            joinBtn.onClick.AddListener(() => JoinSpecificRoom(roomName));
        }
    }

    private void JoinSpecificRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}