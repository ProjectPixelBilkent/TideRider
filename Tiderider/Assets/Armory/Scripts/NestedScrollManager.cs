using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedScrollManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static NestedScrollManager Instance { get; private set; }

    public ScrollRect horizontalScroll;
    public ScrollRect verticalScroll;
    public float horizontalThreshold = 0.5f;

    private static ScrollRect activeScrollRect;
    private static bool isDragging;
    private ScrollRect thisScrollRect;

    void Awake()
    {
        Instance = this;
        thisScrollRect = GetComponent<ScrollRect>();

        // If references aren't set in inspector, try to find them
        if (horizontalScroll == null || verticalScroll == null)
        {
            FindScrollRects();
        }
    }

    private void FindScrollRects()
    {
        ScrollRect[] allScrollRects = GetComponentsInParent<ScrollRect>(true);

        foreach (ScrollRect scroll in allScrollRects)
        {
            if (scroll != thisScrollRect)
            {
                if (horizontalScroll == null && scroll.horizontal)
                {
                    horizontalScroll = scroll;
                }
                else if (verticalScroll == null && scroll.vertical)
                {
                    verticalScroll = scroll;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only process if this is the first drag handler to receive the event
        if (isDragging) return;

        // Determine which scroll to activate based on drag direction
        Vector2 delta = eventData.delta;
        float horizontalRatio = Mathf.Abs(delta.x) / (Mathf.Abs(delta.x) + Mathf.Abs(delta.y));

        if (horizontalRatio > horizontalThreshold)
        {
            activeScrollRect = horizontalScroll;
            if (verticalScroll != null) verticalScroll.enabled = false;
        }
        else
        {
            activeScrollRect = verticalScroll;
            if (horizontalScroll != null) horizontalScroll.enabled = false;
        }

        // Pass the event to the active scroll rect if it's not this one
        if (activeScrollRect != null && activeScrollRect != thisScrollRect)
        {
            ExecuteEvents.Execute(activeScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || activeScrollRect == null) return;

        // If this is the active scroll rect, let it handle normally
        // Otherwise, forward the event to the active scroll rect
        if (activeScrollRect != thisScrollRect)
        {
            ExecuteEvents.Execute(activeScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || activeScrollRect == null) return;

        // Forward the event to the active scroll rect if it's not this one
        if (activeScrollRect != thisScrollRect)
        {
            ExecuteEvents.Execute(activeScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }

        // Re-enable both scrolls for next interaction
        if (horizontalScroll != null) horizontalScroll.enabled = true;
        if (verticalScroll != null) verticalScroll.enabled = true;

        activeScrollRect = null;
        isDragging = false;
    }

    public void LockAll()
    {
        if (horizontalScroll != null) horizontalScroll.enabled = false;
        if (verticalScroll != null) verticalScroll.enabled = false;
    }

    public void UnlockAll()
    {
        if (horizontalScroll != null) horizontalScroll.enabled = true;
        if (verticalScroll != null) verticalScroll.enabled = true;
    }

    // Helper method to check if this scroll rect should process events
    private bool ShouldProcessEvent()
    {
        return !isDragging || activeScrollRect == thisScrollRect;
    }
}