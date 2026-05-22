using Photon.Pun;
using UnityEngine;

public class IsMineCheck : MonoBehaviourPun
{
    [SerializeField] private TopDownCharacterController move;
    [SerializeField] private CharacterPhysicsMotor motor;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera _camera;

    void Start()
    {
        // photonView — это свойство, доступное благодаря MonoBehaviourPun
        if (!photonView.IsMine)
        {
            // Отключаем всё лишнее у ЧУЖИХ игроков
            if (_camera != null) _camera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;

            if (move != null) move.enabled = false;
            if (motor != null) motor.enabled = false;
        }
        else
        {
            // НАШ ИГРОК: Настраиваем камеру сцены под себя
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // 1. Говорим контроллеру двигаться относительно Главной Камеры
                if (move != null && move.movementReference == null)
                {
                    move.movementReference = mainCam.transform;
                }

                // 2. Находим скрипт MMOCamera на Главной Камере и подсовываем себя в качестве цели
                MMOCamera mmoCam = mainCam.GetComponent<MMOCamera>();
                if (mmoCam != null)
                {
                    mmoCam.target = this.transform;
                }
            }
        }
    }
}