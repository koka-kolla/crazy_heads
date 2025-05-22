using System;
using UnityEngine;

/// <summary>Поддерживает HP и полосу здоровья, сообщает о смерти.</summary>
public class HealthManager
{
    public event Action OnDeath;          // подписчики: контроллеры

    public  int CurrentHP { get; private set; }

    private readonly BaseStatsSO _stats;
    private readonly HealthBar   _bar;    // сам UI-скрипт
    private readonly GameObject  _barGO;  // корневой объект полосы

    public HealthManager(BaseStatsSO stats,
                         GameObject barPrefab,
                         Transform canvas,
                         Transform follow)
    {
        _stats    = stats;
        CurrentHP = stats.maxHP;

        if (barPrefab && canvas && follow)
        {
            _barGO = UnityEngine.Object.Instantiate(barPrefab, canvas);
            _bar   = _barGO.GetComponent<HealthBar>();
            _bar.objectToFollow = follow;
            _bar.SetHealth(CurrentHP, _stats.maxHP);
        }
    }

    /// <summary>Наносит урон. Возвращает true, если боец умер.</summary>
    public bool TakeDamage(int dmg)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        _bar?.SetHealth(CurrentHP, _stats.maxHP);

        if (CurrentHP <= 0)
        {
            if (_barGO) UnityEngine.Object.Destroy(_barGO);   // ← удаляем UI-бар
            OnDeath?.Invoke();
            return true;
        }
        return false;
    }
}
