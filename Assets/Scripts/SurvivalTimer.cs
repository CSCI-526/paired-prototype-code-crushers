using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    public static SurvivalTimer Instance;

    float startTime;
    float stopTime;
    bool running = true;

    public float ElapsedSeconds => running ? (Time.time - startTime) : (stopTime - startTime);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetTimer();
    }

    public void ResetTimer()
    {
        startTime = Time.time;
        running = true;
    }

    public void Stop()
    {
        if (!running) return;
        stopTime = Time.time;
        running = false;
    }

    public static string Format(float seconds)
    {
        int m = (int)(seconds / 60f);
        int s = (int)(seconds % 60f);
        return $" - {m:00}:{s:00}";
    }
}
