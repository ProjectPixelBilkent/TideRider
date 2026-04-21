using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LoopingBackground : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private int sortingOrder = -100;
    [SerializeField] private float zPosition = 10f;
    [SerializeField] private bool lockXToCamera = true;
    [SerializeField] private float verticalScrollSpeed = 0f;
    [SerializeField] private bool useUnscaledTime = false;

    private SpriteRenderer primaryRenderer;
    private SpriteRenderer secondaryRenderer;
    private Transform primaryTransform;
    private Transform secondaryTransform;
    private float tileHeight;

    private void Awake()
    {
        primaryRenderer = GetComponent<SpriteRenderer>();
        primaryTransform = primaryRenderer.transform;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        primaryRenderer.sortingOrder = sortingOrder;
        primaryTransform.position = new Vector3(primaryTransform.position.x, primaryTransform.position.y, zPosition);

        CreateSecondaryRenderer();
        FitSpritesToCamera();
        RebuildStack();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        float primaryY = primaryTransform.position.y;
        float secondaryY = secondaryTransform.position.y;

        if (!Mathf.Approximately(verticalScrollSpeed, 0f))
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float scrollDistance = verticalScrollSpeed * deltaTime;
            primaryY -= scrollDistance;
            secondaryY -= scrollDistance;
        }

        if (lockXToCamera)
        {
            float x = targetCamera.transform.position.x;
            primaryTransform.position = new Vector3(x, primaryY, zPosition);
            secondaryTransform.position = new Vector3(x, secondaryY, zPosition);
        }
        else
        {
            primaryTransform.position = new Vector3(primaryTransform.position.x, primaryY, zPosition);
            secondaryTransform.position = new Vector3(secondaryTransform.position.x, secondaryY, zPosition);
        }

        float cameraBottom = targetCamera.transform.position.y - targetCamera.orthographicSize;

        if (primaryTransform.position.y + tileHeight * 0.5f < cameraBottom)
        {
            primaryTransform.position = new Vector3(primaryTransform.position.x, secondaryTransform.position.y + tileHeight, zPosition);
            SwapTiles();
        }
    }

    private void CreateSecondaryRenderer()
    {
        GameObject secondary = new GameObject("Background Tile B");
        secondaryTransform = secondary.transform;
        secondaryTransform.SetParent(transform.parent, false);

        secondaryRenderer = secondary.AddComponent<SpriteRenderer>();
        secondaryRenderer.sprite = primaryRenderer.sprite;
        secondaryRenderer.sharedMaterial = primaryRenderer.sharedMaterial;
        secondaryRenderer.color = primaryRenderer.color;
        secondaryRenderer.flipX = primaryRenderer.flipX;
        secondaryRenderer.flipY = primaryRenderer.flipY;
        secondaryRenderer.sortingLayerID = primaryRenderer.sortingLayerID;
        secondaryRenderer.sortingOrder = sortingOrder;
        secondaryRenderer.drawMode = primaryRenderer.drawMode;
        secondaryRenderer.maskInteraction = primaryRenderer.maskInteraction;
        secondaryRenderer.spriteSortPoint = primaryRenderer.spriteSortPoint;
    }

    private void FitSpritesToCamera()
    {
        if (targetCamera == null || primaryRenderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = primaryRenderer.sprite.bounds.size;
        float targetHeight = targetCamera.orthographicSize * 2f;
        float targetWidth = targetHeight * targetCamera.aspect;

        float scale = Mathf.Max(targetWidth / spriteSize.x, targetHeight / spriteSize.y);
        Vector3 scaleVector = new Vector3(scale, scale, 1f);

        primaryTransform.localScale = scaleVector;
        secondaryTransform.localScale = scaleVector;

        tileHeight = spriteSize.y * scale;
    }

    private void RebuildStack()
    {
        float x = targetCamera != null ? targetCamera.transform.position.x : primaryTransform.position.x;
        float y = targetCamera != null ? targetCamera.transform.position.y : primaryTransform.position.y;

        primaryTransform.position = new Vector3(x, y, zPosition);
        secondaryTransform.position = new Vector3(x, y + tileHeight, zPosition);
    }

    private void SwapTiles()
    {
        (primaryRenderer, secondaryRenderer) = (secondaryRenderer, primaryRenderer);
        (primaryTransform, secondaryTransform) = (secondaryTransform, primaryTransform);
    }
}
