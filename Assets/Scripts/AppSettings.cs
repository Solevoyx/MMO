using UnityEngine;

public class AppSettings : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }
}