using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class HasHealth: MonoBehaviour   
{
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    [Header("Damage Flash")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 128f / 255f, 128f / 255f, 1f);
    [SerializeField] private float damageFlashDuration = 0.45f;
    protected Collider2D coll2d;
    public Slider healthSlider;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action HealthChanged;

    private SpriteRenderer[] damageFlashRenderers;
    private Color[] baseRendererColors;
    private Coroutine damageFlashRoutine;


    protected virtual void Start()
    {
        Restore();
       
    }
    public void Update()
    {
        if(healthSlider!=null)
        {
            healthSlider.value = currentHealth;
        }
    }
    public void Restore()
    {
        currentHealth = maxHealth;
        coll2d = GetComponent<Collider2D>();
        CacheDamageFlashRenderers();
    }

    public void SetMaxHealth(int value, bool restoreToFull = true)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = restoreToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public int TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return currentHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        HealthChanged?.Invoke();
        TriggerDamageFlash();
        if (currentHealth <= 0)
        {
            Die();
        }

        return currentHealth;
    }

    public int Heal(int amount)
    {
        if (amount <= 0)
        {
            return currentHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        HealthChanged?.Invoke();
        return currentHealth;
    }

    public virtual void Die()
    {
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void CacheDamageFlashRenderers()
    {
        damageFlashRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseRendererColors = new Color[damageFlashRenderers.Length];

        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            baseRendererColors[i] = damageFlashRenderers[i] != null ? damageFlashRenderers[i].color : Color.white;
        }
    }

    private void TriggerDamageFlash()
    {
        if (damageFlashDuration <= 0f)
        {
            return;
        }

        if (damageFlashRenderers == null || damageFlashRenderers.Length == 0)
        {
            CacheDamageFlashRenderers();
        }

        if (damageFlashRenderers == null || damageFlashRenderers.Length == 0)
        {
            return;
        }

        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            SpriteRenderer renderer = damageFlashRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            baseRendererColors[i] = renderer.color;
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }

        damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / damageFlashDuration);

            for (int i = 0; i < damageFlashRenderers.Length; i++)
            {
                SpriteRenderer renderer = damageFlashRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Color baseColor = i < baseRendererColors.Length ? baseRendererColors[i] : Color.white;
                Color flashColor = new Color(damageFlashColor.r, damageFlashColor.g, damageFlashColor.b, baseColor.a);
                renderer.color = Color.Lerp(flashColor, baseColor, t);
            }

            yield return null;
        }

        for (int i = 0; i < damageFlashRenderers.Length; i++)
        {
            SpriteRenderer renderer = damageFlashRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.color = i < baseRendererColors.Length ? baseRendererColors[i] : Color.white;
        }

        damageFlashRoutine = null;
    }
}
