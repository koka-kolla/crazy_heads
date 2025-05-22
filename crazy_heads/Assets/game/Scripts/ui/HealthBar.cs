using UnityEngine;
using UnityEngine.UI;
using System.Collections;  // Добавляем пространство имён для IEnumerator

public class HealthBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Слайдер, который сразу показывает текущее здоровье")]
    public Slider healthBarSlider;
    [Tooltip("Слайдер, который плавно догоняет основную полоску")]
    public Slider delayBarSlider;

    [Header("Follow Settings")]
    [Tooltip("К какому объекту будет привязана полоска")]
    public Transform objectToFollow;
    [Tooltip("Смещение над объектом в мире")]
    public Vector3 offset = new Vector3(0, 1, 0);

    [Header("Delay Animation")]
    [Tooltip("Сколько секунд займёт отставшая полоска, чтобы догнать")]
    public float delayDuration = 0.5f;

    private Coroutine _delayRoutine;

    private void Start()
    {
        // Чтобы до первого SetHealth они не висели в углу
        if (healthBarSlider != null)  healthBarSlider.value  = 1f;
        if (delayBarSlider != null)  delayBarSlider.value  = 1f;

        // Позиционируем сразу же один раз
        StartCoroutine(PositionHealthBar());
    }

    private IEnumerator PositionHealthBar()
    {
        // ждём, пока всё отрендерится
        yield return new WaitForEndOfFrame();

        UpdatePosition();  // Вызываем обновление позиции
    }

    private void Update()
    {
        UpdatePosition();  // В каждое обновление позиции вызываем метод
    }

    // Публичный доступ для обновления позиции полоски здоровья
    public void UpdatePosition()
    {
        if (objectToFollow == null || healthBarSlider == null) return;
        Vector3 worldPos = objectToFollow.position + offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        healthBarSlider.transform.position = screenPos;
        if (delayBarSlider != null)
            delayBarSlider.transform.position = screenPos;
    }

    /// <summary>
    /// Устанавливает новую нормализованную жизнь (0..1)
    /// </summary>
    public void SetHealth(float health, float maxHealth)
    {
        float normalized = Mathf.Clamp01(health / maxHealth);

        // сразу основной
        if (healthBarSlider != null)
            healthBarSlider.value = normalized;

        // плавно догоняющий
        if (delayBarSlider != null)
        {
            if (_delayRoutine != null)
                StopCoroutine(_delayRoutine);
            _delayRoutine = StartCoroutine(AnimateDelayBar(delayBarSlider.value, normalized));
        }
    }

    private IEnumerator AnimateDelayBar(float from, float to)
    {
        float elapsed = 0f;
        // убираем резкий «скачок», если from < to (хил), сразу ставим
        if (to > from)
        {
            delayBarSlider.value = to;
            yield break;
        }

        while (elapsed < delayDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / delayDuration);
            delayBarSlider.value = Mathf.Lerp(from, to, t);
            yield return null;
        }

        delayBarSlider.value = to;
    }
}
