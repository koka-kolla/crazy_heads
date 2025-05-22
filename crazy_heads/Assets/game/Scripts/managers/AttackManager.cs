using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager
{
    public event Action<int, float> OnAttackExecuted;

    readonly List<ICharacterAttack> _attacks = new();
    readonly float[] _lastTimes;

    public float GlobalDelay = .3f;
    float _lastAnyTime = float.NegativeInfinity;
    int   _lastSkill   = -1;

    public AttackManager(IEnumerable<ScriptableObject> sos)
    {
        foreach (var so in sos)
            if (so is ICharacterAttack a) _attacks.Add(a);

        _lastTimes = new float[_attacks.Count];
        for (int i = 0; i < _lastTimes.Length; i++)
            _lastTimes[i] = float.NegativeInfinity;
    }

    public bool TryAttack(int idx, ICombatant owner)
    {
        Debug.Log($"TryAttack: index={idx}, cdRemaining={(Time.time - _lastAnyTime < GlobalDelay ? GlobalDelay - (Time.time - _lastAnyTime) : 0f)}");
        
        if (_attacks.Count == 0)
        {
            Debug.Log("  → Отказ: нет атак в списке");
            return false;
        }
        if (idx < 0 || idx >= _attacks.Count)
        {
            Debug.Log("  → Отказ: индекс вне диапазона");
            return false;
        }
        if (idx != _lastSkill && Time.time - _lastAnyTime < GlobalDelay)
        {
            Debug.Log("  → Отказ: глобальная задержка ещё не прошла");
            return false;
        }

        var tgt = owner.GetEnemyTarget();
        if (tgt == null)
        {
            Debug.Log("  → Отказ: нет цели");
            return false;
        }

        if ((tgt.Transform.position - owner.Transform.position).sqrMagnitude >
            owner.Stats.attackRange * owner.Stats.attackRange)
        {
            Debug.Log("  → Отказ: вне диапазона атаки");
            return false;
        }

        var atk = _attacks[idx];
        var timeSinceLast = Time.time - _lastTimes[idx];
        if (timeSinceLast < atk.Cooldown)
        {
            Debug.Log($"  → Отказ: кулдаун ({atk.Cooldown - timeSinceLast:F2}s осталось)");
            return false;
        }

        owner.FaceTarget(tgt.Transform);
        atk.Execute(owner);

        _lastTimes[idx] = _lastAnyTime = Time.time;
        _lastSkill      = idx;
        OnAttackExecuted?.Invoke(idx, atk.Cooldown);

        Debug.Log("  → Атака выполнена успешно");
        return true;
    }

    public void Tick(ICombatant owner)
    {
        if (_attacks.Count == 0) return;
        if (!owner.AutoAttack) return;
        if (Time.time - _lastAnyTime < GlobalDelay) return;

        int start = (_lastSkill + 1) % _attacks.Count;
        for (int o = 0; o < _attacks.Count; o++)
            if (TryAttack((start + o) % _attacks.Count, owner))
                break;
    }
}
