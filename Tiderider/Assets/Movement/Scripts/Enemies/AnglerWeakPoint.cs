using UnityEngine;

public class AnglerWeakPoint : MonoBehaviour
{
    public ImitatorAngler2D owner;
    public int hp = 5;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bullet")) return;

        hp -= 1;
        Destroy(other.gameObject);

        if (hp <= 0)
        {
            if (owner != null) owner.Die();
            gameObject.SetActive(false);
        }
    }
}
