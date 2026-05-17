using Photon.Pun;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField, Tooltip("Точное имя префаба игрока в папке Resources")]
    private string prefabName = "PlayerPrefab";

    [SerializeField] private Transform _spawn;

    private void Start()
    {
        // Теперь переименование объекта на сцене не сломает спавн
        PhotonNetwork.Instantiate(prefabName, _spawn.position, Quaternion.identity);
    }
}