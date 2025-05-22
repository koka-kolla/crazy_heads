// AgroZone.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class AgroZone : MonoBehaviour
{
    [Tooltip("Ссылка на HeroController")]
    public HeroController hero;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (hero == null)
            hero = Object.FindFirstObjectByType<HeroController>();

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            hero.SetTarget(other.transform);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            hero.SetTarget(other.transform);
    }
}