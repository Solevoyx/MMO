using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsMineCheck : MonoBehaviour
{
    [SerializeField] private SC_FPSController move;
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private Camera _camera;

    void Start()
    {
        if (!_photonView.IsMine)
        {
            _camera.enabled = false;
            move.enabled = false;
            audioListener.enabled = false;
        }
    }
}
