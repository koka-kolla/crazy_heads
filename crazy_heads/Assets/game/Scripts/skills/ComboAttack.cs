using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Attack/Combo")]
public class ComboAttack : ScriptableObject, ICharacterAttack
{
    public string AttackName => "Combo";

    [Header("Series")]
    [Min(1)] public int   hits         = 3;
    [Min(1)] public int   damagePerHit = 6;
    [Min(0)] public float interval     = 0.15f;

    [Header("Hero Step FX")]
    [Min(0)] public float stepDistance = 0.25f;
    [Min(0)] public float stepDuration = 0.06f;

    [Header("Enemy Knockback")]
    [Min(0)] public float pushXDistance = 0.6f;
    [Min(0)] public float pushYDistance = 0.2f;
    [Tooltip("Flight time: duration of the up-and-down arc (sec)")]
    [Min(0)] public float flyTime       = 0.1f;
    [Tooltip("Stun duration: time enemy remains stunned after landing (sec)")]
    [Min(0)] public float stunTime      = 0.4f;
    [Tooltip("Degrees to spin during flight (absolute value)")]
    [Min(0)] public float spinDegrees   = 180f;
    [Tooltip("Invert overall spin direction when true")]
    public bool  invertSpin  = false;

    [Header("Cooldown")]
    [Min(0)] public float cooldown      = 2f;
    float ICharacterAttack.Cooldown    => cooldown;

    public void Execute(ICombatant owner)
    {
        var mb = owner.Transform.GetComponent<MonoBehaviour>();
        if (mb) mb.StartCoroutine(PlayCombo(owner));
    }

    private IEnumerator PlayCombo(ICombatant owner)
    {
        var rbHero = owner.Transform.GetComponent<Rigidbody2D>();

        for (int i = 0; i < hits; i++)
        {
            var target = owner.GetEnemyTarget();
            if (target == null) yield break;

            float signX = Mathf.Sign(target.Transform.position.x - owner.Transform.position.x);
            Vector2 dir = new Vector2(signX, 0f);

            // Hero steps forward
            if (rbHero)
            {
                Vector2 a = rbHero.position;
                Vector2 b = a + dir * stepDistance;
                for (float t = 0; t < stepDuration; t += Time.deltaTime)
                {
                    rbHero.MovePosition(Vector2.Lerp(a, b, t / stepDuration));
                    yield return null;
                }
                rbHero.MovePosition(b);
            }

            // Apply damage
            if (Mathf.Abs(owner.Transform.position.x - target.Transform.position.x)
                <= owner.Stats.attackRange + 0.1f)
            {
                target.TakeDamage(damagePerHit);
            }

            // Calculate spin direction based on hit side
            float baseSpin = invertSpin ? -spinDegrees : spinDegrees;
            float spin     = -signX * baseSpin;

            // Knockback + spin
            Vector2 disp = new Vector2(signX * pushXDistance, pushYDistance);
            if (target is EnemyController ec)
                ec.Knockback(disp, flyTime, stunTime, spin);

            yield return new WaitForSeconds(interval);
        }
    }
}
