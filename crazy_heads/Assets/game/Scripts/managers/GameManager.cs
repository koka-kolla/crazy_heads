// GameManager.cs
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public HeroController Hero { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // ищем героя автоматически, если не назначили в инспекторе
        if (Hero == null)
            Hero = Object.FindFirstObjectByType<HeroController>();
    }
}
