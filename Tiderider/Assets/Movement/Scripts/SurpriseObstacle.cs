using DG.Tweening;
using UnityEngine;

public class SurpriseObstacle : BigObstacle
{
    [SerializeField] SpriteRenderer surpiseSprite;
    [SerializeField] SpriteRenderer warningSprite;

    Collider2D circleCollider;

    protected override void Start()
    {
        base.Start();
        circleCollider = GetComponent<Collider2D>();
        surpiseSprite.transform.position = warningSprite.transform.position;
        surpiseSprite.enabled = false;
        warningSprite.enabled = true;
        circleCollider.enabled = false;

        warningApparent = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 1);
        warningFaded = new Color(warningSprite.color.r, warningSprite.color.g, warningSprite.color.b, 0.5f);

        DOVirtual.DelayedCall(0.4f, () =>
        {
            try
            {
                if(warningFaded != null && surpiseSprite != null)
                {
                    warningSprite.enabled = false;
                    surpiseSprite.enabled = true;
                    surpiseSprite.transform.DOMoveY(transform.position.y, 0.1f).onComplete += () =>
                    {
                        if (circleCollider != null)
                        {
                            circleCollider.enabled = true;
                        }
                    };
                }
            }
            catch { }
            
        });
    }

    private Color warningFaded;
    private Color warningApparent;

    protected override void Update()
    {
        base.Update();

        if(warningSprite != null && warningSprite.enabled)
        {
            warningSprite.color = Mathf.Floor(Time.time * 10) % 2 == 0 ? warningFaded : warningApparent;
        }
    }

}
