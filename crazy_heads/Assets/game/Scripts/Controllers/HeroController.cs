using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HeroController : MonoBehaviour, ICombatant
{
    /* -------- Inspector -------- */
    [Header("Stats")]
    [SerializeField] private HeroStatsSO stats;

    [Header("Skills")]
    [SerializeField] private List<ScriptableObject> attackObjects = new();

    [Header("UI")]
    [SerializeField] private Transform         uiCanvas;
    [SerializeField] private GameObject        healthBarPrefab;
    [SerializeField] private SkillCooldownUI[] skillCooldownUIs;

    [Header("Auto-attack")]
    [SerializeField] private bool  autoAttackEnabled;
    [SerializeField] private float autoAttackInterval = 0.3f;

    /* -------- Managers -------- */
    private Rigidbody2D     _rb;
    private HealthManager   _hpMgr;
    private MovementManager _mvMgr;
    private AttackManager   _atkMgr;

    /* -------- Internals -------- */
    private Transform _currentTarget;
    private Vector3   _startScale;

    /* -------- ICombatant -------- */
    public BaseStatsSO Stats      => stats;
    public Transform   Transform  => transform;
    public bool        AutoAttack => autoAttackEnabled;

    /* -------- Mono -------- */
    void Awake()
    {
        _rb         = GetComponent<Rigidbody2D>();
        _startScale = transform.localScale;

        _hpMgr = new HealthManager(stats, healthBarPrefab, uiCanvas, transform);
        _hpMgr.OnDeath += Die;                                    

        _mvMgr = new MovementManager(_rb, transform.position, _startScale, 0.05f);

        _atkMgr = new AttackManager(attackObjects) { GlobalDelay = autoAttackInterval };
        _atkMgr.OnAttackExecuted += (i, cd) =>
        {
            if (i < skillCooldownUIs.Length && skillCooldownUIs[i])
                skillCooldownUIs[i].StartCooldown(cd);
        };
    }

    void FixedUpdate()
    {
        if (_currentTarget && !_currentTarget.gameObject.activeInHierarchy)
            _currentTarget = null;

        _mvMgr.HandleMovement(_currentTarget, stats.attackRange, stats.moveSpeed);
    }

    void Update() => _atkMgr.Tick(this);

    /* -------- UI buttons -------- */
    public void OnSkillButton(int idx)
    {
        Debug.Log($"OnSkillButton: нажата кнопка {idx}");  // ← добавили лог :contentReference[oaicite:0]{index=0}
        _atkMgr.TryAttack(idx, this);
    }

    public bool  AutoAttackEnabled => autoAttackEnabled;
    public void  ToggleAutoAttack()  => autoAttackEnabled = !autoAttackEnabled;

    /* -------- AgroZone callback -------- */
    public void SetTarget(Transform trg)
    {
        if (_currentTarget == null) _currentTarget = trg;
    }

    /* -------- ICombatant helpers -------- */
    public ICombatant GetEnemyTarget()
        => _currentTarget ? _currentTarget.GetComponent<ICombatant>() : null;

    public void FaceTarget(Transform t)
        => _mvMgr.FaceTarget(transform, t, _startScale);

    public void TakeDamage(int dmg)
    {
        bool died = _hpMgr.TakeDamage(dmg);
        if (!died && TryGetComponent(out BounceScale b)) b.TriggerBounce();
        if (died) Die();
    }

    /* -------- Death logic -------- */
    private void Die()
    {
        gameObject.SetActive(false);
        Debug.Log("Hero died — Game Over!");
    }
}
