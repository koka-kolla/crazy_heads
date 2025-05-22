public interface ICharacterAttack
{
    string AttackName { get; }
    float  Cooldown   { get; }
    void   Execute(ICombatant owner);
}
