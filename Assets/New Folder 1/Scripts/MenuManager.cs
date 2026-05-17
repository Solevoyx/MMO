using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField createField;
    public TMP_InputField joinField;
    public Transform roomListContent;

    // Ссылаемся сразу на компонент RoomListing, а не на GameObject
    public RoomListing roomListItemPrefab;
    public int maxPlayers = 10;

    [SerializeField] private string gameSceneName = "Game"; // Убрали магическую строку "1"

    // Список для пулинга объектов (кэшируем UI элементы)
    private List<RoomListing> _roomListings = new List<RoomListing>();

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        PhotonNetwork.JoinLobby();
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(createField.text, roomOptions, null);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 1. Скрываем все плашки (без Destroy!)
        foreach (var item in _roomListings)
        {
            item.gameObject.SetActive(false);
        }

        // 2. Обновляем нужные или создаем новые
        int index = 0;
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue; // Игнорируем закрытые комнаты

            RoomListing roomItem;

            // Если плашек не хватает — создаем новую
            if (index >= _roomListings.Count)
            {
                roomItem = Instantiate(roomListItemPrefab, roomListContent);
                _roomListings.Add(roomItem);
            }
            else
            {
                // Иначе берем существующую
                roomItem = _roomListings[index];
                roomItem.gameObject.SetActive(true);
            }

            roomItem.SetRoomInfo(room);
            index++;
        }
    }
}