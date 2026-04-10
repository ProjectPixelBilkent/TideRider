using DG.Tweening;
using UnityEngine;

public class EndingObject : MonoBehaviour
{
    public string prefabId;
    public bool fake;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Pulse(float time)
    {
        if (fake)
        {
            Color current = spriteRenderer.color;
            spriteRenderer.DOColor(new Color(0, 0, 0), time / 2).onComplete += () =>
            {
                spriteRenderer.DOColor(current, time / 2);
            };
        }
    }

    private void FixedUpdate()
    {
        if(transform.parent==null)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y, Camera.main.transform.position.y + 2));
        }
    }
}
