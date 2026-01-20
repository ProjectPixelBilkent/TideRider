using UnityEngine;

public class JellyfishStingArea2D : MonoBehaviour
{
    public int damage = 10;
    public float tickRate = 0.5f;

    private float nextHitTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHitTime) return;

        // ShipModel might be on the parent object (common), so we try both:
        ShipModel model = other.GetComponent<ShipModel>();
        if (model == null) model = other.GetComponentInParent<ShipModel>();
        if (model == null) return;

        // Deal damage
        model.Decrement(damage);

        // cooldown between stings
        nextHitTime = Time.time + tickRate;
    }
}
