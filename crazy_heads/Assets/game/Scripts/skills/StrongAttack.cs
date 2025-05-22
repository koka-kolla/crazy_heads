// Assets/Scripts/Attack/StrongAttack.cs
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Strong", fileName = "StrongAttack")]
public class StrongAttack : ScriptableObject, ICharacterAttack
{
    [Header("Basic Info")]
    [SerializeField] private string attackName = "Strong";
    [SerializeField] private float  cooldown   = 2f;
    public string AttackName => attackName;
    public float  Cooldown   => cooldown;

    [Header("Damage")]
    [SerializeField] private int damage = 15;

    [Header("Hero Jump")]
    [SerializeField] private float heroJumpHeight   = 2f;
    [SerializeField] private float heroJumpDuration = 0.7f;

    [Header("Enemy Flight")]
    [SerializeField] private float enemyXOffset     = 0.6f;
    [SerializeField] private float enemyArcHeight   = 4f;
    [SerializeField] private float enemyArcDuration = 0.7f;
    [Tooltip("Градусы вращения врага во время полёта (абсолютное значение)")]
    [SerializeField] private float spinDegrees      = 180f;
    [SerializeField] private bool  invertSpin       = false;

    [Header("Stun")]
    [SerializeField] private float stunDuration = 1.5f;

    public void Execute(ICombatant owner)
    {
        var hero = (owner as MonoBehaviour)?.GetComponent<HeroController>();
        if (hero == null) return;

        var target = hero.GetEnemyTarget();
        if (target == null) return;

        // не бить, если цель уже в воздухе
        var rbT = (target as MonoBehaviour)?.GetComponent<Rigidbody2D>();
        if (rbT != null && Mathf.Abs(rbT.linearVelocity.y) > 0.1f)
            return;

        hero.StartCoroutine(PerformStrong(hero, target));
    }

    private IEnumerator PerformStrong(HeroController hero, ICombatant target)
    {
        // 1) Урон
        target.TakeDamage(damage);

        // Кэшируем стартовые данные
        Transform heroT    = hero.transform;
        Vector3 heroStart  = heroT.position;
        var     mbTarget   = (MonoBehaviour)target;
        Transform enemyT   = mbTarget.transform;
        Vector3 enemyStart = enemyT.position;

        float signX    = Mathf.Sign(enemyStart.x - heroStart.x);
        float baseSpin = invertSpin ? -spinDegrees : spinDegrees;
        float spinDeg  = -signX * baseSpin;
        float rot0     = enemyT.eulerAngles.z;

        // 2) Анимация прыжка героя и полёта + вращения врага
        float elapsed = 0f;
        float maxDur  = Mathf.Max(heroJumpDuration, enemyArcDuration);
        while (elapsed < maxDur)
        {
            // прыжок героя
            if (elapsed <= heroJumpDuration)
            {
                float tH = elapsed / heroJumpDuration;
                float yH = 4f * heroJumpHeight * tH * (1f - tH);
                heroT.position = heroStart + Vector3.up * yH;
            }

            // полёт врага и вращение
            if (elapsed <= enemyArcDuration)
            {
                float tE = elapsed / enemyArcDuration;
                float xE = Mathf.Lerp(enemyStart.x, enemyStart.x + signX * enemyXOffset, tE);
                float yE = enemyStart.y + enemyArcHeight * Mathf.Sin(Mathf.PI * tE);
                enemyT.position = new Vector3(xE, yE, enemyStart.z);

                float z = Mathf.Lerp(rot0, rot0 + spinDeg, tE);
                enemyT.eulerAngles = new Vector3(0f, 0f, z);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3) Герой возвращается
        heroT.position = heroStart;

        // 4) Враг остаётся отброшенным
        Vector3 enemyEndPos = enemyT.position;

        // 5) Вызываем Stun у контроллера
        var ec = mbTarget.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.Stun(stunDuration);
        }
    }
}
