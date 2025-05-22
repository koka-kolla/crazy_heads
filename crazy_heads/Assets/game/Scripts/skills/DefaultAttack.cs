using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Default")]
public class DefaultAttack : ScriptableObject, ICharacterAttack
{
    public string AttackName => "Default";
    public int    damage   = 10;
    public float  cooldown = 0.5f;
    float ICharacterAttack.Cooldown => cooldown;

    public void Execute(ICombatant owner)
    {
        var tgt = owner.GetEnemyTarget();
        if (tgt == null) return;

        if (Vector2.Distance(owner.Transform.position, tgt.Transform.position)
            <= owner.Stats.attackRange)
            tgt.TakeDamage(damage);
    }
}
