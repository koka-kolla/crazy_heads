using UnityEngine;

public interface ICombatant
{
    Transform   Transform   { get; }
    BaseStatsSO Stats       { get; }
    bool        AutoAttack  { get; }

    ICombatant  GetEnemyTarget();
    void        FaceTarget(Transform trg);
    void        TakeDamage(int dmg);
}
