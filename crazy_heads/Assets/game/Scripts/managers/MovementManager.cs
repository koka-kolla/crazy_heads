using UnityEngine;

/// <summary>
/// Отвечает за движение персонажа к цели или возвращение на исходную позицию,
/// а также за поворот спрайта. Не трогает «bounce»-масштаб, меняет только знак X-оси.
/// </summary>
public class MovementManager
{
    private readonly Rigidbody2D _rb;
    private readonly Vector3     _originPos;
    private readonly Vector3     _startScale;
    private readonly float       _returnThreshold;

    public MovementManager(Rigidbody2D rb, Vector3 origin, Vector3 startScale, float threshold)
    {
        _rb             = rb;
        _originPos      = origin;
        _startScale     = startScale;
        _returnThreshold= threshold;
    }

    /// <summary>Двигает к цели или возвращает к точке старта.</summary>
    public void HandleMovement(Transform target, float stopRange, float speed)
    {
        Vector2 pos  = _rb.position;
        Vector2 dest = target ? (Vector2)target.position : (Vector2)_originPos;

        float dist = Vector2.Distance(pos, dest);
        Vector2 move = Vector2.zero;

        if (target)
        {
            if (dist > stopRange)
                move = (dest - pos).normalized * speed;
        }
        else
        {
            if (dist > _returnThreshold)
                move = (dest - pos).normalized * speed;
        }

        // ---------- Поворот: меняем только знак, сохраняя текущий «bounce»-scale ----------
        if (Mathf.Abs(move.x) > 0.01f)
        {
            float sign = Mathf.Sign(move.x);
            var   s    = _rb.transform.localScale;
            _rb.transform.localScale = new Vector3(
                Mathf.Abs(s.x) * sign,
                s.y,
                s.z);
        }
        // ----------------------------------------------------------------------------------

        _rb.linearVelocity = move;
    }

    /// <summary>Поворачивает спрайт к цели, сохраняя текущий масштаб (bounce).</summary>
    public void FaceTarget(Transform owner, Transform trg, Vector3 startScale)
    {
        if (!trg) return;
        float dx = trg.position.x - owner.position.x;
        if (Mathf.Abs(dx) > 0.01f)
        {
            var s = owner.localScale;
            owner.localScale = new Vector3(
                Mathf.Abs(s.x) * Mathf.Sign(dx),
                s.y,
                s.z);
        }
    }
}
