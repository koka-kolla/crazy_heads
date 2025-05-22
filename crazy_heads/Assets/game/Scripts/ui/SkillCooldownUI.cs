using UnityEngine;
using UnityEngine.UI;

public class SkillCooldownUI : MonoBehaviour
{
    [Tooltip("Image-маска, заполняющаяся по кулдауну")]
    public Image cooldownMask;

    private float cooldownDuration = 1f;
    private float cooldownTimer = 0f;
    private bool isCooldown = false;

    void Start()
    {
        // Скрываем маску изначально
        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;
    }

    void Update()
    {
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(cooldownTimer / cooldownDuration);
            cooldownMask.fillAmount = t;

            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
                cooldownMask.fillAmount = 0f; // снова скрываем маску
            }
        }
    }

    /// <summary>
    /// Запустить анимацию кулдауна заданной длительности
    /// </summary>
    public void StartCooldown(float duration)
    {
        if (cooldownMask == null)
            return;

        cooldownDuration = duration;
        cooldownTimer = duration;
        isCooldown = true;
        cooldownMask.fillAmount = 1f; // показываем маску в полном виде
    }
}
