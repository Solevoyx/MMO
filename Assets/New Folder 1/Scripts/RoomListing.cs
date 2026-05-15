using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    public TMP_Text roomNameText; // Ссылка на TMP_Text для отображения имени комнаты
    public Button connectButton; // Ссылка на кнопку подключения

    private RoomInfo roomInfo; // Информация о комнате

    // Метод для инициализации списка комнат
    public void SetRoomInfo(RoomInfo info)
    {
        roomInfo = info;
        roomNameText.text = roomInfo.Name; // Устанавливаем имя комнаты

        // Добавляем слушатель нажатия кнопки
        connectButton.onClick.AddListener(() => ConnectToRoom());
    }

    // Метод для подключения к комнате
    private void ConnectToRoom()
    {
        PhotonNetwork.JoinRoom(roomInfo.Name); // Подключаемся к комнате по имени
    }
}
