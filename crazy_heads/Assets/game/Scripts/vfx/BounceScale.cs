using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BounceScale : MonoBehaviour
{
    [Header("Bounce Settings")]
    public float maxXScaleMultiplier = 1.2f;
    public float maxYScaleMultiplier = 1.2f;
    public float minXScaleMultiplier = 0.8f;
    public float minYScaleMultiplier = 0.8f;
    [Tooltip("Длительность одной фазы (grow / shrink / back), сек.")]
    public float bouncingDuration    = 0.1f;

    private Vector3 defaultScale;
    private float   bottomLocalY;
    private float   baselineWorldY;
    private Coroutine bounceRoutine;

    void Awake()
    {
        Vector3 s = transform.localScale;
        defaultScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

        var col = GetComponent<BoxCollider2D>();
        bottomLocalY = col.offset.y - col.size.y * 0.5f;
    }

    /// <summary>
    /// Запускает пружинящее масштабирование от нижней точки без дрейфа.
    /// Устанавливает новую базовую позицию в момент удара, чтобы поддерживать текущий knockback.
    /// </summary>
    public void TriggerBounce()
    {
        if (!gameObject.activeInHierarchy) return;
        if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);

        // Пересчитаем текущую базовую Y, учитывая возможный knockback
        baselineWorldY = transform.position.y + bottomLocalY * transform.localScale.y;

        float sign = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(
            sign * defaultScale.x,
            defaultScale.y,
            defaultScale.z
        );
        // Восстанавливаем позицию исходя из новой базовой линии
        Vector3 pos = transform.position;
        pos.y = baselineWorldY - bottomLocalY * defaultScale.y;
        transform.position = pos;

        bounceRoutine = StartCoroutine(BounceRoutine(sign));
    }

    private IEnumerator BounceRoutine(float sign)
    {
        Vector3 start = new Vector3(sign * defaultScale.x, defaultScale.y);
        Vector3 max   = new Vector3(sign * defaultScale.x * maxXScaleMultiplier,
                                    defaultScale.y * maxYScaleMultiplier);
        Vector3 min   = new Vector3(sign * defaultScale.x * minXScaleMultiplier,
                                    defaultScale.y * minYScaleMultiplier);

        yield return ScaleStep(start, max);
        yield return ScaleStep(max,   min);
        yield return ScaleStep(min,   start);

        bounceRoutine = null;
    }

    private IEnumerator ScaleStep(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < bouncingDuration)
        {
            if (!gameObject.activeInHierarchy) yield break;
            elapsed += Time.deltaTime;
            var scale = Vector3.Lerp(from, to, elapsed / bouncingDuration);
            transform.localScale = new Vector3(scale.x, scale.y, defaultScale.z);

            // Поддерживаем текущий baselineWorldY
            Vector3 pos = transform.position;
            pos.y = baselineWorldY - bottomLocalY * scale.y;
            transform.position = pos;

            yield return null;
        }
        transform.localScale = new Vector3(to.x, to.y, defaultScale.z);
        Vector3 finalPos = transform.position;
        finalPos.y = baselineWorldY - bottomLocalY * to.y;
        transform.position = finalPos;
    }
}
