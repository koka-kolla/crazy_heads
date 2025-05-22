// EnemyManager.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // ────────────────────────────────────────────────
    // 1.  ЛЕНИВЫЙ СИНГЛТОН
    // ────────────────────────────────────────────────
    private static EnemyManager _instance;
    public static EnemyManager Instance
    {
        get
        {
            // если кто-то обратился раньше Awake()
            if (_instance == null)
                _instance = Object.FindFirstObjectByType<EnemyManager>();

            return _instance;
        }
        private set => _instance = value;
    }

    // ────────────────────────────────────────────────
    [Tooltip("Список активных врагов (видно в Inspector)")]
    public List<EnemyController> enemies = new List<EnemyController>();

    public int Count => enemies.Count;

    void Awake()
    {
        // фикс на случай дубликатов
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    // ────────────────────────────────────────────────
    // 2.  API РЕГИСТРАЦИИ
    // ────────────────────────────────────────────────
    public void Register(EnemyController enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void Unregister(EnemyController enemy)
    {
        enemies.Remove(enemy);
    }

    // ────────────────────────────────────────────────
    // 3.  Поиск ближайшего
    // ────────────────────────────────────────────────
    public EnemyController GetNearest(Vector2 position)
    {
        EnemyController nearest = null;
        float minDist = float.MaxValue;

        foreach (var e in enemies)
        {
            if (!e || !e.gameObject.activeInHierarchy) continue;

            float d = Vector2.Distance(position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }
        return nearest;
    }
}