using DG.Tweening;
using UnityEngine;

public class VerticalShark : MonoBehaviour
{
    [SerializeField] private SpriteRenderer shark;
    [SerializeField] private SpriteRenderer exclamationMark;

    private Camera mainCamera;
    private Collider2D sharkCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        sharkCollider = shark.GetComponent<Collider2D>();
        shark.enabled = false;
        exclamationMark.enabled = true;
        sharkCollider.enabled = false;
        DOVirtual.DelayedCall(0.6f, () =>
        {
            try
            {
                exclamationMark.enabled = false;
                shark.enabled = true;
                sharkCollider.enabled = true;
                shark.transform.position = new Vector3(exclamationMark.transform.position.x, exclamationMark.transform.position.y - 5, exclamationMark.transform.position.z);
                shark.transform.DOMoveY(SceneObjectSpawner.GetScreenBounds().y + 6, 1f).onComplete += () =>
                {
                    if(gameObject != null)
                    {
                        Destroy(gameObject);
                    }
                };
            }
            catch { }

        });
    }


    private void FixedUpdate()
    {
        transform.position = new Vector3(transform.position.x, mainCamera.transform.position.y, transform.position.z);
        exclamationMark.transform.position = new Vector3(transform.position.x, 2 * mainCamera.transform.position.y - SceneObjectSpawner.GetScreenBounds().y + 1, -6);
    }
}
