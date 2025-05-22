using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour, ICombatant
{
    [Header("Stats")]   public EnemyStatsSO stats;
    [Header("Skills")]  public List<ScriptableObject> attackObjects = new();
    [Header("UI")]      public GameObject healthBarPrefab;
                          public Transform uiCanvas;

    Rigidbody2D     _rb;
    MovementManager _mvMgr;
    HealthManager   _hpMgr;
    AttackManager   _atkMgr;
    Transform       _hero;
    Vector3         _startScale;

    public BaseStatsSO Stats      => stats;
    public Transform   Transform  => transform;
    public bool        AutoAttack => true;

    void Awake()
    {
        _rb         = GetComponent<Rigidbody2D>();
        _startScale = transform.localScale;

        _mvMgr  = new MovementManager(_rb, transform.position, _startScale, 0.05f);
        _hpMgr  = new HealthManager(stats, healthBarPrefab, uiCanvas, transform);
        _hpMgr.OnDeath += () => gameObject.SetActive(false);
        _atkMgr = new AttackManager(attackObjects) { GlobalDelay = 0.3f };
    }

    void Start()
    {
        _hero = GameManager.Instance.Hero.transform;
    }

    void FixedUpdate()
    {
        Vector2 pos     = _rb.position;
        Vector2 heroPos = _hero.position;
        float dist      = Vector2.Distance(pos, heroPos);

        Vector2 velocity;
        if (dist > stats.attackRange)
        {
            if (dist > stats.fullChaseDistance)
            {
                float dirX = Mathf.Sign(heroPos.x - pos.x);
                velocity   = new Vector2(dirX * stats.moveSpeed, 0f);
                Face(dirX);
            }
            else
            {
                Vector2 dir = (heroPos - pos).normalized;
                velocity    = dir * stats.moveSpeed;
                if (Mathf.Abs(dir.x) > 0.01f)
                    Face(Mathf.Sign(dir.x));
            }
        }
        else
        {
            velocity = Vector2.zero;
        }
        _rb.linearVelocity = velocity;
    }

    void Update() => _atkMgr.Tick(this);

    public void Knockback(Vector2 disp, float flightTime, float stunTime, float spinDeg)
        => StartCoroutine(ArcRotateKnockback(disp, flightTime, stunTime, spinDeg));

    public void Knockback(Vector2 disp, float stunTime)
        => Knockback(disp, 0f, stunTime, 0f);

    private IEnumerator ArcRotateKnockback(Vector2 disp, float flightTime, float stunTime, float spinDeg)
    {
        _rb.simulated = false;
        Vector2 start = transform.position;
        Vector2 end   = start + new Vector2(disp.x, 0f);
        float peakY   = disp.y;
        float t       = 0f;
        float rot0    = transform.eulerAngles.z;

        while (t < flightTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(flightTime, 0.0001f));
            float x = Mathf.Lerp(start.x, end.x, p);
            float y = start.y + peakY * Mathf.Sin(Mathf.PI * p);
            transform.position = new Vector2(x, y);
            if (Mathf.Abs(spinDeg) > 0.01f)
            {
                float z = Mathf.Lerp(rot0, rot0 + spinDeg, p);
                transform.eulerAngles = new Vector3(0f, 0f, z);
            }
            yield return null;
        }
        transform.position   = end;
        transform.eulerAngles = new Vector3(0f, 0f, rot0);
        yield return new WaitForSeconds(stunTime);
        _rb.position  = end;
        _rb.linearVelocity  = Vector2.zero;
        _rb.simulated = true;
    }

    public ICombatant GetEnemyTarget() => GameManager.Instance.Hero;
    public void       FaceTarget(Transform t) => _mvMgr.FaceTarget(transform, t, _startScale);

    public void TakeDamage(int dmg)
    {
        bool died = _hpMgr.TakeDamage(dmg);
        if (!died && TryGetComponent(out BounceScale bs))
            bs.TriggerBounce();
    }

    void Face(float signX)
    {
        var s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * signX, s.y, s.z);
    }
}
