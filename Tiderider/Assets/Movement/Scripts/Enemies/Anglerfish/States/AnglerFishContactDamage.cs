using UnityEngine;

public class AnglerFishContactDamage : MonoBehaviour
{
    public int contactDamage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        HasHealth health = other.GetComponent<HasHealth>();
        if (health != null)
        {
            if (SoundLibrary.Instance != null)
                SoundLibrary.Instance.Play("anglerfish");

            health.TakeDamage(contactDamage);
        }
    }
}
