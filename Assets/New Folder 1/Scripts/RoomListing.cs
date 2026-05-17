using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private Button connectButton;

    private string _currentRoomName;

    private void Awake()
    {
        // Подписываемся в Awake один раз. Это предотвращает утечки памяти 
        // и проблемы с замыканиями (closures), которые были в старом коде.
        connectButton.onClick.AddListener(ConnectToRoom);
    }

    public void SetRoomInfo(RoomInfo info)
    {
        _currentRoomName = info.Name;
        roomNameText.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
    }

    private void ConnectToRoom()
    {
        PhotonNetwork.JoinRoom(_currentRoomName);
    }
}