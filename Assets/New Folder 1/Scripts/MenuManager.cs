using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField createField; // Поле для создания комнаты
    public TMP_InputField joinField;   // Поле для входа в комнату
    public GameObject roomListContent; // Контейнер для элементов списка (Content Scroll View)
    public GameObject roomListItemPrefab; // Префаб для элемента списка комнаты с TMP_Text
    public int maxPlayers = 10;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined; // Захватываем курсор
        PhotonNetwork.JoinLobby(); // Подключаемся к лобби для получения списка комнат
    }

    // Метод создания комнаты
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        PhotonNetwork.CreateRoom(createField.text, roomOptions, null);
        //PhotonNetwork.CreateRoom(createField.text);
    }

    // Метод присоединения к комнате через поле
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    // Этот метод вызывается каждый раз, когда список комнат обновляется
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Очищаем старые данные в Scroll View, чтобы не было дубликатов
        foreach (Transform child in roomListContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Проходим по каждой комнате в списке и добавляем её в Scroll View
        foreach (RoomInfo room in roomList)
        {
            // Создаем новый элемент из префаба
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListContent.transform);

            // Получаем компонент TMP_Text на созданном объекте
            TMP_Text roomText = roomItem.GetComponent<TMP_Text>();

            // Устанавливаем текст: имя комнаты и количество игроков
            roomText.text = room.Name + " (" + room.PlayerCount + "/" + room.MaxPlayers + ")";

            // Находим кнопку (дочерний объект с именем "Con")
            Button joinButton = roomItem.transform.Find("Con").GetComponent<Button>();

            // Привязываем действие нажатия кнопки к присоединению к конкретной комнате
            string roomName = room.Name; // Копируем имя комнаты в отдельную переменную, чтобы избежать проблем с замыканиями
            joinButton.onClick.AddListener(() => JoinSpecificRoom(roomName));
        }
    }

    // Метод для присоединения к конкретной комнате по ее имени
    private void JoinSpecificRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}
