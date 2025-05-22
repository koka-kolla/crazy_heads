using UnityEngine;

/// <summary>
/// Базовые статы врага + порог переключения преследования.
/// </summary>
[CreateAssetMenu(menuName = "Stats/Enemy")]
public class EnemyStatsSO : BaseStatsSO
{
    [Header("Chase Settings")]
    [Tooltip("Distance at which enemy switches from horizontal chase to full 2D chase")]
    [Min(0)] public float fullChaseDistance = 3f;
}
