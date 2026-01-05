using UnityEngine;

public class LoseVibrationListener : MonoBehaviour
{
    private bool vibrated = false;

    void OnEnable()
    {
        // Lose panel ilk açıldığında çalışır
        VibrateOnce();
    }

    void VibrateOnce()
    {
        if (vibrated) return;

        vibrated = true;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    void OnDisable()
    {
        // yeniden oynandığında tekrar olur
        vibrated = false;
    }
}