using UnityEngine;

public abstract class BaseStatsSO : ScriptableObject
{
    [Header("Basic")]
    [Min(1)] public int maxHP = 100;

    [Header("Movement")]
    [Min(0)] public float moveSpeed   = 3f;
    [Min(0)] public float attackRange = 1.5f;
}
