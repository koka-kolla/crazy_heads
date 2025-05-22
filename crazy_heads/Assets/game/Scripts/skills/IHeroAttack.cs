public interface IHeroAttack
{
    string AttackName { get; }
    float Cooldown { get; }
    void Execute(HeroController hero);
}
