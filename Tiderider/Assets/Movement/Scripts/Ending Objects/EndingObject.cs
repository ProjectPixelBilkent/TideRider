using DG.Tweening;
using System;
using UnityEngine;

public class EndingObject : MonoBehaviour
{
    public string prefabId;
    public bool fake;

    public static event Action OnLevelCompleted;

    public SpriteRenderer SpriteRenderer { get; private set; }

    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(fake || !collision.CompareTag("Player"))
        {
            return;
        }

        BulletSpawner bulletSpawner = collision.GetComponent<BulletSpawner>();
        if(bulletSpawner==null || bulletSpawner.objectSpawner.isInEndingSequence)
        {
            return;
        }

        OnLevelCompleted?.Invoke();
    }

    private void FixedUpdate()
    {
        if(transform.parent==null)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Max(transform.position.y, Camera.main.transform.position.y + 2));
        }
    }
}
