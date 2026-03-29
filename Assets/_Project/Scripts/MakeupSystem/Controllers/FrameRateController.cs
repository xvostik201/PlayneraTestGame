using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 90;
        QualitySettings.vSyncCount = 0;
    }
}