using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour, ICombatant
{
    [Header("Stats")]
    public EnemyStatsSO stats;

    [Header("Skills")]
    public List<ScriptableObject> attackObjects = new();

    [Header("UI")]
    public GameObject healthBarPrefab;
    public Transform uiCanvas;

    // Менеджеры
    private Rigidbody2D     _rb;
    private MovementManager _mvMgr;
    private HealthManager   _hpMgr;
    private AttackManager   _atkMgr;
    private Transform       _hero;
    private Vector3         _startScale;

    // Флаг стана
    private bool _isStunned = false;
    // Изначальный угол Z
    private float _initialRotZ;

    // ICombatant
    public BaseStatsSO Stats      => stats;
    public Transform   Transform  => transform;
    public bool        AutoAttack => !_isStunned;

    void Awake()
    {
        _rb           = GetComponent<Rigidbody2D>();
        _startScale   = transform.localScale;
        _initialRotZ  = transform.eulerAngles.z;

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
        if (_isStunned)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 pos     = _rb.position;
        Vector2 heroPos = _hero.position;
        float   dist    = Vector2.Distance(pos, heroPos);

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
                Vector2 dir  = (heroPos - pos).normalized;
                velocity     = dir * stats.moveSpeed;
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

    void Update()
    {
        if (_isStunned) return;
        _atkMgr.Tick(this);
    }

    /// <summary>
    /// Внешний вызов стана без отбрасывания
    /// </summary>
    public void Stun(float duration)
    {
        if (!_isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        _isStunned = true;
        yield return new WaitForSeconds(duration);

        // Сброс поворота
        transform.eulerAngles = new Vector3(0f, 0f, _initialRotZ);
        _isStunned = false;
    }

    /// <summary>
    /// Отбрасывание + вращение + стан
    /// </summary>
    public void Knockback(Vector2 disp, float flightTime, float stunTime, float spinDeg)
        => StartCoroutine(ArcRotateKnockback(disp, flightTime, stunTime, spinDeg));

    private IEnumerator ArcRotateKnockback(Vector2 disp, float flightTime, float stunTime, float spinDeg)
    {
        _isStunned    = true;
        _rb.simulated = false;  // временно отключаем физику

        Vector2 start = transform.position;
        Vector2 end   = start + new Vector2(disp.x, 0f);
        float   peakY = disp.y;
        float   t     = 0f;
        float   rot0  = _initialRotZ;

        // Полёт + вращение
        while (t < flightTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / Mathf.Max(flightTime, 0.0001f));

            // движение по параболе
            float x = Mathf.Lerp(start.x, end.x, p);
            float y = start.y + peakY * Mathf.Sin(Mathf.PI * p);
            transform.position = new Vector2(x, y);

            // ревёрсивный spin: наклон и возврат
            if (Mathf.Abs(spinDeg) > 0.01f)
            {
                float spinT = Mathf.Sin(Mathf.PI * p); // 0→1→0
                float z     = rot0 + spinDeg * spinT;
                transform.eulerAngles = new Vector3(0f, 0f, z);
            }

            yield return null;
        }

        // Фиксируем конечное положение и сбрасываем поворот
        transform.position    = end;
        transform.eulerAngles = new Vector3(0f, 0f, _initialRotZ);

        // Ждём стан
        yield return new WaitForSeconds(stunTime);

        // Включаем физику и сбрасываем флаг
        _rb.position        = end;
        _rb.linearVelocity        = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.simulated       = true;
        _isStunned          = false;
    }

    public ICombatant GetEnemyTarget() => GameManager.Instance.Hero;

    public void FaceTarget(Transform t) => _mvMgr.FaceTarget(transform, t, _startScale);

    public void TakeDamage(int dmg)
    {
        bool died = _hpMgr.TakeDamage(dmg);
        if (!died && TryGetComponent(out BounceScale bs))
            bs.TriggerBounce();
    }

    private void Face(float signX)
    {
        var s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * signX, s.y, s.z);
    }
}
